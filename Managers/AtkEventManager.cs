using System.Collections.Concurrent;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

internal unsafe class AtkEventManager : OmenServiceBase<AtkEventManager>
{
    internal uint RegisterEvent(AtkEventWrapper eventWrapper)
    {
        if (!availableParamKeys.TryPop(out var newParam))
        {
            newParam = Interlocked.Increment(ref nextParamKey);
            if (newParam >= BASE_PARAM_KEY + MAX_HANDLERS)
            {
                Interlocked.Decrement(ref nextParamKey);
                throw new Exception("事件处理器过多 (≥ 100 万)");
            }
        }

        if (!eventHandlers.TryAdd(newParam, eventWrapper))
        {
            availableParamKeys.Push(newParam);
            throw new Exception($"注册事件失败, 回收的 ID 发生重复碰撞: {newParam}");
        }

        return newParam;
    }

    internal void UnregisterEvent(uint paramKey)
    {
        if (!eventHandlers.TryRemove(paramKey, out _)) return;
        
        availableParamKeys.Push(paramKey);
    }
    
    
    private static readonly CompSig ReceiveGlobalEventSig = new("48 89 5C 24 ?? 55 57 41 57 48 83 EC 50 48 8B D9");
    private delegate        void ReceiveGlobalEventDelegate(AtkUnitBase* addon, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* data);
    private                 Hook<ReceiveGlobalEventDelegate>? ReceiveGlobalEventHook;

    private readonly ConcurrentDictionary<uint, AtkEventWrapper> eventHandlers      = [];
    private readonly ConcurrentStack<uint>                       availableParamKeys = [];
    
    private const uint BASE_PARAM_KEY = 0x53540000U;
    private const uint MAX_HANDLERS   = 1000000;
    
    private uint nextParamKey = BASE_PARAM_KEY - 1;
    
    internal override void Init()
    {
        ReceiveGlobalEventHook ??= ReceiveGlobalEventSig.GetHook<ReceiveGlobalEventDelegate>(ReceiveGlobalEventDetour);
        ReceiveGlobalEventHook.Enable();
    }

    internal override void Uninit()
    {
        ReceiveGlobalEventHook?.Dispose();
        ReceiveGlobalEventHook = null;

        foreach (var (_, atkEvent) in eventHandlers)
            atkEvent.Dispose();
        
        eventHandlers.Clear();
        availableParamKeys.Clear();
    }

    private void ReceiveGlobalEventDetour(AtkUnitBase* addon, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* data)
    {
        if (addon    == null ||
            atkEvent == null ||
            data     == null)
            return;
        
        if (eventHandlers.TryGetValue((uint)eventParam, out var simpleEvent))
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
        this.ParamKey = AtkEventManager.Instance().RegisterEvent(this);
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

        AtkEventManager.Instance().UnregisterEvent(ParamKey);
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
