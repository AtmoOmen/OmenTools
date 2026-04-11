namespace OmenTools.Info.Game.ItemSource.Models;

public class ShopNPCInfos
{
    public uint                   ID        { get; init; }
    public string                 Name      { get; init; }
    public string?                ShopName  { get; init; }
    public List<ShopItemCostInfo> CostInfos { get; init; }
    public ShopNPCLocation        Location  { get; init; }
}
