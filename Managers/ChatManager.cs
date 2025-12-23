using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Hooking;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using FFXIVClientStructs.FFXIV.Component.Shell;
using InteropGenerator.Runtime;
using Lumina.Text.ReadOnly;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public unsafe class ChatManager : OmenServiceBase
{
    public static ChatManagerConfig Config { get; private set; } = null!;

    private delegate void ProcessChatBoxEntryDelegate(
        UIModule*                          module,
        Utf8String*                        message,
        nint                               a3,
        [MarshalAs(UnmanagedType.U1)] bool saveToHistory);
    private static readonly CompSig                      ProcessChatBoxEntrySig = new("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B F2 48 8B F9 45 84 C9");
    private static          Hook<ProcessChatBoxEntryDelegate> ProcessChatBoxEntryHook;
    
    private delegate        void                              ExecuteCommandInnerDelegate(ShellCommandModule* module, Utf8String* message, UIModule* uiModule);
    private static readonly CompSig                           ExecuteCommandInnerSig = new("E8 ?? ?? ?? ?? FE 87 ?? ?? ?? ?? C7 87 ?? ?? ?? ?? ?? ?? ?? ??");
    private static          Hook<ExecuteCommandInnerDelegate> ExecuteCommandInnerHook;
    
    private static readonly ConcurrentDictionary<Type, ConcurrentBag<Delegate>> MethodsCollection = [];

    #region 事件

    public delegate void PreProcessChatBoxEntryDelagte(ref bool          isPrevented, ref ReadOnlySeString message, ref bool saveToHistory);
    public delegate void PostProcessChatBoxEntryDelagte(ReadOnlySeString message,     bool                 saveToHistory);
    
    public delegate void PreExecuteCommandInnerDelagte(ref bool          isPrevented, ref ReadOnlySeString message);
    public delegate void PostExecuteCommandInnerDelagte(ReadOnlySeString message);

    #endregion
    
    internal override void Init()
    {
        Config = LoadConfig<ChatManagerConfig>() ?? new();
        
        ProcessChatBoxEntryHook = ProcessChatBoxEntrySig.GetHook<ProcessChatBoxEntryDelegate>(ProcessChatBoxEntryDetour);
        ProcessChatBoxEntryHook.Enable();
        
        ExecuteCommandInnerHook = ExecuteCommandInnerSig.GetHook<ExecuteCommandInnerDelegate>(ExecuteCommandInnerDetour);
        ExecuteCommandInnerHook.Enable();
    }
    
    internal override void Uninit()
    {
        ProcessChatBoxEntryHook?.Dispose();
        ProcessChatBoxEntryHook = null;

        ExecuteCommandInnerHook?.Dispose();
        ExecuteCommandInnerHook = null;

        MethodsCollection.Clear();
    }

    #region 注册

    private static bool RegisterGeneric<T>(params T[] methods) where T : Delegate
    {
        var type = typeof(T);
        var bag  = MethodsCollection.GetOrAdd(type, _ => []);
        foreach (var method in methods)
            bag.Add(method);

        return true;
    }

    private static bool UnregisterGeneric<T>(params T[] methods) where T : Delegate
    {
        var type = typeof(T);
        if (MethodsCollection.TryGetValue(type, out var bag))
        {
            foreach (var method in methods)
            {
                var newBag = new ConcurrentBag<Delegate>(bag.Where(d => d != method));
                MethodsCollection[type] = newBag;
            }
            return true;
        }

        return false;
    }
    
    public static bool RegPreProcessChatBoxEntry(PreProcessChatBoxEntryDelagte            methods) => RegisterGeneric(methods);
    public static bool RegPreProcessChatBoxEntry(params PreProcessChatBoxEntryDelagte[]   methods) => RegisterGeneric(methods);
    public static bool RegPostProcessChatBoxEntryDelagte(PostProcessChatBoxEntryDelagte   methods) => RegisterGeneric(methods);
    public static bool RegPostProcessChatBoxEntry(params PostProcessChatBoxEntryDelagte[] methods) => RegisterGeneric(methods);
    public static bool RegPreExecuteCommandInner(PreExecuteCommandInnerDelagte            methods) => RegisterGeneric(methods);
    public static bool RegPreExecuteCommandInner(params PreExecuteCommandInnerDelagte[]   methods) => RegisterGeneric(methods);
    public static bool PostExecuteCommandInner(PostExecuteCommandInnerDelagte             methods) => RegisterGeneric(methods);
    public static bool PostExecuteCommandInner(params PostExecuteCommandInnerDelagte[]    methods) => RegisterGeneric(methods);
    
    public static bool Unreg(params PreProcessChatBoxEntryDelagte[]  methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PostProcessChatBoxEntryDelagte[] methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PreExecuteCommandInnerDelagte[]  methods) => UnregisterGeneric(methods);
    public static bool Unreg(params PostExecuteCommandInnerDelagte[] methods) => UnregisterGeneric(methods);

    #endregion

    #region Invokes
    
    private static void ProcessChatBoxEntryDetour(UIModule* module, Utf8String* message, nint a3, [MarshalAs(UnmanagedType.U1)] bool saveToHistory)
    {
        if (message == null || !message->StringPtr.HasValue)
        {
            ProcessChatBoxEntryHook.Original(module, message, a3, saveToHistory);
            return;
        }
        
        var isPrevented    = false;
        var stringToModify = message->StringPtr.AsReadOnlySeString();
        
        if (Config.ShowProcessChatBoxEntryLog)
            Debug($"[Chat Manager] Process Chat Box Entry\n" +
                  $"消息: {stringToModify}\n"                  +
                  $"存储至历史记录: {saveToHistory}");

        if (MethodsCollection.TryGetValue(typeof(PreProcessChatBoxEntryDelagte), out var preDelegates))
        {
            foreach (var postDelegate in preDelegates)
            {
                var postExecuteCommand = (PreProcessChatBoxEntryDelagte)postDelegate;
                postExecuteCommand(ref isPrevented, ref stringToModify, ref saveToHistory);
                if (isPrevented)
                    return;
            }
        }

        message->SetString(stringToModify.ToDalamudString().EncodeWithNullTerminator());
        
        ProcessChatBoxEntryHook.Original(module, message, a3, saveToHistory);

        if (MethodsCollection.TryGetValue(typeof(PostProcessChatBoxEntryDelagte), out var postDelegates))
        {
            foreach (var postDelegate in postDelegates)
            {
                var postExecuteCommand = (PostProcessChatBoxEntryDelagte)postDelegate;
                postExecuteCommand(stringToModify, saveToHistory);
            }
        }
    }

    private static void ExecuteCommandInnerDetour(ShellCommandModule* module, Utf8String* message, UIModule* uiModule)
    {
        if (message == null || !message->StringPtr.HasValue)
        {
            ExecuteCommandInnerHook.Original(module, message, uiModule);
            return;
        }

        var isPrevented    = false;
        var stringToModify = message->StringPtr.AsReadOnlySeString();

        if (Config.ShowExecuteCommandInnerLog)
            Debug($"[Chat Manager] Execute Command Inner\n" +
                  $"消息: {stringToModify}");

        if (MethodsCollection.TryGetValue(typeof(PreExecuteCommandInnerDelagte), out var preDelegates))
        {
            foreach (var postDelegate in preDelegates)
            {
                var postExecuteCommand = (PreExecuteCommandInnerDelagte)postDelegate;
                postExecuteCommand(ref isPrevented, ref stringToModify);
                if (isPrevented)
                    return;
            }
        }

        message->SetString(stringToModify.ToDalamudString().EncodeWithNullTerminator());

        ExecuteCommandInnerHook.Original(module, message, uiModule);

        if (MethodsCollection.TryGetValue(typeof(PostExecuteCommandInnerDelagte), out var postDelegates))
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

    public static void SendMessage(string message, bool saveToHistory = false)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        SendMessage(bytes, saveToHistory);
    }
    
    public static void SendMessage(ReadOnlySpan<byte> message, bool saveToHistory = false)
    {
        if (message.Length == 0) return;

        using var builder    = new RentedSeStringBuilder();
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

    public static void SendCommand(string command)
    {
        var bytes = Encoding.UTF8.GetBytes(command);
        SendCommand(bytes);
    }
    
    public static void SendCommand(ReadOnlySpan<byte> command)
    {
        if (command.Length == 0) return;
        
        using var builder    = new RentedSeStringBuilder();
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
            this.Save(DService.GetOmenService<ChatManager>());
    }
}
