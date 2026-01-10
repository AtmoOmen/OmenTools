using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using FFXIVClientStructs.FFXIV.Component.Shell;
using Lumina.Text.ReadOnly;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public unsafe class ChatManager : OmenServiceBase<ChatManager>
{
    public ChatManagerConfig Config { get; private set; } = null!;

    private delegate void ProcessChatBoxEntryDelegate(
        UIModule*                          module,
        Utf8String*                        message,
        nint                               a3,
        [MarshalAs(UnmanagedType.U1)] bool saveToHistory);
    private Hook<ProcessChatBoxEntryDelegate> ProcessChatBoxEntryHook;

    private delegate void                              ExecuteCommandInnerDelegate(ShellCommandModule* module, Utf8String* message, UIModule* uiModule);
    private          Hook<ExecuteCommandInnerDelegate> ExecuteCommandInnerHook;

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> methodsCollection = [];

    #region 事件

    public delegate void PreProcessChatBoxEntryDelagte(ref bool isPrevented, ref ReadOnlySeString message, ref bool saveToHistory);

    public delegate void PostProcessChatBoxEntryDelagte(ReadOnlySeString message, bool saveToHistory);

    public delegate void PreExecuteCommandInnerDelagte(ref bool isPrevented, ref ReadOnlySeString message);

    public delegate void PostExecuteCommandInnerDelagte(ReadOnlySeString message);

    #endregion

    internal override void Init()
    {
        Config = LoadConfig<ChatManagerConfig>() ?? new();

        ProcessChatBoxEntryHook = DService.Instance().Hook.HookFromAddress<ProcessChatBoxEntryDelegate>(
            GetMemberFuncByName(typeof(UIModule.MemberFunctionPointers), "ProcessChatBoxEntry"),
            ProcessChatBoxEntryDetour);
        ProcessChatBoxEntryHook.Enable();

        ExecuteCommandInnerHook = DService.Instance().Hook.HookFromAddress<ExecuteCommandInnerDelegate>(
            GetMemberFuncByName(typeof(ShellCommandModule.MemberFunctionPointers), "ExecuteCommandInner"),
            ExecuteCommandInnerDetour);
        ExecuteCommandInnerHook.Enable();
    }

    internal override void Uninit()
    {
        ProcessChatBoxEntryHook?.Dispose();
        ProcessChatBoxEntryHook = null;

        ExecuteCommandInnerHook?.Dispose();
        ExecuteCommandInnerHook = null;

        methodsCollection.Clear();
    }

    #region 注册

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

    public bool RegPreProcessChatBoxEntry(PreProcessChatBoxEntryDelagte method, params PreProcessChatBoxEntryDelagte[] methods) =>
        RegisterGeneric(method, methods);
    public bool RegPostProcessChatBoxEntry(PostProcessChatBoxEntryDelagte method, params PostProcessChatBoxEntryDelagte[] methods) =>
        RegisterGeneric(method, methods);
    public bool RegPreExecuteCommandInner(PreExecuteCommandInnerDelagte method, params PreExecuteCommandInnerDelagte[] methods) => 
        RegisterGeneric(method, methods);
    public bool RegPostExecuteCommandInner(PostExecuteCommandInnerDelagte method, params PostExecuteCommandInnerDelagte[] methods) =>
        RegisterGeneric(method, methods);

    public bool Unreg(params PreProcessChatBoxEntryDelagte[]  methods) => UnregisterGeneric(methods);
    public bool Unreg(params PostProcessChatBoxEntryDelagte[] methods) => UnregisterGeneric(methods);
    public bool Unreg(params PreExecuteCommandInnerDelagte[]  methods) => UnregisterGeneric(methods);
    public bool Unreg(params PostExecuteCommandInnerDelagte[] methods) => UnregisterGeneric(methods);

    #endregion

    #region Invokes

    private void ProcessChatBoxEntryDetour(UIModule* module, Utf8String* message, nint a3, [MarshalAs(UnmanagedType.U1)] bool saveToHistory)
    {
        if (message == null || !message->StringPtr.HasValue)
        {
            ProcessChatBoxEntryHook.Original(module, message, a3, saveToHistory);
            return;
        }

        var isPrevented    = false;
        var stringOriginal = message->StringPtr.AsReadOnlySeString();
        var stringToModify = message->StringPtr.AsReadOnlySeString();

        if (Config.ShowProcessChatBoxEntryLog)
            Debug($"[Chat Manager] Process Chat Box Entry\n" +
                  $"消息: {stringToModify}\n"                  +
                  $"存储至历史记录: {saveToHistory}");

        if (methodsCollection.TryGetValue(typeof(PreProcessChatBoxEntryDelagte), out var preDelegates))
        {
            foreach (var postDelegate in preDelegates)
            {
                var postExecuteCommand = (PreProcessChatBoxEntryDelagte)postDelegate;
                postExecuteCommand(ref isPrevented, ref stringToModify, ref saveToHistory);
                if (isPrevented)
                    return;
            }
        }

        if (stringOriginal != stringToModify)
            message->SetString(stringToModify.ToDalamudString().EncodeWithNullTerminator());

        ProcessChatBoxEntryHook.Original(module, message, a3, saveToHistory);

        if (methodsCollection.TryGetValue(typeof(PostProcessChatBoxEntryDelagte), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postExecuteCommand = (PostProcessChatBoxEntryDelagte)postDelegate;
                postExecuteCommand(stringToModify, saveToHistory);
            }
        }
    }

    private void ExecuteCommandInnerDetour(ShellCommandModule* module, Utf8String* message, UIModule* uiModule)
    {
        if (message == null || !message->StringPtr.HasValue)
        {
            ExecuteCommandInnerHook.Original(module, message, uiModule);
            return;
        }

        var isPrevented    = false;
        var stringOriginal = message->StringPtr.AsReadOnlySeString();
        var stringToModify = message->StringPtr.AsReadOnlySeString();

        if (Config.ShowExecuteCommandInnerLog)
            Debug($"[Chat Manager] Execute Command Inner\n" +
                  $"消息: {stringToModify}");

        if (methodsCollection.TryGetValue(typeof(PreExecuteCommandInnerDelagte), out var preDelegates))
        {
            foreach (var postDelegate in preDelegates)
            {
                var postExecuteCommand = (PreExecuteCommandInnerDelagte)postDelegate;
                postExecuteCommand(ref isPrevented, ref stringToModify);
                if (isPrevented)
                    return;
            }
        }

        if (stringOriginal != stringToModify)
            message->SetString(stringToModify.ToDalamudString().EncodeWithNullTerminator());

        ExecuteCommandInnerHook.Original(module, message, uiModule);

        if (methodsCollection.TryGetValue(typeof(PostExecuteCommandInnerDelagte), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postExecuteCommand = (PostExecuteCommandInnerDelagte)postDelegate;
                postExecuteCommand(stringToModify);
            }
        }
    }

    #endregion

    #region 调用

    public void SendMessage(string message, bool saveToHistory = false)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        SendMessage(bytes, saveToHistory);
    }

    public void SendMessage(SeString message, bool saveToHistory = false) =>
        SendMessage(message.Encode(), saveToHistory);

    public void SendMessage(ReadOnlySpan<byte> message, bool saveToHistory = false)
    {
        if (message.Length == 0) return;

        using var builder = new RentedSeStringBuilder();
        message = builder.Builder.Append(message).ToReadOnlySeString().ToDalamudString().EncodeWithNullTerminator();

        var utf8String = Utf8String.FromSequence(message);

        try
        {
            ProcessChatBoxEntryDetour(UIModule.Instance(), utf8String, (nint)utf8String, saveToHistory);
            // 避免折叠
        }
        finally
        {
            utf8String->Dtor(true);
        }
    }

    public void SendCommand(string command)
    {
        var bytes = Encoding.UTF8.GetBytes(command);
        SendCommand(bytes);
    }

    public void SendCommand(SeString command) =>
        SendCommand(command.Encode());

    public void SendCommand(ReadOnlySpan<byte> command)
    {
        if (command.Length == 0) return;

        using var builder = new RentedSeStringBuilder();
        command = builder.Builder.Append(command).ToReadOnlySeString().ToDalamudString().EncodeWithNullTerminator();

        var utf8String = Utf8String.FromSequence(command);

        try
        {
            ExecuteCommandInnerDetour((ShellCommandModule*)RaptureShellModule.Instance(), utf8String, UIModule.Instance());
            // 避免折叠
        }
        finally
        {
            utf8String->Dtor(true);
        }
    }

    public static string SanitiseText(string text)
    {
        using var utf8String = new Utf8String();
        utf8String.SetString(text);
        utf8String.SanitizeString((AllowedEntities)0x27F);
        var sanitised = utf8String.ToString();
        return sanitised;
    }

    #endregion

    public class ChatManagerConfig : OmenServiceConfiguration
    {
        public bool ShowProcessChatBoxEntryLog;
        public bool ShowExecuteCommandInnerLog;

        public void Save() =>
            this.Save(DService.Instance().GetOmenService<ChatManager>());
    }
}
