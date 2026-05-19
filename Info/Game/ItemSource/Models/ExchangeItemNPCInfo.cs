namespace OmenTools.Info.Game.ItemSource.Models;

public sealed class ExchangeItemNPCInfo
{
    public uint                   ID        { get; init; }
    public string                 Name      { get; init; } = string.Empty;
    public string?                ShopName  { get; init; }
    public List<ShopItemCostInfo> CostInfos { get; init; } = [];
    public ShopNPCLocation        Location  { get; init; }
}
