namespace OmenTools.Infos;

public enum ExecuteCommandComplexFlag
{
    /// <summary>
    /// 低空飞行下坐骑 (场地)
    /// </summary>
    /// <remarks>
    /// <para><c>location</c>: 目标位置</para>
    /// <para><c>param1</c>: 玩家旋转角度</para>
    /// <para><c>param2</c>: 未知, 一直为 1</para>
    /// <para><c>param2</c>: 未知, 一直为 0</para>
    /// </remarks>
    Dismount = 101,

    /// <summary>
    /// 未知
    /// </summary>
    Unk208 = 208,
    
    /// <summary>
    /// 潜水通过 (场地)
    /// </summary>
    /// <remarks>
    /// <para><c>location</c>: 目标位置</para>
    /// <para><c>param1</c>: 玩家旋转角度</para>
    /// </remarks>
    DiveThrough = 209,
    
    /// <summary>
    /// 未知
    /// </summary>
    Unk212 = 212,
    
    /// <summary>
    /// 放置目标标记
    /// </summary>
    /// <remarks>
    /// <para><c>target</c>: 目标 Entity ID</para>
    /// <para><c>param1</c>: 目标标记索引 (从 0 开始)</para>
    /// </remarks>
    PlaceMarker = 301,

    /// <summary>
    /// 使用情感动作
    /// </summary>
    /// <remarks>
    /// <para><c>target</c>: 目标 Entity ID</para>
    /// <para><c>param1</c>: Emote ID</para>
    /// <para><c>param3</c>: 是否发送情感动作消息 (1 - 不发送, 0 - 发送)</para>
    /// </remarks>
    Emote = 500,

    /// <summary>
    /// 使用情感动作 (场地)
    /// </summary>
    /// <remarks>
    /// <para><c>location</c>: 目标位置</para>
    /// <para><c>param1</c>: Emote ID</para>
    /// <para><c>param2</c>: 角度</para>
    /// <para><c>param4</c>: 玩家旋转角度</para>
    /// </remarks>
    EmoteLocation = 501,

    /// <summary>
    /// 打断当前情感动作 (场地)
    /// </summary>
    /// <para><c>location</c>: 目标位置</para>
    /// <para><c>param2</c>: Rotation Packet</para>
    EmoteInterruptLocation = 504,

    /// <summary>
    /// 未知 (场地)
    /// </summary>
    /// <para><c>location</c>: 位置</para>
    /// <para><c>param1</c>: Rotation Packet</para>
    /// <para><c>param2</c>: 未知</para>
    /// <para><c>param3</c>: 未知</para>
    Unk603 = 603,
    
    /// <summary>
    /// 潜水结束 (场地)
    /// </summary>
    /// <remarks>
    /// <para><c>location</c>: 目标位置</para>
    /// <para><c>param1</c>: Rotation Packet</para>
    /// <para><c>param2</c>: 玩家是否在坐骑上 (1 - 是, 0 - 否)</para>
    /// </remarks>
    DiveEnd = 607,
    
    /// <summary>
    /// 非法潜水 → 回到当前地图的出生点
    /// </summary>
    /// <para><c>location</c>: 当前位置 (似乎不影响)</para>
    /// <para><c>param1</c>: 未知 (似乎不影响)</para>
    DiveInvalid = 610,

    /// <summary>
    /// 召唤物技能
    /// </summary>
    /// <remarks>
    /// <para><c>target/location</c>: 0xE0000000 / 目的地位置 (仅移动)</para>
    /// <para><c>param1</c>: Pet Action ID</para>
    /// </remarks>
    PetAction = 1800,

    /// <summary>
    /// 冒险者分队技能
    /// </summary>
    /// <remarks>
    /// <para><c>target</c>: 目标 Entity ID</para>
    /// <para><c>param1</c>: BgcArmyAction ID</para>
    /// </remarks>
    BgcArmyAction = 1810,
    
    /// <summary>
    /// 未知 (场地)
    /// </summary>
    /// <para><c>location</c>: 位置</para>
    /// <para><c>param1</c>: Entity ID</para>
    Unk2000 = 2000,
}
