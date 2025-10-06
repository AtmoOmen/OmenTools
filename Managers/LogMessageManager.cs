using System.Collections.Concurrent;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using OmenTools.Abstracts;
using Character = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace OmenTools.Managers;

public unsafe class LogMessageManager : OmenServiceBase
{
    public static LogMessageManagerConfig Config { get; private set; } = null!;
    
    private static readonly CompSig                       ShowMessageSig = new("E8 ?? ?? ?? ?? 33 C0 EB ?? 73");
    private delegate        void                          ShowLogMessageDelegate(RaptureLogModule* module, uint logMessageID);
    private static          Hook<ShowLogMessageDelegate>? ShowLogMessageHook;
    
    private static readonly CompSig ShowMessage2Sig = new("E8 ?? ?? ?? ?? 32 C0 EB 59"); 
    private delegate        void ShowLog2Message2Delegate(RaptureLogModule* module, uint logMessageID, Character* localPlayer, Character* targetObject);
    private static          Hook<ShowLog2Message2Delegate>? ShowLogMessage2Hook;

    private static readonly CompSig                           ShowMessageUIntSig = new("E8 ?? ?? ?? ?? C6 43 29 01");
    private delegate        void                              ShowLogMessageUIntDelegate(RaptureLogModule* module, uint logMessageID, uint value);
    private static          Hook<ShowLogMessageUIntDelegate>? ShowLogMessageUIntHook;

    private static readonly CompSig                            ShowMessageUInt2Sig = new("E8 ?? ?? ?? ?? 0F BE 4B 44 ?? ?? ?? ?? ?? ?? ??");
    private delegate        void                               ShowLogMessageUInt2Delegate(RaptureLogModule* module, uint logMessageID, uint value1, uint value2);
    private static          Hook<ShowLogMessageUInt2Delegate>? ShowLogMessageUInt2Hook;

    private static readonly CompSig ShowMessageUInt3Sig = new("E8 ?? ?? ?? ?? 40 84 ED 0F 84 ?? ?? ?? ?? 83 7F 20 00 ?? ?? ?? ?? ?? ??");
    private delegate        void ShowLogMessageUInt3Delegate(RaptureLogModule* module, uint logMessageID, uint value1, uint value2, uint value3);
    private static          Hook<ShowLogMessageUInt3Delegate>? ShowLogMessageUInt3Hook;

    private static readonly CompSig                             ShowMessageStringSig = new("E8 ?? ?? ?? ?? EB 68 48 8B 07 ?? ?? ?? ?? ?? ??");
    private delegate        void                                ShowLogMessageStringDelegate(RaptureLogModule* module, uint logMessageID, Utf8String* value);
    private static          Hook<ShowLogMessageStringDelegate>? ShowLogMessageStringHook;

    private static readonly CompSig BattleLogAddLogMessageSig = new("E8 ?? ?? ?? ?? EB 08 85 F6");
    private delegate        void BattleLogAddLogMessageDelegate(nint a1, nint a2, uint logMessageID, byte a4, int a5, int a6, int a7, int a8);
    private static          Hook<BattleLogAddLogMessageDelegate>? BattleLogAddLogMessageHook;
    
    private static readonly CompSig ProcessSystemLogMessageSig =
        new("40 55 56 41 54 41 55 41 57 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 ?? 49 8B F1");
    private delegate void                 ProcessSystemLogMessageDelegate(EventFramework* framework, uint eventID, uint logMessageID, nint a4, ushort a5);
    private static   Hook<ProcessSystemLogMessageDelegate>? ProcessSystemLogMessageHook;
    
    public delegate void PreLogMessageDelegate(ref bool isPrevented, ref uint logMessageID);
    public delegate void PostLogMessageDelegate(uint logMessageID);
    
    private static readonly ConcurrentDictionary<Type, ConcurrentBag<Delegate>> MethodsCollection = [];

    internal override void Init()
    {
        Config = LoadConfig<LogMessageManagerConfig>() ?? new();
        
        ShowLogMessageHook          ??= ShowMessageSig.GetHook<ShowLogMessageDelegate>(ShowLogMessageDetour);
        ShowLogMessage2Hook         ??= ShowMessage2Sig.GetHook<ShowLog2Message2Delegate>(ShowLogMessage2Detour);
        ShowLogMessageUIntHook      ??= ShowMessageUIntSig.GetHook<ShowLogMessageUIntDelegate>(ShowLogMessageUIntDetour);
        ShowLogMessageUInt2Hook     ??= ShowMessageUInt2Sig.GetHook<ShowLogMessageUInt2Delegate>(ShowLogMessageUInt2Detour);
        ShowLogMessageUInt3Hook     ??= ShowMessageUInt3Sig.GetHook<ShowLogMessageUInt3Delegate>(ShowLogMessageUInt3Detour);
        ShowLogMessageStringHook    ??= ShowMessageStringSig.GetHook<ShowLogMessageStringDelegate>(ShowLogMessageStringDetour);
        BattleLogAddLogMessageHook  ??= BattleLogAddLogMessageSig.GetHook<BattleLogAddLogMessageDelegate>(BattleLogAddLogMessageDetour);
        ProcessSystemLogMessageHook ??= ProcessSystemLogMessageSig.GetHook<ProcessSystemLogMessageDelegate>(ProcessSystemLogMessageDetour);
        
        ToggleHooks(true);
    }

    #region Hook

    private static void ShowLogMessageDetour(RaptureLogModule* module, uint logMessageID)
    {
        if (!OnPreReceiveLogMessage(ref logMessageID)) return;
        ShowLogMessageHook.Original(module, logMessageID);
        OnPostReceiveLogMessage(logMessageID);
    }

    private static void ShowLogMessage2Detour(RaptureLogModule* module, uint logMessageID, Character* localPlayer, Character* targetObject)
    {
        if (!OnPreReceiveLogMessage(ref logMessageID)) return;
        ShowLogMessage2Hook.Original(module, logMessageID, localPlayer, targetObject);
        OnPostReceiveLogMessage(logMessageID);
    }

    private static void ShowLogMessageUIntDetour(RaptureLogModule* module, uint logMessageID, uint value)
    {
        if (!OnPreReceiveLogMessage(ref logMessageID)) return;
        ShowLogMessageUIntHook.Original(module, logMessageID, value);
        OnPostReceiveLogMessage(logMessageID);
    }

    private static void ShowLogMessageUInt2Detour(RaptureLogModule* module, uint logMessageID, uint value1, uint value2)
    {
        if (!OnPreReceiveLogMessage(ref logMessageID)) return;
        ShowLogMessageUInt2Hook.Original(module, logMessageID, value1, value2);
        OnPostReceiveLogMessage(logMessageID);
    }

    private static void ShowLogMessageUInt3Detour(
        RaptureLogModule* module, uint logMessageID, uint value1, uint value2, uint value3)
    {
        if (!OnPreReceiveLogMessage(ref logMessageID)) return;
        ShowLogMessageUInt3Hook.Original(module, logMessageID, value1, value2, value3);
        OnPostReceiveLogMessage(logMessageID);
    }

    private static void ShowLogMessageStringDetour(RaptureLogModule* module, uint logMessageID, Utf8String* value)
    {
        if (!OnPreReceiveLogMessage(ref logMessageID)) return;
        ShowLogMessageStringHook.Original(module, logMessageID, value);
        OnPostReceiveLogMessage(logMessageID);
    }

    private static void BattleLogAddLogMessageDetour(nint a1, nint a2, uint logMessageID, byte a4, int a5, int a6, int a7, int a8)
    {
        if (!OnPreReceiveLogMessage(ref logMessageID)) return;
        BattleLogAddLogMessageHook.Original(a1, a2, logMessageID, a4, a5, a6, a7, a8);
        OnPostReceiveLogMessage(logMessageID);
    }

    private static void ProcessSystemLogMessageDetour(EventFramework* framework, uint eventID, uint logMessageID, nint a4, ushort a5)
    {
        if (!OnPreReceiveLogMessage(ref logMessageID)) return;
        ProcessSystemLogMessageHook.Original(framework, eventID, logMessageID, a4, a5);
        OnPostReceiveLogMessage(logMessageID);
    }

    public static void ToggleHooks(bool isEnable)
    {
        ShowLogMessageHook?.Toggle(isEnable);
        ShowLogMessage2Hook?.Toggle(isEnable);
        ShowLogMessageUIntHook?.Toggle(isEnable);
        ShowLogMessageUInt2Hook?.Toggle(isEnable);
        ShowLogMessageUInt3Hook?.Toggle(isEnable);
        ShowLogMessageStringHook?.Toggle(isEnable);
        BattleLogAddLogMessageHook?.Toggle(isEnable);
        ProcessSystemLogMessageHook?.Toggle(isEnable);
    }

    #endregion

    #region Event
    
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
    
    public static bool Register(params   PreLogMessageDelegate[]  methods) => RegisterGeneric(methods);
    public static bool Register(params   PostLogMessageDelegate[] methods) => RegisterGeneric(methods);
    public static bool Unregister(params PreLogMessageDelegate[]  methods) => UnregisterGeneric(methods);
    public static bool Unregister(params PostLogMessageDelegate[] methods) => UnregisterGeneric(methods);

    private static bool OnPreReceiveLogMessage(ref uint logMessageID)
    {
        if (Config.ShowLogMessageLog)
            Debug($"[Log Message Manager]\nID: {logMessageID}");
        
        var isPrevented = false;
        if (MethodsCollection.TryGetValue(typeof(PreLogMessageDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PreLogMessageDelegate)preDelegate;
                preExecuteCommand(ref isPrevented, ref logMessageID);
                if (isPrevented) return false;
            }
        }

        return true;
    }
    
    private static void OnPostReceiveLogMessage(uint logMessageID)
    {
        if (MethodsCollection.TryGetValue(typeof(PostLogMessageDelegate), out var preDelegates))
        {
            foreach (var preDelegate in preDelegates)
            {
                var preExecuteCommand = (PostLogMessageDelegate)preDelegate;
                preExecuteCommand(logMessageID);
            }
        }
    }


    #endregion

    #region Send

    public static void Send(uint logMessageID) 
        => ShowLogMessageHook.Original(UIModule.Instance()->GetRaptureLogModule(), logMessageID);

    public static void SendUInt(uint logMessageID, uint value) 
        => ShowLogMessageUIntHook.Original(UIModule.Instance()->GetRaptureLogModule(), logMessageID, value);

    public static void SendUInt2(uint logMessageID, uint value1, uint value2) 
        => ShowLogMessageUInt2Hook.Original(UIModule.Instance()->GetRaptureLogModule(), logMessageID, value1, value2);

    public static void SendUInt3(uint logMessageID, uint value1, uint value2, uint value3) 
        => ShowLogMessageUInt3Hook.Original(UIModule.Instance()->GetRaptureLogModule(), logMessageID, value1, value2, value3);

    public static void SendString(uint logMessageID, string value) 
        => ShowLogMessageStringHook.Original(UIModule.Instance()->GetRaptureLogModule(), logMessageID, Utf8String.FromString(value));

    public static void SendString(uint logMessageID, SeString value)
    {
        var utf8String = Utf8String.FromString(".");
        utf8String->SetString(value.Encode());
        
        ShowLogMessageStringHook.Original(UIModule.Instance()->GetRaptureLogModule(), logMessageID, utf8String);
        
        utf8String->Dtor(true);
    }

    public static void SendString(uint logMessageID, Utf8String* value)
        => ShowLogMessageStringHook.Original(UIModule.Instance()->GetRaptureLogModule(), logMessageID, value);

    #endregion

    internal override void Uninit()
    {
        ToggleHooks(false);
        
        MethodsCollection.Clear();

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
    }
    
    public class LogMessageManagerConfig : OmenServiceConfiguration
    {
        public bool ShowLogMessageLog;

        public void Save() => 
            this.Save(DService.GetOmenService<LogMessageManager>());
    }
}
