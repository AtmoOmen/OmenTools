global using static OmenTools.Helpers.HelpersOm;
global using static OmenTools.Infos.InfosOm;
using System.Reflection;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using OmenTools.Abstracts;

namespace OmenTools;

public class DService
{
    internal static Dictionary<Type, OmenServiceBase> OmenServices { get; private set; } = [];
    
    public static void Init(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<DService>();
        
        PI            = pluginInterface;
        UIBuilder     = pluginInterface.UiBuilder;
        ObjectTable   = new ObjectTable();
        Targets       = new TargetManager();
        AetheryteList = new AetheryteList();
        
        var services = Assembly.GetExecutingAssembly().GetTypes()
                               .Where(t => typeof(OmenServiceBase).IsAssignableFrom(t) && !t.IsAbstract)
                               .ToList();
        foreach (var serviceType in services)
        {
            if (Activator.CreateInstance(serviceType) is not OmenServiceBase serviceInstance) continue;
            OmenServices.TryAdd(serviceType, serviceInstance);
        }
        
        var invalidServices = new List<OmenServiceBase>();
        OmenServices.ForEach(x =>
        {
            if (x.Value.IsDisposed) return;

            try
            {
                x.Value.Init();
            }
            catch (Exception ex)
            {
                Error($"在加载 OmenService {x.Value} 的过程中发生错误", ex);

                invalidServices.Add(x.Value);
            }
        });

        invalidServices.ForEach(x =>
        {
            if (x.IsDisposed) return;

            try
            {
                x.Uninit();
            }
            catch
            {
                // ignored
            }
        });
    }

    public static void Uninit()
    {
        OmenServices.Reverse().ForEach(x =>
        {
            if (x.Value.IsDisposed) return;
            
            try
            {
                x.Value.Uninit();
                x.Value.SetDisposed();
            }
            catch (Exception ex)
            {
                Error($"在卸载 OmenService {x.Value} 的过程中发生错误", ex);
            }
        });
        
        TaskHelper.DisposeAll();
        MemoryPatch.DisposeAll();
        ThrottlerHelper.Uninit();
        TrayNotify.Uninit();
        CompSig.DisposeAllHooks();
    }

    public static T? GetOmenService<T>() where T : OmenServiceBase => 
        (T?)OmenServices.GetValueOrDefault(typeof(T));

    public static void InitTrayNotify(Icon icon, string multiMessagesReceived = "收到了 {0} 条新消息", bool onlyBackground = false) => 
        TrayNotify.Init(icon, multiMessagesReceived, onlyBackground);
    
    [PluginService] public static IAddonLifecycle      AddonLifecycle    { get; private set; } = null!;
    [PluginService] public static IAddonEventManager   AddonEvent        { get; private set; } = null!;
    [PluginService] public static IBuddyList           BuddyList         { get; private set; } = null!;
    [PluginService] public static IChatGui             Chat              { get; private set; } = null!;
    [PluginService] public static IClientState         ClientState       { get; private set; } = null!;
    [PluginService] public static ICommandManager      Command           { get; private set; } = null!;
    [PluginService] public static ICondition           Condition         { get; private set; } = null!;
    [PluginService] public static IContextMenu         ContextMenu       { get; private set; } = null!;
    [PluginService] public static IDataManager         Data              { get; private set; } = null!;
    [PluginService] public static IDtrBar              DtrBar            { get; private set; } = null!;
    [PluginService] public static IDutyState           DutyState         { get; private set; } = null!;
    [PluginService] public static IFateTable           Fate              { get; private set; } = null!;
    [PluginService] public static IFlyTextGui          FlyText           { get; private set; } = null!;
    [PluginService] public static IFramework           Framework         { get; private set; } = null!;
    [PluginService] public static IGameConfig          GameConfig        { get; private set; } = null!;
    [PluginService] public static IGameGui             Gui               { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider Hook              { get; private set; } = null!;
    [PluginService] public static IGameInventory       Inventory         { get; private set; } = null!;
    [PluginService] public static IGameLifecycle       Lifecycle         { get; private set; } = null!;
    [PluginService] public static IGamepadState        Gamepad           { get; private set; } = null!;
    [PluginService] public static IJobGauges           JobGauges         { get; private set; } = null!;
    [PluginService] public static IKeyState            KeyState          { get; private set; } = null!;
    [PluginService] public static IMarketBoard         MarketBoard       { get; private set; } = null!;
    [PluginService] public static INamePlateGui        NamePlateGui      { get; private set; } = null!;
    [PluginService] public static INotificationManager DNotice           { get; private set; } = null!;
    [PluginService] public static IPartyFinderGui      PartyFinder       { get; private set; } = null!;
    [PluginService] public static IPartyList           PartyList         { get; private set; } = null!;
    [PluginService] public static IPlayerState         PlayerState       { get; private set; } = null!;
    [PluginService] public static IPluginLog           Log               { get; private set; } = null!;
    [PluginService] public static ISeStringEvaluator   SeStringEvaluator { get; private set; } = null!;
    [PluginService] public static ITextureProvider     Texture           { get; private set; } = null!;
    [PluginService] public static ITitleScreenMenu     TitleScreenMenu   { get; private set; } = null!;
    [PluginService] public static IToastGui            Toast             { get; private set; } = null!;

    public static IDalamudPluginInterface PI            { get; private set; } = null!;
    public static IUiBuilder              UIBuilder     { get; private set; } = null!;
    public static IAetheryteList          AetheryteList { get; private set; } = null!;
    public static IObjectTable            ObjectTable   { get; private set; } = null!;
    public static ITargetManager          Targets       { get; private set; } = null!;
    public static SigScanner              SigScanner    { get; private set; } = new();
}
