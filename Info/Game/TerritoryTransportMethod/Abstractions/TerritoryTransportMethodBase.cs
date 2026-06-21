using FFXIVClientStructs.FFXIV.Client.UI;
using OmenTools.OmenService;
using OmenTools.Threading.TaskHelper;

namespace OmenTools.Info.Game.TerritoryTransportMethod.Abstractions;

public abstract class TerritoryTransportMethodBase
{
    public abstract uint                SourceZone  { get; }
    public abstract IReadOnlyList<uint> TargetZones { get; }
    
    public abstract string DisplayName { get; }

    public abstract IEnumerable<uint> EnumerateReachableTargets(uint sourceTerritory);

    public abstract void Enqueue(TaskHelper taskHelper, uint targetTerritory);

    public virtual void Cleanup() { }

    #region 工具

    protected static Func<bool> WaitForZoneReady(uint zone) =>
        () => GameState.TerritoryType == zone &&
              LocalPlayerState.Object != null &&
              UIModule.IsScreenReady();

    #endregion
}
