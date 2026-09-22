using OmenTools.Info.Game.Packets.Upstream;
using OmenTools.Interop.Game.AddonEvent;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.TerritoryTransport.Abstractions;

public abstract class EventSimpleYesTransportBase : EventTransportBase
{
    protected override void EnqueueEventTrigger
    (
        TaskHelper taskHelper,
        uint       targetZone,
        int        weight
    )
    {
        taskHelper.Enqueue
        (
            () => new EventStartPackt(LocalPlayerState.EntityID, EventID).Send(),
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
