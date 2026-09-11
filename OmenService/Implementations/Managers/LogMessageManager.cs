using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Dalamud.Hooking;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler.Clip;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Text;
using OmenTools.Dalamud;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public unsafe class LogMessageManager : OmenServiceBase<LogMessageManager>
{
    #region 公开事件

    public delegate void PreLogMessageDelegate(ref bool isPrevented, ref uint logMessageID, ref LogMessageQueueItem item);

    public delegate void PostLogMessageDelegate(uint logMessageID, LogMessageQueueItem item);

    public delegate void PreInstanceContentTextDelegate(ref bool isPrevented, ref uint rowID);

    public delegate void PostInstanceContentTextDelegate(uint rowID);

    #endregion

    public LogMessageManagerConfig Config { get; private set; } = null!;

    [ThreadStatic]
    private static StringBuilder? LogMessageDebugBuilder;

    private Utf8String* EmptyInstanceContentText;
    
    private Hook<InstanceContentDirector.Delegates.GetInstanceContentText>? GetInstanceContentTextHook;

    private Hook<InstanceContentTextClip.Delegates.ResolveText>? ResolveTextHook;

    private delegate void                  UpdateDelegate(RaptureLogModule* module);
    private          Hook<UpdateDelegate>? UpdateHook;

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> methodsCollection = [];

    private readonly HashSet<nint> seenLogMessageObjects = [];

    protected override void Init()
    {
        Config = LoadConfig<LogMessageManagerConfig>() ?? new();

        UpdateHook ??= IGameInteropProvider.Instance().HookFromMemberFunction
        (
            typeof(RaptureLogModule.MemberFunctionPointers),
            "Update",
            (UpdateDelegate)UpdateDetour
        );
        UpdateHook?.Enable();

        if (EmptyInstanceContentText == null)
            EmptyInstanceContentText = Utf8String.CreateEmpty();
        
        GetInstanceContentTextHook = InstanceContentDirector.StaticVirtualTablePointer->HookVFuncFromName
        (
            "GetInstanceContentText",
            (InstanceContentDirector.Delegates.GetInstanceContentText)GetInstanceContentTextDetour
        );
        GetInstanceContentTextHook.Enable();
        
        ResolveTextHook = InstanceContentTextClip.StaticVirtualTablePointer->HookVFuncFromName
        (
            "ResolveText",
            (InstanceContentTextClip.Delegates.ResolveText)ResolveInstanceContentTextClipDetour
        );
        ResolveTextHook.Enable();
    }

    protected override void Uninit()
    {
        UpdateHook?.Dispose();
        UpdateHook = null;

        GetInstanceContentTextHook?.Dispose();
        GetInstanceContentTextHook = null;

        ResolveTextHook?.Dispose();
        ResolveTextHook = null;

        if (EmptyInstanceContentText != null)
        {
            EmptyInstanceContentText->Dtor(true);
            EmptyInstanceContentText = null;
        }

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

    private Utf8String* GetInstanceContentTextDetour(InstanceContentDirector* director, uint rowID)
    {
        if (!OnPreInstanceContentText(ref rowID))
            return EmptyInstanceContentText;

        var result = GetInstanceContentTextHook.Original(director, rowID);
        OnPostInstanceContentText(rowID);
        return result;
    }

    private bool ResolveInstanceContentTextClipDetour(InstanceContentTextClip* clip)
    {
        if (clip == null || clip->Data == null)
            return ResolveTextHook.Original(clip);

        var rowID = clip->TypedData->RowId;
        if (!OnPreInstanceContentText(ref rowID))
            return true;

        clip->TypedData->RowId = rowID;

        var result = ResolveTextHook.Original(clip);
        if (result)
            OnPostInstanceContentText(rowID);

        return result;
    }

    private bool OnPreInstanceContentText(ref uint rowID)
    {
        if (Config.ShowInstanceContentTextLog)
        {
            DLog.Debug
            (
                "[Log Message Manager] Instance Content Text\n" +
                $"ID: {rowID}"
            );
        }

        var isPrevented = false;

        if (methodsCollection.TryGetValue(typeof(PreInstanceContentTextDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preInstanceContentText = (PreInstanceContentTextDelegate)preDelegate;
                preInstanceContentText(ref isPrevented, ref rowID);
                if (isPrevented) return false;
            }
        }

        return true;
    }

    private void OnPostInstanceContentText(uint rowID)
    {
        if (methodsCollection.TryGetValue(typeof(PostInstanceContentTextDelegate), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postInstanceContentText = (PostInstanceContentTextDelegate)postDelegate;
                postInstanceContentText(rowID);
            }
        }
    }

    private bool OnPreReceiveLogMessage(ref LogMessageQueueItem item)
    {
        if (Config.ShowLogMessageLog)
        {
            var sb = RentLogMessageDebugBuilder();

            try
            {
                sb.AppendLine("[Log Message Manager] Log Message");
                sb.Append("ID: ").Append(item.LogMessageId).AppendLine();
                sb.AppendLine("预览:");
                sb.Append('\t').Append(item.ToReadOnlySeString()).AppendLine();

                if (item.SourceKind != EntityRelationKind.None)
                {
                    sb.AppendLine("来源:");
                    sb.Append("\t分类: ").Append(item.SourceKind).AppendLine();
                    sb.Append("\t名称: ").Append(item.SourceNameString).AppendLine();
                    sb.Append("\t服务器: ").Append(LuminaWrapper.GetWorldName(item.SourceHomeWorld)).Append(" (").Append(item.SourceHomeWorld).AppendLine(")");
                    sb.Append("\t玩家: ").Append(item.SourceIsPlayer).AppendLine();
                    sb.Append("\t性别: ").Append(item.SourceSex).AppendLine();
                    sb.Append("\tObjStrID: ").Append(item.SourceObjStrId).Append(' ').Append(item.SourceObjStrId.FromObjStrID()).AppendLine();
                }

                if (item.TargetKind != EntityRelationKind.None)
                {
                    sb.AppendLine("目标:");
                    sb.Append("\t分类: ").Append(item.TargetKind).AppendLine();
                    sb.Append("\t名称: ").Append(item.TargetNameString).AppendLine();
                    sb.Append("\t服务器: ").Append(LuminaWrapper.GetWorldName(item.TargetHomeWorld)).Append(" (").Append(item.TargetHomeWorld).AppendLine(")");
                    sb.Append("\t玩家: ").Append(item.TargetIsPlayer).AppendLine();
                    sb.Append("\t性别: ").Append(item.TargetSex).AppendLine();
                    sb.Append("\tObjStrID: ").Append(item.TargetObjStrId).Append(' ').Append(item.TargetObjStrId.FromObjStrID()).AppendLine();
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
                                    sb.Append("\t[").Append(i).Append("] (").Append(param.Type).AppendLine("):");

                                    for (var d = 0; d < param.ReferencedUtf8StringValue->RefCount; d++)
                                    {
                                        var utf8String = param.ReferencedUtf8StringValue[d];
                                        if (utf8String.Utf8String.IsEmpty || !utf8String.Utf8String.StringPtr.HasValue) continue;
                                        sb.Append("\t\t[").Append(i).Append("]: ").Append(utf8String.Utf8String.StringPtr.ExtractText()).AppendLine();
                                    }
                                }

                                break;
                            case TextParameterType.String:
                                if (param.StringValue.HasValue)
                                    sb.Append("\t[").Append(i).Append("] (").Append(param.Type).Append("): ").Append(param.StringValue.ExtractText()).AppendLine();

                                break;
                            default:
                                sb.Append("\t[").Append(i).Append("] (").Append(param.Type).Append("): ").Append(param.IntValue).AppendLine();
                                break;
                        }
                    }
                }

                TrimEndingNewLine(sb);
                DLog.Debug(sb.ToString());
            }
            finally
            {
                ReturnLogMessageDebugBuilder(sb);
            }
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

    private static StringBuilder RentLogMessageDebugBuilder()
    {
        var sb = LogMessageDebugBuilder;
        if (sb == null)
            return new(1024);

        LogMessageDebugBuilder = null;
        sb.Clear();
        return sb;
    }

    private static void ReturnLogMessageDebugBuilder(StringBuilder sb)
    {
        if (sb.Capacity > 32 * 1024)
            return;

        sb.Clear();
        LogMessageDebugBuilder = sb;
    }

    private static void TrimEndingNewLine(StringBuilder sb)
    {
        if (sb.Length == 0)
            return;

        var end = sb.Length - 1;
        if (sb[end] != '\n')
            return;

        sb.Length = end;
        if (sb.Length > 0 && sb[^1] == '\r')
            sb.Length--;
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

    public bool RegPreInstanceContentText(PreInstanceContentTextDelegate method, params PreInstanceContentTextDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool RegPostInstanceContentText(PostInstanceContentTextDelegate method, params PostInstanceContentTextDelegate[] methods) =>
        RegisterGeneric(method, methods);

    public bool Unreg(params PreLogMessageDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PostLogMessageDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PreInstanceContentTextDelegate[] methods) => UnregisterGeneric(methods);

    public bool Unreg(params PostInstanceContentTextDelegate[] methods) => UnregisterGeneric(methods);

    #endregion

    public class LogMessageManagerConfig : OmenServiceConfig
    {
        public bool ShowLogMessageLog;
        public bool ShowInstanceContentTextLog;

        public void Save() =>
            this.Save(DService.Instance().GetOmenService<LogMessageManager>());
    }
}
