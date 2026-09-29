using FFXIVClientStructs.FFXIV.Component.GUI;
using AtkEventManager = OmenTools.OmenService.AtkEventManager;

namespace OmenTools.Interop.Game.Models;

public unsafe class AtkEventWrapper : IDisposable
{
    public delegate void AtkEventActionDelegate
    (
        AtkEventType  eventType,
        AtkUnitBase*  addon,
        AtkEvent*     atkEvent,
        AtkEventData* data
    );

    /// <summary>
    ///     事件触发时要执行的回调。
    /// </summary>
    public AtkEventActionDelegate Action { get; }

    /// <summary>
    ///     唯一事件ID
    /// </summary>
    public uint ParamKey { get; }

    private readonly List<(nint AddonPtr, nint NodePtr, AtkEventType Type)> registeredData = [];

    private readonly Lock dataLock = new();

    private bool isDisposed;

    public AtkEventWrapper
    (
        AtkEventActionDelegate action
    )
    {
        Action   = action;
        ParamKey = AtkEventManager.Instance().RegisterEvent(this);
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        RemoveRegisteredEvents();

        AtkEventManager.Instance().UnregisterEvent(ParamKey);
        GC.SuppressFinalize(this);
    }

    public void Add
    (
        AtkUnitBase* addon,
        AtkResNode*  node,
        AtkEventType eventType
    )
    {
        using var scope = dataLock.EnterScope();

        if (isDisposed) return;

        node->AddEvent(eventType, ParamKey, (AtkEventListener*)addon, node, true);
        registeredData.Add(((nint)addon, (nint)node, eventType));
    }

    public void Remove
    (
        AtkUnitBase* addon,
        AtkResNode*  node,
        AtkEventType eventType
    )
    {
        using var scope = dataLock.EnterScope();

        if (isDisposed) return;

        node->RemoveEvent(eventType, ParamKey, (AtkEventListener*)addon, true);
        registeredData.Remove(((nint)addon, (nint)node, eventType));
    }

    internal bool IsBoundToAddon
    (
        nint addonPtr
    )
    {
        using var scope = dataLock.EnterScope();

        foreach (var (registeredAddonPtr, _, _) in registeredData)
        {
            if (registeredAddonPtr == addonPtr)
                return true;
        }

        return false;
    }

    private void RemoveRegisteredEvents()
    {
        using var scope = dataLock.EnterScope();

        foreach (var (addonPtr, nodePtr, type) in registeredData)
        {
            var addon = (AtkUnitBase*)addonPtr;
            var node  = (AtkResNode*)nodePtr;

            if (addon == null || node == null || !addon->IsFullyLoaded())
                continue;

            node->RemoveEvent(type, ParamKey, (AtkEventListener*)addon, true);
        }

        registeredData.Clear();
    }
}
