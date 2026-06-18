using OmenTools.Info.Game.TerritoryTransportMethod.Abstractions;

namespace OmenTools.Info.Game.TerritoryTransportMethod;

public static class TerritoryTransportGraph
{
    public static IReadOnlyList<TransportHop>? FindPath
    (
        IReadOnlyList<TerritoryTransportMethodBase> methods,
        uint                                        source,
        uint                                        goal
    )
    {
        if (source == goal) return [];

        var visited  = new HashSet<uint> { source };
        var frontier = new Queue<(uint Territory, List<TransportHop> Path)>();
        frontier.Enqueue((source, []));

        while (frontier.Count > 0)
        {
            var (current, path) = frontier.Dequeue();

            foreach (var method in methods)
            {
                foreach (var next in method.EnumerateReachableTargets(current))
                {
                    if (!visited.Add(next)) continue;

                    var newPath = new List<TransportHop>(path.Count + 1);
                    newPath.AddRange(path);
                    newPath.Add(new(method, next));

                    if (next == goal) return newPath;
                    frontier.Enqueue((next, newPath));
                }
            }
        }

        return null;
    }
}
