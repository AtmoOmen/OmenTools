using OmenTools.Info.Game.ItemSource.Enums;

namespace OmenTools.Info.Game.ItemSource.Models;

public readonly record struct ExchangeItemsQueryResult
(
    ItemSourceQueryState State,
    ExchangeItemsInfo?   Data = null
)
{
    public static ExchangeItemsQueryResult Ready(ExchangeItemsInfo data) =>
        new(ItemSourceQueryState.Ready, data);

    public static ExchangeItemsQueryResult Building { get; } =
        new(ItemSourceQueryState.Building);

    public static ExchangeItemsQueryResult NotFound { get; } =
        new(ItemSourceQueryState.NotFound);

    public static ExchangeItemsQueryResult Failed { get; } =
        new(ItemSourceQueryState.Failed);
}
