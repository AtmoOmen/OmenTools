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

    private const  uint BaseParamKey = 0x1000000U;
    private const  uint MaxHandlers  = 1000000;
    private static uint NextParamKey = BaseParamKey - 1;
    
    private static readonly ConcurrentDictionary<uint, AtkEventWrapper> EventHandlers = [];
    
    internal override void Init()
    {
        GlobalEventHook ??= GlobalEventSig.GetHook<GlobalEventDelegate>(GlobalEventDetour);
        GlobalEventHook.Enable();
    }

    internal override void Uninit()
    {
        GlobalEventHook?.Dispose();
        GlobalEventHook = null;
        
        EventHandlers.Clear();
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

    internal static uint RegisterEvent(AtkEventWrapper atkEventWrapper)
    {
        var newParam = Interlocked.Increment(ref NextParamKey);

        if (newParam >= BaseParamKey + MaxHandlers)
        {
            Interlocked.Decrement(ref NextParamKey);
            throw new Exception("事件处理器过多 (≥ 100 万)");
        }

        EventHandlers.TryAdd(newParam, atkEventWrapper);
        return newParam;
    }

    internal static void UnregisterEvent(uint paramKey) =>
        EventHandlers.TryRemove(paramKey, out _);
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

    public AtkEventWrapper(AtkEventActionDelegate action)
    {
        this.Action = action;
        this.ParamKey = AtkEventManager.RegisterEvent(this);
    }
    
    public void Dispose()
    {
        AtkEventManager.UnregisterEvent(this.ParamKey);
        GC.SuppressFinalize(this);
    }
    
    public void Add(AtkUnitBase* addon, AtkResNode* node, AtkEventType eventType) => 
        node->AddEvent(eventType, ParamKey, (AtkEventListener*)addon, node, true);

    public void Remove(AtkUnitBase* addon, AtkResNode* node, AtkEventType eventType) => 
        node->RemoveEvent(eventType, ParamKey, (AtkEventListener*)addon, true);
}
