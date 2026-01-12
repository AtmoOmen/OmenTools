using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Hooking;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Text;
using InteropGenerator.Runtime;
using OmenTools.Abstracts;
using CSCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace OmenTools.Managers;

public unsafe class LogMessageManager : OmenServiceBase<LogMessageManager>
{
    #region 公开事件

    public delegate void PreLogMessageDelegate(ref bool isPrevented, ref uint logMessageID, ref LogMessageQueueItem item);

    public delegate void PostLogMessageDelegate(uint logMessageID, LogMessageQueueItem item);

    #endregion

    public LogMessageManagerConfig Config { get; private set; } = null!;

    private delegate void UpdateDelegate(RaptureLogModule* module);

    // ReSharper disable once InconsistentNaming
    private Hook<UpdateDelegate>? UpdateHook;

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> methodsCollection = [];

    private readonly HashSet<nint> seenLogMessageObjects = [];

    internal override void Init()
    {
        Config = LoadConfig<LogMessageManagerConfig>() ?? new();

        UpdateHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(RaptureLogModule.MemberFunctionPointers),
            "Update",
            (UpdateDelegate)UpdateDetour
        );

        UpdateHook?.Enable();
    }

    internal override void Uninit()
    {
        UpdateHook?.Dispose();
        UpdateHook = null;

        methodsCollection.Clear();
    }

    #region Hook

    private void UpdateDetour(RaptureLogModule* module)
    {
        foreach (ref var item in module->LogMessageQueue)
        {
            if (seenLogMessageObjects.Contains((nint)Unsafe.AsPointer(ref item)))
                continue;

            ProcessLogMessage(ref item);
        }

        UpdateHook.Original(module);

        seenLogMessageObjects.Clear();
        foreach (ref var item in module->LogMessageQueue)
            seenLogMessageObjects.Add((nint)Unsafe.AsPointer(ref item));
    }

    private void ProcessLogMessage(ref LogMessageQueueItem item)
    {
        if (item.LogMessageId == 0) return;
        
        if (!OnPreReceiveLogMessage(ref item))
        {
            item.LogMessageId = 0;
            return;
        }

        if (item.LogMessageId == 0) return;
        
        OnPostReceiveLogMessage(item);
    }

    private bool OnPreReceiveLogMessage(ref LogMessageQueueItem item)
    {
        if (Config.ShowLogMessageLog)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[Log Message Manager]");
            sb.AppendLine($"ID: {item.LogMessageId}");
            
            if (item.SourceKind != EntityRelationKind.None)
            {
                sb.AppendLine($"来源: {item.SourceKind}");
                sb.AppendLine($"\t名称: {item.SourceNameString}");
                sb.AppendLine($"\t服务器: {LuminaWrapper.GetWorldName(item.SourceHomeWorld)} ({item.SourceHomeWorld})");
                sb.AppendLine($"\t玩家: {item.SourceIsPlayer}");
                sb.AppendLine($"\t性别: {item.SourceSex}");
                sb.AppendLine($"\tObjStrID: {item.SourceObjStrId} {item.SourceObjStrId.FromObjStrID()}");
            }
            
            if (item.TargetKind != EntityRelationKind.None)
            {
                sb.AppendLine($"来源: {item.TargetKind}");
                sb.AppendLine($"\t名称: {item.TargetNameString}");
                sb.AppendLine($"\t服务器: {LuminaWrapper.GetWorldName(item.TargetHomeWorld)} ({item.TargetHomeWorld})");
                sb.AppendLine($"\t玩家: {item.TargetIsPlayer}");
                sb.AppendLine($"\t性别: {item.TargetSex}");
                sb.AppendLine($"\tObjStrID: {item.TargetObjStrId} {item.TargetObjStrId.FromObjStrID()}");
            }

            if (item.Parameters.Count > 0)
            {
                sb.AppendLine("参数:");

                for (var i = 0; i < item.Parameters.Count; i++)
                {
                    var param = item.Parameters[i];

                    switch (param.Type)
                    {
                        case TextParameterType.Uninitialized:
                            continue;
                        case TextParameterType.ReferencedUtf8String:
                            if (param.ReferencedUtf8StringValue != null && param.ReferencedUtf8StringValue->RefCount > 0)
                            {
                                sb.AppendLine($"\t[{i}] ({param.Type}):");

                                for (var d = 0; d < param.ReferencedUtf8StringValue->RefCount; d++)
                                {
                                    var utf8String = param.ReferencedUtf8StringValue[d];
                                    if (utf8String.Utf8String.IsEmpty || !utf8String.Utf8String.StringPtr.HasValue) continue;
                                    sb.AppendLine($"\t\t[{i}]: {utf8String.Utf8String.StringPtr.ExtractText()}");
                                }
                            }

                            break;
                        case TextParameterType.String:
                            if (param.StringValue.HasValue)
                            {
                                sb.AppendLine($"\t[{i}] ({param.Type}): {param.StringValue.ExtractText()}");
                            }

                            break;
                        default:
                            sb.AppendLine($"\t[{i}] ({param.Type}): {param.IntValue}");
                            break;
                    }
                }
            }
            
            Debug(sb.ToString().Trim());
        }

        var isPrevented = false;

        if (methodsCollection.TryGetValue(typeof(PreLogMessageDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PreLogMessageDelegate)preDelegate;
                preExecuteCommand(ref isPrevented, ref item.LogMessageId, ref item);
                if (isPrevented) return false;
            }
        }

        return true;
    }

    private void OnPostReceiveLogMessage(LogMessageQueueItem item)
    {
        if (methodsCollection.TryGetValue(typeof(PostLogMessageDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PostLogMessageDelegate)preDelegate;
                preExecuteCommand(item.LogMessageId, item);
            }
        }
    }

    #endregion

    #region Event

    private bool RegisterGeneric<T>(T method, params T[] methods) where T : Delegate
    {
        var type = typeof(T);

        methodsCollection.AddOrUpdate
        (
            type,
            _ =>
            {
                var list = ImmutableList.Create<Delegate>(method);
                return methods.Length > 0 ? list.AddRange(methods) : list;
            },
            (_, currentList) =>
            {
                var newList = currentList.Add(method);
                return methods.Length > 0 ? newList.AddRange(methods) : newList;
            }
        );

        return true;
    }

    private bool UnregisterGeneric<T>(params T[] methods) where T : Delegate
    {
        if (methods is not { Length: > 0 }) return false;

        var type = typeof(T);

        while (methodsCollection.TryGetValue(type, out var currentList))
        {
            var newList = currentList.RemoveRange(methods);

            if (newList == currentList)
                return false;

            if (newList.IsEmpty)
            {
                var kvp = new KeyValuePair<Type, ImmutableList<Delegate>>(type, currentList);
                if (((ICollection<KeyValuePair<Type, ImmutableList<Delegate>>>)methodsCollection).Remove(kvp))
                    return true;
            }
            else
            {
                if (methodsCollection.TryUpdate(type, newList, currentList))
                    return true;
            }
        }

        return false;
    }


    public bool RegPre(PreLogMessageDelegate method, params PreLogMessageDelegate[] methods) => RegisterGeneric(method, methods);

    public bool RegPost(PostLogMessageDelegate method, params PostLogMessageDelegate[] methods) => RegisterGeneric(method, methods);

    public bool Unreg(params PreLogMessageDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PostLogMessageDelegate[] methods) => UnregisterGeneric(methods);

    #endregion
    
    public class LogMessageManagerConfig : OmenServiceConfiguration
    {
        public bool ShowLogMessageLog;

        public void Save() =>
            this.Save(DService.Instance().GetOmenService<LogMessageManager>());
    }
}

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct LogMessageParam
{
    [FieldOffset(0)]
    public LogMessageParamType Type;

    // union field
    [FieldOffset(4)]
    public int Int;

    [FieldOffset(4)]
    public long Long;

    [FieldOffset(4)]
    public uint UInt;

    [FieldOffset(4)]
    public ulong ULong;

    [FieldOffset(4)]
    public float Float;

    /// <summary>
    ///     如果手动修改了, 一定一定要手动调用析构函数 Dtor 掉, 不然会内存泄露
    /// </summary>
    [FieldOffset(4)]
    public CStringPointer String;

    [FieldOffset(4)]
    public unsafe CSCharacter* Character;

    public void SetType(LogMessageParamType type)
    {
        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));

        Type = type;
    }

    public bool SetInt(int value)
    {
        SetType(LogMessageParamType.Int);

        Int = value;
        return true;
    }

    /// <summary>
    ///     如果手动修改了, 一定一定要手动调用析构函数 Dtor 掉, 不然会内存泄露
    /// </summary>
    public unsafe bool SetString(Utf8String* value)
    {
        if (value == null || value->IsEmpty || !value->StringPtr.HasValue)
            return false;

        return SetString(value->StringPtr);
    }

    /// <summary>
    ///     如果手动修改了, 一定一定要手动调用析构函数 Dtor 掉, 不然会内存泄露
    /// </summary>
    public bool SetString(CStringPointer value)
    {
        if (!value.HasValue)
            return false;

        SetType(LogMessageParamType.String);
        String = value;
        return true;
    }

    public unsafe bool SetCharacter(CSCharacter* value)
    {
        if (value == null) return false;

        SetType(LogMessageParamType.Character);
        Character = value;
        return true;
    }

    public unsafe bool SetCharacter(nint value)
    {
        var characterPtr = (CSCharacter*)value;
        if (characterPtr == null) return false;

        return SetCharacter(characterPtr);
    }

    public bool SetLong(long value)
    {
        SetType(LogMessageParamType.Long);
        Long = value;
        return true;
    }

    public bool SetUInt(uint value)
    {
        SetType(LogMessageParamType.UInt);
        UInt = value;
        return true;
    }

    public bool SetULong(ulong value)
    {
        SetType(LogMessageParamType.ULong);
        ULong = value;
        return true;
    }

    public bool SetFloat(float value)
    {
        SetType(LogMessageParamType.Float);
        Float = value;
        return true;
    }

    public override unsafe string ToString() =>
        Type switch
        {
            LogMessageParamType.Int       => Int.ToString(),
            LogMessageParamType.Long      => Long.ToString(),
            LogMessageParamType.UInt      => UInt.ToString(),
            LogMessageParamType.ULong     => ULong.ToString(),
            LogMessageParamType.Float     => Float.ToString(CultureInfo.InvariantCulture),
            LogMessageParamType.String    => String.ToString(),
            LogMessageParamType.Character => Character == null ? "null" : $"{Character->NameString} [{(nint)Character:X}]",
            _                             => "Undefined"
        };
}


public enum LogMessageParamType
{
    Undefined,
    Int,
    Long,
    UInt,
    ULong,
    Float,
    Bool,
    String,
    Character
}
