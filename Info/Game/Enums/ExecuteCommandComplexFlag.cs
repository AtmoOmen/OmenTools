namespace OmenTools.Info.Game.Enums;

public enum ExecuteCommandComplexFlag
{
    /// <summary>
    ///     设置软目标
    /// </summary>
    /// <remarks>
    ///     <para><c>target</c>: 软目标 Entity ID, 无目标为 <c>0xE0000000</c></para>
    /// </remarks>
    SetSoftTarget = 4,

    /// <summary>
    ///     空中下坐骑
    /// </summary>
    /// <remarks>
    ///     <para><c>location</c>: 落点</para>
    ///     <para><c>param1</c>: 玩家旋转封包值</para>
    ///     <para><c>param2</c>: 客户端递增序列号</para>
    /// </remarks>
    Dismount = 101,

    /// <summary>
    ///     完成过图线传送
    /// </summary>
    /// <remarks>
    ///     <para><c>location</c>: 校正后的本地玩家位置</para>
    /// </remarks>
    CompleteZoneLineWarp = 208,

    /// <summary>
    ///     进入不可见出口范围
    /// </summary>
    /// <remarks>
    ///     <para><c>location</c>: 触发出口时记录的位置</para>
    ///     <para><c>param1</c>: ExitRange InstanceKey</para>
    /// </remarks>
    EnterInvisibleExitRange = 209,

    /// <summary>
    ///     进入拉拉菲尔专用不可见出口范围
    /// </summary>
    /// <remarks>
    ///     <para><c>location</c>: 触发出口时记录的位置</para>
    ///     <para><c>param1</c>: ExitRange InstanceKey</para>
    /// </remarks>
    EnterLalafellOnlyExitRange = 212,

    /// <summary>
    ///     设置对象标记
    /// </summary>
    /// <remarks>
    ///     <para><c>target</c>: 被标记对象的 Entity ID, <c>0xE0000000</c> 表示清除</para>
    ///     <para><c>param1</c>: 标记索引, 从 <c>0</c> 开始</para>
    /// </remarks>
    SetObjectMarker = 301,

    /// <summary>
    ///     按 Content ID 请求角色名称
    /// </summary>
    /// <remarks>
    ///     <para><c>target</c>: Content ID</para>
    /// </remarks>
    RequestNameByContentID = 305,

    /// <summary>
    ///     使用情感动作
    /// </summary>
    /// <remarks>
    ///     <para><c>target</c>: 目标 Entity ID</para>
    ///     <para><c>param1</c>: Emote ID</para>
    ///     <para><c>param3</c>: 情感动作执行标志</para>
    /// </remarks>
    Emote = 500,

    /// <summary>
    ///     在位置使用情感动作
    /// </summary>
    /// <remarks>
    ///     <para><c>location</c>: 玩家位置</para>
    ///     <para><c>param1</c>: Emote ID</para>
    ///     <para><c>param2</c>: 固定为 <c>0</c></para>
    ///     <para><c>param3</c>: 情感动作执行标志</para>
    ///     <para><c>param4</c>: 玩家旋转封包值</para>
    /// </remarks>
    EmoteLocation = 501,

    /// <summary>
    ///     在位置打断当前情感动作
    /// </summary>
    /// <remarks>
    ///     <para><c>location</c>: 玩家位置</para>
    ///     <para><c>param2</c>: 玩家旋转封包值</para>
    /// </remarks>
    EmoteInterruptLocation = 504,

    /// <summary>
    ///     服务器强制移动
    /// </summary>
    /// <remarks>
    ///     <para><c>location</c>: 强制移动的目标位置</para>
    ///     <para><c>param1</c>: 玩家旋转封包值</para>
    /// </remarks>
    ForcedMovement = 603,

    /// <summary>
    ///     结束潜水
    /// </summary>
    /// <remarks>
    ///     <para><c>location</c>: 目标位置</para>
    ///     <para><c>param1</c>: 玩家旋转封包值</para>
    ///     <para><c>param2</c>: 飞行坐骑位置校正标志</para>
    /// </remarks>
    DiveEnd = 607,

    /// <summary>
    ///     设置潜水状态
    /// </summary>
    /// <remarks>
    ///     <para><c>location</c>: 玩家当前位置</para>
    ///     <para><c>param1</c>: 目标潜水状态, <c>1</c> 为进入, <c>0</c> 为退出</para>
    /// </remarks>
    SetDivingState = 610,

    /// <summary>
    ///     与事件处理器交互
    /// </summary>
    /// <remarks>
    ///     <para><c>target</c>: 交互对象的 Entity ID</para>
    ///     <para><c>param1</c>: Event ID</para>
    ///     <para><c>param2</c>: 选择器选项参数</para>
    /// </remarks>
    InteractWithEventHandler = 815,

    /// <summary>
    ///     将战利品分配给队员
    /// </summary>
    /// <remarks>
    ///     <para><c>target</c>: 受领队员的 Entity ID</para>
    ///     <para><c>param1</c>: LootItem ChestObjectID</para>
    ///     <para><c>param2</c>: LootItem ChestItemIndex</para>
    /// </remarks>
    AssignLootToPlayer = 1251,

    /// <summary>
    ///     召唤物技能
    /// </summary>
    /// <remarks>
    ///     <para><c>target</c>: 目标 Entity ID, 无目标为 <c>0xE0000000</c></para>
    ///     <para><c>location</c>: 范围目标位置</para>
    ///     <para><c>param1</c>: PetAction ID</para>
    /// </remarks>
    PetAction = 1800,

    /// <summary>
    ///     冒险者分队技能
    /// </summary>
    /// <remarks>
    ///     <para><c>target</c>: 目标 Entity ID</para>
    ///     <para><c>param1</c>: BgcArmyAction ID</para>
    /// </remarks>
    SquadronAction = 1810,

    /// <summary>
    ///     萌宠之王小宠物移动
    /// </summary>
    /// <remarks>
    ///     <para><c>location</c>: 小宠物单位的目标位置</para>
    ///     <para><c>param1</c>: 小宠物单位 Entity ID</para>
    ///     <para><c>param2</c>: 当前选中的小宠物单位总数</para>
    /// </remarks>
    VerminionMove = 2000
}
