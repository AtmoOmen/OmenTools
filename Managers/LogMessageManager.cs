using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using InteropGenerator.Runtime;
using OmenTools.Abstracts;
using CSCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace OmenTools.Managers;

public unsafe class LogMessageManager : OmenServiceBase<LogMessageManager>
{
    #region 公开事件

    /// <summary>
    ///     严禁加长或缩减数组，严禁将某个值类型设为 Undefined <br />
    ///     如有阻止需求请设置 isPrevented 而不是修改参数
    /// </summary>
    public delegate void PreLogMessageDelegate(ref bool isPrevented, ref uint logMessageID, ref Span<LogMessageParam> values);

    public delegate void PostLogMessageDelegate(uint logMessageID, Span<LogMessageParam> values);

    #endregion

    public LogMessageManagerConfig Config { get; private set; } = null!;

    private delegate void ShowLogMessageDelegate(RaptureLogModule* module, uint logMessageID);
    private Hook<ShowLogMessageDelegate>? ShowLogMessageHook;

    private delegate void ShowLog2Message2Delegate(RaptureLogModule* module, uint logMessageID, CSCharacter* sourceObject, CSCharacter* targetObject);
    private Hook<ShowLog2Message2Delegate>? ShowLogMessage2Hook;

    private delegate void ShowLogMessageUIntDelegate(RaptureLogModule* module, uint logMessageID, uint value);
    private Hook<ShowLogMessageUIntDelegate>? ShowLogMessageUIntHook;

    private delegate void ShowLogMessageUInt2Delegate(RaptureLogModule* module, uint logMessageID, uint value1, uint value2);
    private Hook<ShowLogMessageUInt2Delegate>? ShowLogMessageUInt2Hook;

    private delegate void ShowLogMessageUInt3Delegate(RaptureLogModule* module, uint logMessageID, uint value1, uint value2, uint value3);
    private Hook<ShowLogMessageUInt3Delegate>? ShowLogMessageUInt3Hook;

    private delegate void ShowLogMessageStringDelegate(RaptureLogModule* module, uint logMessageID, Utf8String* value);
    private Hook<ShowLogMessageStringDelegate>? ShowLogMessageStringHook;

    private static readonly CompSig BattleLogAddLogMessageSig = new("E8 ?? ?? ?? ?? EB 08 85 F6");
    private delegate void BattleLogAddLogMessageDelegate
    (
        CSCharacter* sourceObject,
        CSCharacter* targetObject,
        uint         logMessageID,
        byte         a4,
        int          a5,
        int          a6,
        int          a7,
        int          a8
    );
    private Hook<BattleLogAddLogMessageDelegate>? BattleLogAddLogMessageHook;

    private static readonly CompSig ProcessSystemLogMessageSig =
        new("40 55 56 41 54 41 55 41 57 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 FF");
    private delegate void ProcessSystemLogMessageDelegate(EventFramework* framework, uint eventID, uint logMessageID, void* a4, ushort a5);
    private Hook<ProcessSystemLogMessageDelegate>? ProcessSystemLogMessageHook;

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> methodsCollection = [];

    internal override void Init()
    {
        Config = LoadConfig<LogMessageManagerConfig>() ?? new();

        ShowLogMessageHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(RaptureLogModule.MemberFunctionPointers),
            "ShowLogMessage",
            (ShowLogMessageDelegate)ShowLogMessageDetour
        );

        ShowLogMessage2Hook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(RaptureLogModule.MemberFunctionPointers),
            "ShowLogMessageSourceObjectTargetObject",
            (ShowLog2Message2Delegate)ShowLogMessage2Detour
        );

        ShowLogMessageUIntHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(RaptureLogModule.MemberFunctionPointers),
            "ShowLogMessageUInt",
            (ShowLogMessageUIntDelegate)ShowLogMessageUIntDetour
        );

        ShowLogMessageUInt2Hook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(RaptureLogModule.MemberFunctionPointers),
            "ShowLogMessageUIntUInt",
            (ShowLogMessageUInt2Delegate)ShowLogMessageUInt2Detour
        );

        ShowLogMessageUInt3Hook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(RaptureLogModule.MemberFunctionPointers),
            "ShowLogMessageUIntUIntUInt",
            (ShowLogMessageUInt3Delegate)ShowLogMessageUInt3Detour
        );

        ShowLogMessageStringHook ??= DService.Instance().Hook.HookFromMemberFunction
        (
            typeof(RaptureLogModule.MemberFunctionPointers),
            "ShowLogMessageString",
            (ShowLogMessageStringDelegate)ShowLogMessageStringDetour
        );

        BattleLogAddLogMessageHook  ??= BattleLogAddLogMessageSig.GetHook<BattleLogAddLogMessageDelegate>(BattleLogAddLogMessageDetour);
        ProcessSystemLogMessageHook ??= ProcessSystemLogMessageSig.GetHook<ProcessSystemLogMessageDelegate>(ProcessSystemLogMessageDetour);

        ShowLogMessageHook?.Enable();
        ShowLogMessage2Hook?.Enable();
        ShowLogMessageUIntHook?.Enable();
        ShowLogMessageUInt2Hook?.Enable();
        ShowLogMessageUInt3Hook?.Enable();
        ShowLogMessageStringHook?.Enable();
        BattleLogAddLogMessageHook?.Enable();
        ProcessSystemLogMessageHook?.Enable();
    }

    internal override void Uninit()
    {
        ShowLogMessageHook?.Dispose();
        ShowLogMessage2Hook?.Dispose();
        ShowLogMessageUIntHook?.Dispose();
        ShowLogMessageUInt2Hook?.Dispose();
        ShowLogMessageUInt3Hook?.Dispose();
        ShowLogMessageStringHook?.Dispose();
        BattleLogAddLogMessageHook?.Dispose();
        ProcessSystemLogMessageHook?.Dispose();

        ShowLogMessageHook          = null;
        ShowLogMessageUIntHook      = null;
        ShowLogMessageUInt2Hook     = null;
        ShowLogMessageUInt3Hook     = null;
        ShowLogMessageStringHook    = null;
        BattleLogAddLogMessageHook  = null;
        ProcessSystemLogMessageHook = null;

        methodsCollection.Clear();
    }

    #region Hook

    private void ShowLogMessageDetour(RaptureLogModule* module, uint logMessageID)
    {
        Span<LogMessageParam> values = stackalloc LogMessageParam[0];
        if (!OnPreReceiveLogMessage(ref logMessageID, ref values)) return;
        ShowLogMessageHook.Original(module, logMessageID);
        OnPostReceiveLogMessage(logMessageID, values);
    }

    private void ShowLogMessage2Detour(RaptureLogModule* module, uint logMessageID, CSCharacter* localPlayer, CSCharacter* targetObject)
    {
        Span<LogMessageParam> values = stackalloc LogMessageParam[2];
        values[0].SetCharacter(localPlayer);
        values[1].SetCharacter(targetObject);

        if (!OnPreReceiveLogMessage(ref logMessageID, ref values)) return;

        localPlayer  = values[0].Character;
        targetObject = values[1].Character;

        ShowLogMessage2Hook.Original(module, logMessageID, localPlayer, targetObject);
        OnPostReceiveLogMessage(logMessageID, values);
    }

    private void ShowLogMessageUIntDetour(RaptureLogModule* module, uint logMessageID, uint value)
    {
        Span<LogMessageParam> values = stackalloc LogMessageParam[1];
        values[0].SetUInt(value);

        if (!OnPreReceiveLogMessage(ref logMessageID, ref values)) return;

        value = values[0].UInt;

        ShowLogMessageUIntHook.Original(module, logMessageID, value);
        OnPostReceiveLogMessage(logMessageID, values);
    }

    private void ShowLogMessageUInt2Detour(RaptureLogModule* module, uint logMessageID, uint value1, uint value2)
    {
        Span<LogMessageParam> values = stackalloc LogMessageParam[2];
        values[0].SetUInt(value1);
        values[1].SetUInt(value2);

        if (!OnPreReceiveLogMessage(ref logMessageID, ref values)) return;

        value1 = values[0].UInt;
        value2 = values[1].UInt;

        ShowLogMessageUInt2Hook.Original(module, logMessageID, value1, value2);
        OnPostReceiveLogMessage(logMessageID, values);
    }

    private void ShowLogMessageUInt3Detour
    (
        RaptureLogModule* module,
        uint              logMessageID,
        uint              value1,
        uint              value2,
        uint              value3
    )
    {
        Span<LogMessageParam> values = stackalloc LogMessageParam[3];
        values[0].SetUInt(value1);
        values[1].SetUInt(value2);
        values[2].SetUInt(value3);

        if (!OnPreReceiveLogMessage(ref logMessageID, ref values)) return;

        value1 = values[0].UInt;
        value2 = values[1].UInt;
        value3 = values[2].UInt;

        ShowLogMessageUInt3Hook.Original(module, logMessageID, value1, value2, value3);
        OnPostReceiveLogMessage(logMessageID, values);
    }

    private void ShowLogMessageStringDetour(RaptureLogModule* module, uint logMessageID, Utf8String* value)
    {
        Span<LogMessageParam> values = stackalloc LogMessageParam[1];
        values[0].SetString(value);

        var origStringPtr = value->StringPtr.Value;

        if (!OnPreReceiveLogMessage(ref logMessageID, ref values)) return;

        var hasChanged = values[0].String.Value != origStringPtr;

        if (hasChanged)
        {
            fixed (byte** stringPtr = &values[0].String.Value)
                ShowLogMessageStringHook.Original(module, logMessageID, (Utf8String*)stringPtr);
        }
        else
            ShowLogMessageStringHook.Original(module, logMessageID, value);

        OnPostReceiveLogMessage(logMessageID, values);
    }

    private void BattleLogAddLogMessageDetour
    (
        CSCharacter* sourceObject,
        CSCharacter* targetObject,
        uint         logMessageID,
        byte         a4,
        int          a5,
        int          a6,
        int          a7,
        int          a8
    )
    {
        Span<LogMessageParam> values = stackalloc LogMessageParam[7];
        values[0].SetCharacter(sourceObject);
        values[1].SetCharacter(targetObject);
        values[2].SetInt(a4);
        values[3].SetInt(a5);
        values[4].SetInt(a6);
        values[5].SetInt(a7);
        values[6].SetInt(a8);

        if (!OnPreReceiveLogMessage(ref logMessageID, ref values)) return;

        sourceObject = values[0].Character;
        targetObject = values[1].Character;
        a4           = (byte)values[2].Int;
        a5           = values[3].Int;
        a6           = values[4].Int;
        a7           = values[5].Int;
        a8           = values[6].Int;

        BattleLogAddLogMessageHook.Original(sourceObject, targetObject, logMessageID, a4, a5, a6, a7, a8);

        OnPostReceiveLogMessage(logMessageID, values);
    }

    private void ProcessSystemLogMessageDetour(EventFramework* framework, uint eventID, uint logMessageID, void* a4, ushort a5)
    {
        Span<LogMessageParam> values = stackalloc LogMessageParam[2];
        values[0].SetUInt(eventID);
        values[1].SetUInt(a5);

        if (!OnPreReceiveLogMessage(ref logMessageID, ref values)) return;

        eventID = values[0].UInt;
        a5      = (ushort)values[1].UInt;

        ProcessSystemLogMessageHook.Original(framework, eventID, logMessageID, a4, a5);
        OnPostReceiveLogMessage(logMessageID, values);
    }
    
    private bool OnPreReceiveLogMessage(ref uint logMessageID, ref Span<LogMessageParam> values)
    {
        if (Config.ShowLogMessageLog)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[Log Message Manager]");
            sb.AppendLine($"ID: {logMessageID}");
            for (var i = 0; i < values.Length; i++)
                sb.AppendLine($"Param[{i}] ({values[i].Type}): {values[i]}");

            Debug(sb.ToString());
        }

        var isPrevented = false;

        if (methodsCollection.TryGetValue(typeof(PreLogMessageDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PreLogMessageDelegate)preDelegate;
                preExecuteCommand(ref isPrevented, ref logMessageID, ref values);
                if (isPrevented) return false;
            }
        }

        return true;
    }

    private void OnPostReceiveLogMessage(uint logMessageID, Span<LogMessageParam> values)
    {
        if (methodsCollection.TryGetValue(typeof(PostLogMessageDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PostLogMessageDelegate)preDelegate;
                preExecuteCommand(logMessageID, values);
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


    public bool RegPre(PreLogMessageDelegate          method, params PreLogMessageDelegate[]  methods) => RegisterGeneric(method, methods);
    public bool RegPost(PostLogMessageDelegate        method, params PostLogMessageDelegate[] methods) => RegisterGeneric(method, methods);
    public bool Unreg(params PreLogMessageDelegate[]  methods) => UnregisterGeneric(methods);
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

    [FieldOffset(4)]
    public bool Bool;

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

    public bool SetBool(bool value)
    {
        SetType(LogMessageParamType.Bool);
        Bool = value;
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
            LogMessageParamType.Bool      => Bool.ToString(),
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
