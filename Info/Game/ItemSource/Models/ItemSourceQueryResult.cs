using OmenTools.Info.Game.ItemSource.Enums;

namespace OmenTools.Info.Game.ItemSource.Models;

public readonly record struct ItemSourceQueryResult
(
    ItemSourceQueryState State,
    ItemSourceInfo?      Data = null
)
{
    public static ItemSourceQueryResult Ready(ItemSourceInfo data) =>
        new(ItemSourceQueryState.Ready, data);

    public static ItemSourceQueryResult Building { get; } =
        new(ItemSourceQueryState.Building);

    public static ItemSourceQueryResult NotFound { get; } =
        new(ItemSourceQueryState.NotFound);

    public static ItemSourceQueryResult Failed { get; } =
        new(ItemSourceQueryState.Failed);
}
