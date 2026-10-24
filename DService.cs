using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Dalamud.Hooking;
using FFXIVClientStructs.Expansion;
using Dalamud.IoC;
using Microsoft.Extensions.DependencyInjection;
using OmenTools.Dalamud;
using OmenTools.Dalamud.Services.Game.Object;
using OmenTools.Dalamud.Services.Game.UI;
using OmenTools.Info.Lumina;
using OmenTools.Interop.Game;
using OmenTools.OmenService.Abstractions;
using OmenTools.Threading;
using OmenTools.Threading.TaskHelper;

namespace OmenTools;

public sealed class DService
{
    #region 公开接口

    /// <summary>
    ///     异步构建依赖容器，并按游戏帧的时间预算依次初始化基础服务。
    /// </summary>
    /// <param name="pluginInterface">持有基础服务的插件接口。</param>
    /// <param name="disabledServices">不参与注册和初始化的服务类型。</param>
    /// <param name="cancellationToken">用于取消尚未执行的初始化操作。</param>
    public static async Task InitAsync
    (
        IDalamudPluginInterface    pluginInterface,
        IReadOnlyCollection<Type>? disabledServices  = null,
        CancellationToken          cancellationToken = default
    )
    {
        if (IsInitialized || IsDisposed) return;

        var instance = Instance();
        pluginInterface.Inject(instance);

        instance.ResetServiceState();

        instance.PI            = pluginInterface;
        instance.UIBuilder     = pluginInterface.UiBuilder;
        instance.ObjectTable   = new ObjectTable();
        instance.AetheryteList = new AetheryteList();

        instance.OmenDalamudServices[typeof(IObjectTable)]   = instance.ObjectTable;
        instance.OmenDalamudServices[typeof(IAetheryteList)] = instance.AetheryteList;

        try
        {
            try
            {
                ExpansionInterop.Initialize(instance.SigScanner.SearchBase);
            }
            catch (Exception ex)
            {
                DLog.Error("[OmenTools] 初始化 FFXIVClientStructs.Expansion 时发生错误。", ex);
            }


            instance.InitializeServices(disabledServices);

            await FrameworkWorkScheduler.RunAsync(instance.ServiceOrder, static service => service.PublicInit(), cancellationToken).ConfigureAwait(false);

            IsInitialized = true;
        }
        catch (Exception ex)
        {
            DLog.Error("[OmenTools] 初始化各 OmenService 时发生错误。", ex);
            await UninitAsync().ConfigureAwait(false);
            throw;
        }

        var alc   = AssemblyLoadContext.GetLoadContext(typeof(DService).Assembly);
        var owner = pluginInterface.GetPlugin(alc);
        DLog.Debug($"[OmenTools] 初始化完成\tALC: {alc}; 持有方: {owner?.InternalName ?? "<shared>"}");
    }

    /// <summary>
    ///     按依赖逆序分段卸载基础服务，并释放登记的任务、内存补丁及钩子。
    /// </summary>
    public static async Task UninitAsync()
    {
        if (IsDisposed)
            return;

        if (InternalInstance == null)
            return;

        try
        {
            await FrameworkWorkScheduler.RunAsync
            (
                [.. InternalInstance.ServiceOrder.Where(static service => service.IsInitialized).Reverse()],
                static service => service.PublicUninit()
            ).ConfigureAwait(false);

            await FrameworkWorkScheduler.RunAsync
            (
                [.. InternalInstance.TaskHelpers.Keys.Where(static helper => !helper.IsDisposed)],
                static helper => DisposeResource(helper)
            ).ConfigureAwait(false);
            InternalInstance.TaskHelpers.Clear();

            await FrameworkWorkScheduler.RunAsync
            (
                [.. InternalInstance.MemoryPatches.Keys],
                static patch => DisposeResource(patch)
            ).ConfigureAwait(false);
            InternalInstance.MemoryPatches.Clear();

            await FrameworkWorkScheduler.RunAsync
            (
                [.. InternalInstance.Hooks.SelectMany(static pair => pair.Value.Keys).Where(static hook => !hook.IsDisposed)],
                static hook => DisposeResource(hook)
            ).ConfigureAwait(false);
            InternalInstance.Hooks.Clear();

            await Task.Run(InternalInstance.ResetServiceState).ConfigureAwait(false);

            Throttler.Shared.Clear();

            InternalInstance.ObjectTable   = null;
            InternalInstance.AetheryteList = null;
            InternalInstance.OmenDalamudServices.Clear();

            // TODO: 等待 Lumina 更新 World 定义
            Sheets.Worlds.Clear();
            Sheets.CNWorlds.Clear();
        }
        catch (Exception ex)
        {
            DLog.Error("[OmenTools] 卸载各 OmenService 时发生错误", ex);
            throw;
        }
        finally
        {
            await IFramework.Instance().Run(ExpansionInterop.Uninitialize).ConfigureAwait(false);
            IsInitialized = false;
            IsDisposed    = true;
        }

        var alc   = AssemblyLoadContext.GetLoadContext(typeof(DService).Assembly);
        var owner = IDalamudPluginInterface.Instance().GetPlugin(alc);
        DLog.Debug($"[OmenTools] 卸载完成\tALC: {alc}; 持有方: {owner?.InternalName ?? "<shared>"}");
    }

    public static DService Instance() =>
        InternalInstance ??= new();

    public T? GetOmenService<T>() where T : OmenServiceBase =>
        (T?)OmenServices.GetValueOrDefault(typeof(T));

    public T? GetOmenDalamudService<T>() where T : IOmenDalamudService =>
        (T?)OmenDalamudServices.GetValueOrDefault(typeof(T));

    #endregion

    #region 生命周期

    public static bool IsDisposed { get; private set; }

    public static bool IsInitialized { get; private set; }

    private static DService? InternalInstance { get; set; }

    #endregion

    #region 私有

    private ServiceProvider? Container { get; set; }

    private Dictionary<Type, OmenServiceBase>     OmenServices        { get; set; } = [];
    private Dictionary<Type, IOmenDalamudService> OmenDalamudServices { get; set; } = [];

    private ConcurrentDictionary<TaskHelper, byte>  TaskHelpers   { get; set; } = [];
    private ConcurrentDictionary<MemoryPatch, byte> MemoryPatches { get; set; } = [];

    private ConditionalWeakTable<AssemblyLoadContext, ConcurrentDictionary<IDalamudHook, byte>> Hooks { get; set; } = [];

    private List<OmenServiceBase> ServiceOrder { get; set; } = [];

    private static Type[] DiscoverServiceTypes() =>
    [
        .. Assembly.GetExecutingAssembly()
                   .GetTypes()
                   .Where(t => typeof(OmenServiceBase).IsAssignableFrom(t) && !t.IsAbstract)
    ];

    private void InitializeServices
    (
        IReadOnlyCollection<Type>? disabledServices
    )
    {
        var serviceTypes = DiscoverServiceTypes();
        var services     = new ServiceCollection();

        foreach (var serviceType in serviceTypes)
        {
            if (IsDisabled(serviceType, disabledServices))
                continue;

            VerifyDependencies(serviceType, disabledServices);

            services.AddSingleton(serviceType, provider => CreateService(provider, serviceType));
        }

        Container = services.BuildServiceProvider();

        foreach (var serviceType in serviceTypes)
        {
            if (IsDisabled(serviceType, disabledServices))
                continue;

            Container.GetRequiredService(serviceType);
        }
    }

    private static bool IsDisabled
    (
        Type                       serviceType,
        IReadOnlyCollection<Type>? disabledServices
    ) =>
        disabledServices?.Contains(serviceType) == true;

    private static void VerifyDependencies
    (
        Type                       serviceType,
        IReadOnlyCollection<Type>? disabledServices
    )
    {
        if (disabledServices == null || disabledServices.Count == 0)
            return;

        var constructor = serviceType.GetConstructors()
                                     .MaxBy(static constructor => constructor.GetParameters().Length);

        if (constructor == null)
            return;

        foreach (var parameter in constructor.GetParameters())
        {
            if (IsDisabled(parameter.ParameterType, disabledServices))
                throw new InvalidOperationException($"服务 {serviceType.Name} 依赖已禁用的服务 {parameter.ParameterType.Name}");
        }
    }

    private OmenServiceBase CreateService
    (
        IServiceProvider provider,
        Type             serviceType
    )
    {
        if (ActivatorUtilities.CreateInstance(provider, serviceType) is not OmenServiceBase service)
            throw new InvalidOperationException($"初始化 OmenService 错误: {serviceType.FullName}");

        ServiceOrder.Add(service);
        OmenServices.TryAdd(serviceType, service);

        return service;
    }

    private void ResetServiceState()
    {
        Container?.Dispose();
        Container = null;

        OmenServices  = [];
        ServiceOrder  = [];
        TaskHelpers   = [];
        MemoryPatches = [];
        Hooks         = new();
    }

    internal void RegTaskHelper
    (
        TaskHelper taskHelper
    )
    {
        ArgumentNullException.ThrowIfNull(taskHelper);
        TaskHelpers.TryAdd(taskHelper, 0);
    }

    internal void UnregTaskHelper
    (
        TaskHelper taskHelper
    )
    {
        ArgumentNullException.ThrowIfNull(taskHelper);
        TaskHelpers.TryRemove(taskHelper, out _);
    }

    internal void RegMemoryPatch
    (
        MemoryPatch memoryPatch
    )
    {
        ArgumentNullException.ThrowIfNull(memoryPatch);
        MemoryPatches.TryAdd(memoryPatch, 0);
    }

    internal void UnregMemoryPatch
    (
        MemoryPatch memoryPatch
    )
    {
        ArgumentNullException.ThrowIfNull(memoryPatch);
        MemoryPatches.TryRemove(memoryPatch, out _);
    }

    internal void RegHook
    (
        IDalamudHook        hook,
        AssemblyLoadContext context
    )
    {
        ArgumentNullException.ThrowIfNull(hook);
        ArgumentNullException.ThrowIfNull(context);

        var contextHooks = Hooks.GetValue(context, static _ => []);

        if (context.IsCollectible && contextHooks.IsEmpty)
            context.Unloading += _ => UnregHooks(context);

        contextHooks.TryAdd(hook, 0);
    }

    internal void UnregHooks
    (
        AssemblyLoadContext context
    )
    {
        if (!Hooks.TryGetValue(context, out var contextHooks)) return;

        DisposeHooks(contextHooks.Keys);
        Hooks.Remove(context);
    }

    private static void DisposeHooks
    (
        IEnumerable<IDalamudHook> hooks
    )
    {
        foreach (var hook in hooks)
        {
            if (hook is not { IsDisposed: false }) continue;
            DisposeResource(hook);
        }
    }

    private static void DisposeResource
    (
        IDisposable resource
    )
    {
        try
        {
            resource.Dispose();
        }
        catch (Exception ex)
        {
            DLog.Error($"[OmenTools] 释放资源 {resource.GetType().Name} 时发生错误", ex);
        }
    }

    #endregion

    #region Dalamud 服务

    [PluginService]
    public IAddonEventManager AddonEvent { get; private set; } = null!;

    [PluginService]
    public IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    [PluginService]
    public IAgentLifecycle AgentLifecycle { get; private set; } = null!;

    /// <summary>
    ///     尽量不要直接使用，用 FFXIVClientStruct。
    /// </summary>
    [PluginService]
    public IBuddyList BuddyList { get; private set; } = null!;

    /// <summary>
    ///     尽量不要直接使用，大部分功能有其他替代方案。
    /// </summary>
    [PluginService]
    public IChatGui Chat { get; private set; } = null!;

    [PluginService]
    public IClientState ClientState { get; private set; } = null!;

    /// <summary>
    ///     尽量不要直接使用，大部分功能有其他替代方案。
    /// </summary>
    [PluginService]
    public ICommandManager Command { get; private set; } = null!;

    [PluginService]
    public ICondition Condition { get; private set; } = null!;

    /// <summary>
    ///     尽量不要直接使用，大部分功能有其他替代方案。
    /// </summary>
    [PluginService]
    public IContextMenu ContextMenu { get; private set; } = null!;

    /// <summary>
    ///     尽量不要直接使用，大部分功能有其他替代方案。
    /// </summary>
    [PluginService]
    public IDataManager Data { get; private set; } = null!;

    [PluginService]
    public IDtrBar DTRBar { get; private set; } = null!;

    [PluginService]
    public IDutyState DutyState { get; private set; } = null!;

    /// <summary>
    ///     尽量不要直接使用，用 FFXIVClientStruct。
    /// </summary>
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

public interface IOmenDalamudService<T> : IOmenDalamudService where T : IOmenDalamudService<T>
{
    static T Instance() =>
        DService.Instance().GetOmenDalamudService<T>() ??
        throw new InvalidOperationException($"服务 {typeof(T).Name} 尚未注册或初始化");
}

public interface IOmenDalamudService;
