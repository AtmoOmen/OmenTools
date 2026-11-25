namespace OmenTools.Interop.Game.Models;

/// <summary>
///     试穿室装备试穿条目。
/// </summary>
/// <param name="ItemID">物品 ID，支持传入原始或 HQ / 收藏品 ID。</param>
/// <param name="GlamourID">幻化外观物品 ID，为 0 表示不使用幻化外观。</param>
/// <param name="Stain0ID">染色通道 1 的染剂 ID。</param>
/// <param name="Stain1ID">染色通道 2 的染剂 ID。</param>
/// <param name="EquipSlot">指定装备槽位索引（0–4、6–12），为 <see langword="null"/> 时交由客户端自动解析。</param>
/// <param name="ApplyCompanyCrest">是否应用部队徽章。</param>
public readonly record struct TryOnEntry
(
    uint  ItemID,
    uint  GlamourID         = 0,
    byte  Stain0ID          = 0,
    byte  Stain1ID          = 0,
    byte? EquipSlot         = null,
    bool  ApplyCompanyCrest = false
);
