using System.Collections.Concurrent;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

internal unsafe class AtkEventManager : OmenServiceBase
{
    private static readonly CompSig ReceiveGlobalEventSig = new("48 89 5C 24 ?? 55 57 41 57 48 83 EC 50 48 8B D9");
    private delegate        void ReceiveGlobalEventDelegate(AtkUnitBase* addon, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* data);
    private static          Hook<ReceiveGlobalEventDelegate>? ReceiveGlobalEventHook;

    private static readonly ConcurrentDictionary<uint, AtkEventWrapper> EventHandlers      = [];
    private static readonly ConcurrentStack<uint>                       AvailableParamKeys = [];
    
    private const uint BASE_PARAM_KEY = 0x53540000U;
    private const uint MAX_HANDLERS   = 1000000;
    
    private static uint NextParamKey = BASE_PARAM_KEY - 1;
    
    internal override void Init()
    {
        ReceiveGlobalEventHook ??= ReceiveGlobalEventSig.GetHook<ReceiveGlobalEventDelegate>(ReceiveGlobalEventDetour);
        ReceiveGlobalEventHook.Enable();
    }

    internal override void Uninit()
    {
        ReceiveGlobalEventHook?.Dispose();
        ReceiveGlobalEventHook = null;

        foreach (var (_, atkEvent) in EventHandlers)
            atkEvent.Dispose();
        
        EventHandlers.Clear();
        AvailableParamKeys.Clear();
    }

    private static void ReceiveGlobalEventDetour(AtkUnitBase* addon, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* data)
    {
        if (addon    == null ||
            atkEvent == null ||
            data     == null)
            return;
        
        if (EventHandlers.TryGetValue((uint)eventParam, out var simpleEvent))
        {
            try
            {
                simpleEvent.Action(eventType, addon, atkEvent, data);
                atkEvent->SetEventIsHandled();
                return;
            }
            catch (Exception ex)
            {
                Error($"尝试触发自定义 AtkEvent 时发生错误, ID: {eventParam}", ex);
            }
        }

        ReceiveGlobalEventHook.Original(addon, eventType, eventParam, atkEvent, data);
    }

    internal static uint RegisterEvent(AtkEventWrapper simpleEvent)
    {
        if (!AvailableParamKeys.TryPop(out var newParam))
        {
            newParam = Interlocked.Increment(ref NextParamKey);
            if (newParam >= BASE_PARAM_KEY + MAX_HANDLERS)
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
    public delegate void AtkEventActionDelegate(AtkEventType eventType, AtkUnitBase* addon, AtkEvent* atkEvent, AtkEventData* data);
    
    /// <summary>
    /// 事件触发时要执行的回调。
    /// </summary>
    public AtkEventActionDelegate Action { get; }

    /// <summary>
    /// 唯一事件ID
    /// </summary>
    public uint ParamKey { get; }
    
    private readonly List<(nint AddonPtr, nint NodePtr, AtkEventType Type)> registeredData = [];

    private readonly Lock dataLock = new();
    
    private bool isDisposed;

    public AtkEventWrapper(AtkEventActionDelegate action)
    {
        this.Action   = action;
        this.ParamKey = AtkEventManager.RegisterEvent(this);
    }
    
    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        if (registeredData is { Count: > 0 })
        {
            foreach (var (addonPtr, nodePtr, type) in registeredData.ToList())
            {
                var addon = (AtkUnitBase*)addonPtr;
                var node  = (AtkResNode*)nodePtr;
                if (!addon->IsAddonAndNodesReady() || node == null) continue;

                Remove(addon, node, type);
            }
        }

        AtkEventManager.UnregisterEvent(ParamKey);
        GC.SuppressFinalize(this);
    }
    
    public void Add(AtkUnitBase* addon, AtkResNode* node, AtkEventType eventType)
    {
        if (isDisposed) return;

        lock (dataLock)
        {
            node->AddEvent(eventType, ParamKey, (AtkEventListener*)addon, node, true);
            registeredData.Add(((nint)addon, (nint)node, eventType));
        }
    }

    public void Remove(AtkUnitBase* addon, AtkResNode* node, AtkEventType eventType)
    {
        lock (dataLock)
        {
            node->RemoveEvent(eventType, ParamKey, (AtkEventListener*)addon, true);
            registeredData.Remove(((nint)addon, (nint)node, eventType));
        }
    }
}
