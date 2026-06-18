using OmenTools.Threading.TaskHelper;

namespace OmenTools.Info.Game.TerritoryTransportMethod.Abstractions;

public abstract class TerritoryTransportMethodBase
{
    public abstract uint                SourceZone  { get; }
    public abstract IReadOnlyList<uint> TargetZones { get; }
    
    public abstract string DisplayName { get; }

    public abstract bool CanTransport(uint sourceTerritory, uint targetTerritory);

    public abstract IEnumerable<uint> EnumerateReachableTargets(uint sourceTerritory);

    public virtual bool CanExecute
    {
        get => !DService.Instance().Condition.IsBoundByDuty &&
               !DService.Instance().Condition.IsBetweenAreas;
    }

    public abstract void Enqueue(TaskHelper taskHelper, uint targetTerritory);

    public abstract string Describe(uint targetTerritory);

    public virtual void Cleanup() { }
}
