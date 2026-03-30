using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using OmenTools.Dalamud;
using OmenTools.Dalamud.Services.AetheryteList;
using OmenTools.Dalamud.Services.ObjectTable;
using OmenTools.Interop.Game;
using OmenTools.OmenService.Abstractions;
using OmenTools.Threading.TaskHelper;

namespace OmenTools;

public sealed class DService
{
    #region 公开接口

    public static void Init(IDalamudPluginInterface pluginInterface, Func<DServiceInitOptions>? optionsFunc = null)
    {
        if (IsInitialized || IsDisposed) return;

        var instance = Instance();
        pluginInterface.Inject(instance);

        instance.ResetServiceState();

        instance.InitOptions = optionsFunc != null ? optionsFunc() : new();

        instance.PI            = pluginInterface;
        instance.UIBuilder     = pluginInterface.UiBuilder;
        instance.ObjectTable   = new ObjectTable();
        instance.AetheryteList = new AetheryteList();

        try
        {
            var serviceTypes = instance.DiscoverEnabledServiceTypes();
            instance.InstantiateServices(serviceTypes);

            foreach (var serviceType in serviceTypes)
            {
                var service = instance.OmenServices[serviceType];
                service.PublicInit();
                instance.initializedServiceOrder.Add(serviceType);
            }

            IsInitialized = true;
        }
        catch (Exception ex)
        {
            DLog.Error("[OmenTools] 初始化各 OmenService 时发生错误", ex);
            Uninit();

            throw;
        }

        var alc   = AssemblyLoadContext.GetLoadContext(typeof(DService).Assembly);
        var owner = pluginInterface.GetPlugin(alc);
        DLog.Debug($"[OmenTools] 初始化完成\tALC: {alc}; 持有方: {owner?.InternalName ?? "<shared>"}");
    }

    public static void Uninit()
    {
        if (IsDisposed)
            return;
        
        if (InternalInstance == null) 
            return;
        
        try
        {
            InternalInstance.UninitOmenServices();

            InternalInstance.DisposeTrackedTaskHelpers();
            InternalInstance.DisposeTrackedMemoryPatches();
            InternalInstance.DisposeTrackedHooks();

            InternalInstance.ResetServiceState();
        }
        catch (Exception ex)
        {
            DLog.Error("[OmenTools] 卸载各 OmenService 时发生错误", ex);
            throw;
        }
        finally
        {
            IsDisposed = true;
        }
        
        var alc   = AssemblyLoadContext.GetLoadContext(typeof(DService).Assembly);
        var owner = Instance().PI.GetPlugin(alc);
        DLog.Debug($"[OmenTools] 卸载完成\tALC: {alc}; 持有方: {owner?.InternalName ?? "<shared>"}");
    }

    public static DService Instance() =>
        InternalInstance ??= new();

    public T? GetOmenService<T>() where T : OmenServiceBase =>
        (T?)OmenServices.GetValueOrDefault(typeof(T));

    #endregion

    #region 生命周期

    public static bool IsDisposed { get; private set; }

    public static bool IsInitialized { get; private set; }

    private static DService? InternalInstance { get; set; }

    #endregion

    #region 私有

    private DServiceInitOptions InitOptions { get; set; } = new();

    private Dictionary<Type, OmenServiceBase> OmenServices { get; set; } = [];
    
    private ConcurrentDictionary<TaskHelper, byte>   TaskHelpers   { get; set; } = [];
    private ConcurrentDictionary<MemoryPatch, byte>  MemoryPatches { get; set; } = [];
    private ConcurrentDictionary<IDalamudHook, byte> Hooks         { get; set; } = [];

    private List<Type> initializedServiceOrder = [];

    private List<Type> DiscoverEnabledServiceTypes() =>
        Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(OmenServiceBase).IsAssignableFrom(t) && !t.IsAbstract)
                .Where(t => !InitOptions.IsDisabled(t))
                .ToList();

    private void InstantiateServices(IEnumerable<Type> serviceTypes)
    {
        foreach (var serviceType in serviceTypes)
        {
            if (Activator.CreateInstance(serviceType) is not OmenServiceBase serviceInstance)
                throw new InvalidOperationException($"鏃犳硶鍒涘缓 OmenService 瀹炰緥: {serviceType.FullName}");

            OmenServices.TryAdd(serviceType, serviceInstance);
        }
    }

    private void UninitOmenServices()
    {
        foreach (var serviceType in initializedServiceOrder.AsEnumerable().Reverse())
        {
            if (OmenServices.TryGetValue(serviceType, out var service))
                service.PublicUninit();
        }
    }

    private void ResetServiceState()
    {
        OmenServices  = [];
        TaskHelpers   = [];
        MemoryPatches = [];
        Hooks         = [];
        
        initializedServiceOrder = [];
        InitOptions             = new();
    }

    internal void RegTaskHelper(TaskHelper taskHelper)
    {
        ArgumentNullException.ThrowIfNull(taskHelper);
        TaskHelpers.TryAdd(taskHelper, 0);
    }

    internal void UnregTaskHelper(TaskHelper taskHelper)
    {
        ArgumentNullException.ThrowIfNull(taskHelper);
        TaskHelpers.TryRemove(taskHelper, out _);
    }

    internal void RegMemoryPatch(MemoryPatch memoryPatch)
    {
        ArgumentNullException.ThrowIfNull(memoryPatch);
        MemoryPatches.TryAdd(memoryPatch, 0);
    }

    internal void UnregMemoryPatch(MemoryPatch memoryPatch)
    {
        ArgumentNullException.ThrowIfNull(memoryPatch);
        MemoryPatches.TryRemove(memoryPatch, out _);
    }

    internal void RegHook(IDalamudHook hook)
    {
        ArgumentNullException.ThrowIfNull(hook);
        Hooks.TryAdd(hook, 0);
    }

    private void DisposeTrackedTaskHelpers()
    {
        foreach (var taskHelper in TaskHelpers.Keys)
        {
            if (taskHelper is not { IsDisposed: false }) continue;
            taskHelper.Dispose();
        }

        TaskHelpers.Clear();
    }

    private void DisposeTrackedMemoryPatches()
    {
        foreach (var memoryPatch in MemoryPatches.Keys)
            memoryPatch.Dispose();

        MemoryPatches.Clear();
    }

    private void DisposeTrackedHooks()
    {
        foreach (var hook in Hooks.Keys)
        {
            if (hook is not { IsDisposed: false }) continue;
            hook.Dispose();
        }

        Hooks.Clear();
    }

    #endregion

    #region Dalamud 服务

    [PluginService]
    public IAddonEventManager AddonEvent { get; private set; } = null!;

    [PluginService]
    public IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    [PluginService]
    public IAgentLifecycle AgentLifecycle { get; private set; } = null!;

    [PluginService]
    public IBuddyList BuddyList { get; private set; } = null!;

    [PluginService]
    public IChatGui Chat { get; private set; } = null!;

    [PluginService]
    public IClientState ClientState { get; private set; } = null!;

    [PluginService]
    public ICommandManager Command { get; private set; } = null!;

    [PluginService]
    public ICondition Condition { get; private set; } = null!;

    [PluginService]
    public IContextMenu ContextMenu { get; private set; } = null!;

    [PluginService]
    public IDataManager Data { get; private set; } = null!;

    [PluginService]
    public IDtrBar DTRBar { get; private set; } = null!;

    [PluginService]
    public IDutyState DutyState { get; private set; } = null!;

    [PluginService]
    public IFateTable Fate { get; private set; } = null!;

    [PluginService]
    public IFlyTextGui FlyText { get; private set; } = null!;

    [PluginService]
    public IFramework Framework { get; private set; } = null!;

    [PluginService]
    public IGameConfig GameConfig { get; private set; } = null!;

    [PluginService]
    public IGameGui GameGUI { get; private set; } = null!;

    [PluginService]
    public IGameInteropProvider Hook { get; private set; } = null!;

    [PluginService]
    public IGameInventory GameInventory { get; private set; } = null!;

    [PluginService]
    public IGameLifecycle GameLifecycle { get; private set; } = null!;

    [PluginService]
    public IGamepadState Gamepad { get; private set; } = null!;

    [PluginService]
    public IJobGauges JobGauges { get; private set; } = null!;

    [PluginService]
    public IKeyState KeyState { get; private set; } = null!;

    [PluginService]
    public IMarketBoard MarketBoard { get; private set; } = null!;

    [PluginService]
    public INamePlateGui NamePlate { get; private set; } = null!;

    [PluginService]
    public INotificationManager DalamudNotification { get; private set; } = null!;

    [PluginService]
    public IPartyFinderGui PartyFinder { get; private set; } = null!;

    [PluginService]
    public IPartyList PartyList { get; private set; } = null!;

    [PluginService]
    public IPlayerState PlayerState { get; private set; } = null!;

    [PluginService]
    public IPluginLog Log { get; private set; } = null!;

    [PluginService]
    public ISeStringEvaluator SeStringEvaluator { get; private set; } = null!;

    [PluginService]
    public ISelfTestRegistry SelfTestRegistry { get; private set; }

    [PluginService]
    public ISigScanner SigScanner { get; private set; }

    [PluginService]
    public ITextureProvider Texture { get; private set; } = null!;

    [PluginService]
    public ITextureReadbackProvider TextureReadback { get; private set; } = null!;

    [PluginService]
    public ITextureSubstitutionProvider TextureSubstitution { get; private set; } = null!;

    [PluginService]
    public ITitleScreenMenu TitleScreenMenu { get; private set; } = null!;

    [PluginService]
    public IToastGui Toast { get; private set; } = null!;

    [PluginService]
    public IUnlockState UnlockState { get; private set; } = null!;

    public IDalamudPluginInterface PI            { get; private set; } = null!;
    public IUiBuilder              UIBuilder     { get; private set; } = null!;
    public IAetheryteList          AetheryteList { get; private set; } = null!;
    public IObjectTable            ObjectTable   { get; private set; } = null!;

    #endregion
}
