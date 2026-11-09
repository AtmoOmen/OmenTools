using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Interop.Game.AddonEvent;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.TerritoryTransport.Abstractions;

public abstract class EventSimpleYesTransportBase : EventTransportBase
{
    protected virtual bool UseOriginalEntityID =>
        false;

    protected override unsafe void EnqueueEventTrigger
    (
        TaskHelper taskHelper,
        uint       targetZone,
        int        weight
    )
    {
        taskHelper.Enqueue
        (
            () =>
            {
                if (UseOriginalEntityID)
                {
                    if (!EventFramework.Instance()->TryGetNearestEvent
                        (
                            info => info.EventId.Id == EventID,
                            _ => true,
                            Vector3.Zero,
                            out _,
                            out var gameObjectID
                        ))
                        return false;
                    
                    new EventStartPackt(gameObjectID, EventID).Send();
                    return true;
                }
                
                new EventStartPackt(LocalPlayerState.EntityID, EventID).Send();
                return true;
            },
            "发送交互包",
            weight: weight
        );
        taskHelper.Enqueue
        (
            () =>
            {
                AddonTalkEvent.ClickNext();
                return AddonSelectYesnoEvent.ClickYes();
            },
            "进入目标区域",
            weight: weight
        );
    }
}
