namespace OmenTools.OmenService;

[Flags]
public enum TooltipItemGroupFlags
{
    None = 0,

    /// <summary>
    ///     工匠名称 (StringArray index 26)
    /// </summary>
    CrafterName = 1 << 0,

    /// <summary>
    ///     物品描述 (StringArray index 13)
    /// </summary>
    Description = 1 << 1,

    /// <summary>
    ///     市场出售信息 (StringArray index 25)
    /// </summary>
    Marketable = 1 << 2,

    /// <summary>
    ///     装备限制 — 标准物品模式 (StringArray index 22, 23)
    ///     仅在 HeaderStatsGroup 不可见时生效 (flag 0x80 未设置)
    /// </summary>
    EquipRestriction = 1 << 3,

    /// <summary>
    ///     效果 / 属性加成 (StringArray index 15, 16)
    /// </summary>
    Bonuses = 1 << 4,

    /// <summary>
    ///     魔晶石信息 (StringArray index 17-19, 52-62)
    /// </summary>
    Materia = 1 << 5,

    /// <summary>
    ///     制作与修理信息 (StringArray index 28-35)
    /// </summary>
    CraftingAndRepairs = 1 << 6,

    /// <summary>
    ///     HeaderStatsGroup 可见模式
    ///     决定 UpdateGroupPositions 使用哪条布局路径
    /// </summary>
    HeaderStatsGroup = 1 << 7,

    /// <summary>
    ///     特殊效果 (StringArray index 36-41)
    ///     仅在 <see cref="HeaderStatsGroup" /> 模式时生效
    /// </summary>
    Effects = 1 << 8,

    /// <summary>
    ///     新手 / 雏鸟提示
    /// </summary>
    Fledgling = 1 << 9,

    /// <summary>
    ///     装备限制 — HQ / 收藏品 / HeaderStats 模式 (StringArray index 22, 23)
    ///     仅在 <see cref="HeaderStatsGroup" /> 模式时生效
    /// </summary>
    EquipRestrictionHeader = 1 << 15,

    /// <summary>
    ///     商店售价 (StringArray index 63)
    /// </summary>
    ShopSellingPrice = 1 << 19
}
