using System.Collections.Concurrent;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

internal unsafe class AtkEventManager : OmenServiceBase
{
    private static readonly CompSig GlobalEventSig = new("48 89 5C 24 ?? 55 57 41 57 48 83 EC 50 48 8B D9 0F B7 EA");
    private delegate        void* GlobalEventDelegate(AtkUnitBase* addon, AtkEventType eventType, uint eventParam, AtkResNode** eventData, uint* a5);
    private static          Hook<GlobalEventDelegate>? GlobalEventHook;

    private static readonly ConcurrentDictionary<uint, AtkEventWrapper> EventHandlers      = [];
    private static readonly ConcurrentStack<uint>                       AvailableParamKeys = [];
    
    private const  uint BaseParamKey = 0x53540000U;
    private const  uint MaxHandlers  = 1000000;
    private static uint NextParamKey = BaseParamKey - 1;
    
    internal override void Init()
    {
        GlobalEventHook ??= GlobalEventSig.GetHook<GlobalEventDelegate>(GlobalEventDetour);
        GlobalEventHook.Enable();
    }

    internal override void Uninit()
    {
        GlobalEventHook?.Dispose();
        GlobalEventHook = null;

        foreach (var (_, atkEvent) in EventHandlers)
            atkEvent.Dispose();
        
        EventHandlers.Clear();
        AvailableParamKeys.Clear();
    }

    private static void* GlobalEventDetour(AtkUnitBase* atkUnitBase, AtkEventType eventType, uint eventParam, AtkResNode** eventData, uint* a5)
    {
        if (EventHandlers.TryGetValue(eventParam, out var simpleEvent))
        {
            try
            {
                simpleEvent.Action(eventType, atkUnitBase, eventData[0]);
                return null;
            }
            catch (Exception ex)
            {
                Error($"尝试触发自定义 AtkEvent 时发生错误, ID: {eventParam}", ex);
            }
        }

        return GlobalEventHook.Original(atkUnitBase, eventType, eventParam, eventData, a5);
    }

    internal static uint RegisterEvent(AtkEventWrapper simpleEvent)
    {
        if (!AvailableParamKeys.TryPop(out var newParam))
        {
            newParam = Interlocked.Increment(ref NextParamKey);
            if (newParam >= BaseParamKey + MaxHandlers)
            {
                Interlocked.Decrement(ref NextParamKey);
                throw new Exception("事件处理器过多 (≥ 100 万)");
            }
        }

        if (!EventHandlers.TryAdd(newParam, simpleEvent))
        {
            AvailableParamKeys.Push(newParam);
            throw new Exception($"注册事件失败, 回收的 ID 发生重复碰撞: {newParam}");
        }

        return newParam;
    }

    internal static void UnregisterEvent(uint paramKey)
    {
        if (!EventHandlers.TryRemove(paramKey, out _)) return;
        
        AvailableParamKeys.Push(paramKey);
    }
}

public unsafe class AtkEventWrapper : IDisposable
{
    public delegate void AtkEventActionDelegate(AtkEventType eventType, AtkUnitBase* addon, AtkResNode* node);
    
    /// <summary>
    /// 事件触发时要执行的回调。
    /// </summary>
    public AtkEventActionDelegate Action { get; }

    /// <summary>
    /// 唯一事件ID
    /// </summary>
    public uint ParamKey { get; }
    
    private readonly List<(nint AddonPtr, nint NodePtr, AtkEventType Type)> RegisteredData = [];

    private bool IsDisposed;

    public AtkEventWrapper(AtkEventActionDelegate action)
    {
        this.Action   = action;
        this.ParamKey = AtkEventManager.RegisterEvent(this);
    }
    
    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        
        if (RegisteredData is { Count: > 0 })
        {
            foreach (var (addonPtr, nodePtr, type) in RegisteredData)
            {
                var addon = (AtkUnitBase*)addonPtr;
                var node  = (AtkResNode*)nodePtr;
                if (addon == null || node == null) continue;

                Remove(addon, node, type);
            }
        }

        AtkEventManager.UnregisterEvent(ParamKey);
        GC.SuppressFinalize(this);
    }
    
    public void Add(AtkUnitBase* addon, AtkResNode* node, AtkEventType eventType)
    {
        if (IsDisposed) return;
        node->AddEvent(eventType, ParamKey, (AtkEventListener*)addon, node, true);
    }

    public void Remove(AtkUnitBase* addon, AtkResNode* node, AtkEventType eventType) => 
        node->RemoveEvent(eventType, ParamKey, (AtkEventListener*)addon, true);
}
