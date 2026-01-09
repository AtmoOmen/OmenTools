using System.Reflection;
using Dalamud.Interface.Textures;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using OmenTools.Abstracts;

namespace OmenTools;

public sealed class DService
{
    public static bool Initialized { get; private set; }

    private static DService? InternalInstance { get; set; }
    
    public static void Init(IDalamudPluginInterface pluginInterface)
    {
        if (Initialized) return;
        Initialized = true;
        
        pluginInterface.Inject(Instance());

        Instance().PI            = pluginInterface;
        Instance().UIBuilder     = pluginInterface.UiBuilder;
        Instance().ObjectTable   = new ObjectTable();
        Instance().AetheryteList = new AetheryteList();

        var services = Assembly.GetExecutingAssembly().GetTypes()
                               .Where(t => typeof(OmenServiceBase).IsAssignableFrom(t) && !t.IsAbstract)
                               .ToList();

        foreach (var serviceType in services)
        {
            if (Activator.CreateInstance(serviceType) is not OmenServiceBase serviceInstance) continue;
            Instance().OmenServices.TryAdd(serviceType, serviceInstance);
        }

        var invalidServices = new List<OmenServiceBase>();
        foreach (var instance in Instance().OmenServices)
        {
            if (instance.Value.IsDisposed) continue;

            try
            {
                instance.Value.Init();
            }
            catch (Exception ex)
            {
                Error($"在加载 OmenService {instance.Value} 的过程中发生错误", ex);

                invalidServices.Add(instance.Value);
            }
        }

        foreach (var instance in invalidServices)
        {
            if (instance.IsDisposed) continue;

            try
            {
                instance.Uninit();
            }
            catch
            {
                // ignored
            }
        }
    }

    public static void Uninit()
    {
        if (!Initialized) return;

        foreach (var instance in Instance().OmenServices)
        {
            if (instance.Value.IsDisposed) continue;

            try
            {
                instance.Value.Uninit();
                instance.Value.SetDisposed();
            }
            catch (Exception ex)
            {
                Error($"在卸载 OmenService {instance.Value} 的过程中发生错误", ex);
            }
        }

        TaskHelper.DisposeAll();
        MemoryPatch.DisposeAll();
        ThrottlerHelper.Uninit();
        
        if (Instance().TrayNotification is { } trayNotification)
        {
            trayNotification.Dispose();
            Instance().TrayNotification = null;
        }
        
        CompSig.DisposeAllHooks();
        
        Initialized = false;
    }

    public static DService Instance() => 
        InternalInstance ??= new();
    
    private Dictionary<Type, OmenServiceBase> OmenServices { get; set; } = [];
    
    public T? GetOmenService<T>() where T : OmenServiceBase =>
        (T?)OmenServices.GetValueOrDefault(typeof(T));

    [PluginService] public IAddonLifecycle      AddonLifecycle      { get; private set; } = null!;
    [PluginService] public IAddonEventManager   AddonEvent          { get; private set; } = null!;
    [PluginService] public IBuddyList           BuddyList           { get; private set; } = null!;
    [PluginService] public IChatGui             Chat                { get; private set; } = null!;
    [PluginService] public IClientState         ClientState         { get; private set; } = null!;
    [PluginService] public ICommandManager      Command             { get; private set; } = null!;
    [PluginService] public ICondition           Condition           { get; private set; } = null!;
    [PluginService] public IContextMenu         ContextMenu         { get; private set; } = null!;
    [PluginService] public IDataManager         Data                { get; private set; } = null!;
    [PluginService] public IDtrBar              DtrBar              { get; private set; } = null!;
    [PluginService] public IDutyState           DutyState           { get; private set; } = null!;
    [PluginService] public IFateTable           Fate                { get; private set; } = null!;
    [PluginService] public IFlyTextGui          FlyText             { get; private set; } = null!;
    [PluginService] public IFramework           Framework           { get; private set; } = null!;
    [PluginService] public IGameConfig          GameConfig          { get; private set; } = null!;
    [PluginService] public IGameGui             Gui                 { get; private set; } = null!;
    [PluginService] public IGameInteropProvider Hook                { get; private set; } = null!;
    [PluginService] public IGameInventory       Inventory           { get; private set; } = null!;
    [PluginService] public IGameLifecycle       Lifecycle           { get; private set; } = null!;
    [PluginService] public IGamepadState        Gamepad             { get; private set; } = null!;
    [PluginService] public IJobGauges           JobGauges           { get; private set; } = null!;
    [PluginService] public IKeyState            KeyState            { get; private set; } = null!;
    [PluginService] public IMarketBoard         MarketBoard         { get; private set; } = null!;
    [PluginService] public INamePlateGui        NamePlateGui        { get; private set; } = null!;
    [PluginService] public INotificationManager DalamudNotification { get; private set; } = null!;
    [PluginService] public IPartyFinderGui      PartyFinder         { get; private set; } = null!;
    [PluginService] public IPartyList           PartyList           { get; private set; } = null!;
    [PluginService] public IPlayerState         PlayerState         { get; private set; } = null!;
    [PluginService] public IPluginLog           Log                 { get; private set; } = null!;
    [PluginService] public ISeStringEvaluator   SeStringEvaluator   { get; private set; } = null!;
    [PluginService] public ITextureProvider     Texture             { get; private set; } = null!;
    [PluginService] public ITitleScreenMenu     TitleScreenMenu     { get; private set; } = null!;
    [PluginService] public IToastGui            Toast               { get; private set; } = null!;

    public IDalamudPluginInterface PI            { get; private set; } = null!;
    public IUiBuilder              UIBuilder     { get; private set; } = null!;
    public IAetheryteList          AetheryteList { get; private set; } = null!;
    public IObjectTable            ObjectTable   { get; private set; } = null!;
    public SigScanner              SigScanner    { get; private set; } = new();

    public TrayNotification? TrayNotification
    {
        get;
        set
        {
            field?.Dispose();
            field = value;
        }
    }
    
    public ISharedImmediateTexture? DalamudNotificationIcon { get; set; }
}
