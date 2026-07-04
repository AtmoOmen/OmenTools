using OmenTools.Interop.Game.ExecuteCommand.Implementations;

namespace OmenTools.Info.Game.Enums;

// ReSharper disable InconsistentNaming
public enum ExecuteCommandFlag
{
    /// <summary>
    ///     拔出/收回武器
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 1 - 拔出, 0 - 收回</para>
    ///     <para><c>param2</c>: 是否立即操作 (1 - 是, 0 - 否)</para>
    /// </remarks>
    /// <seealso cref="WeaponCommand" />
    ToggleWeapon = 1,

    /// <summary>
    ///     自动攻击
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 是否开启自动攻击 (0 - 否, 1 - 是)</para>
    ///     <para><c>param2</c>: 目标对象 ID</para>
    ///     <para><c>param3</c>: 是否为手动操作 (0 - 否, 1 - 是)</para>
    /// </remarks>
    /// <seealso cref="AutoAttackCommand" />
    ToggleAutoAttack = 2,

    /// <summary>
    ///     选中目标
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 目标 Entity ID (无目标为: 0xE0000000)</para>
    /// </remarks>
    /// <seealso cref="TargetCommand" />
    Target = 3,

    /// <summary>
    ///     PVP 快捷发言
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: QuickChat Row ID</para>
    ///     <para><c>param2</c>: 参数 1</para>
    ///     <para><c>param3</c>: 参数 2</para>
    /// </remarks>
    /// <seealso cref="PVPCommand" />
    SnedPVPQuickChat = 5,

    /// <summary>
    ///     游戏管理员指令
    /// </summary>
    GMCommand11 = 11,

    /// <summary>
    ///     下坐骑
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 0 - 不进入队列; 1 - 进入队列</para>
    /// </remarks>
    /// <seealso cref="MountCommand" />
    Dismount = 101,

    /// <summary>
    ///     召唤宠物
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 宠物 ID</para>
    /// </remarks>
    /// <seealso cref="MinionCommand" />
    SummonMinion = 102,

    /// <summary>
    ///     收回宠物
    /// </summary>
    WithdrawMinion = 103,

    /// <summary>
    ///     取消身上指定的状态
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Status ID</para>
    ///     <para><c>param3</c>: Status 来源的 GameObjectID，亦可用 0xE0000000 指定清除任意来源的首个该状态 </para>
    /// </remarks>
    /// <seealso cref="StatusCommand" />
    RemoveStatus = 104,

    /// <summary>
    ///     中断咏唱
    /// </summary>
    CancelCast = 105,

    /// <summary>
    ///     共同骑乘 (指定座位)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 骑乘者的 EntityID</para>
    ///     <para><c>param2</c>: 座位索引</para>
    /// </remarks>
    /// <seealso cref="MountCommand" />
    RidePillion = 106,

    /// <summary>
    ///     共同骑乘 (自动分配座位)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 骑乘者的 EntityID</para>
    /// </remarks>
    /// <seealso cref="MountCommand" />
    RidePillionAuto = 107,

    /// <summary>
    ///     请求加载小队成员角色数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 索引 (0 至 6)</para>
    ///     <para><c>param2</c>: 成员的 EntityID</para>
    /// </remarks>
    LoadPartyMember = 108,

    /// <summary>
    ///     因执行其他动作或不满足条件, 而被强行收起时尚配饰
    /// </summary>
    WithdrawParasolForced = 109,

    /// <summary>
    ///     主动收起时尚配饰
    /// </summary>
    WithdrawParasol = 110,

    /// <summary>
    ///     若已设置时尚配饰, 则根据当前情况使用/收回时尚配饰
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 时尚配饰 ID (0 表示收回当前配饰)</para>
    /// </remarks>
    UpdateParasolState = 111,

    /// <summary>
    ///     设置要自动时尚配饰
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 时尚配饰 ID (0 表示取消选择)</para>
    /// </remarks>
    SetParasolToAutoUse = 112,

    /// <summary>
    ///     复活
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 操作 (5 - 接受复活; 8 - 确认返回返回点)</para>
    /// </remarks>
    /// <seealso cref="ReviveCommand" />
    Revive = 200,

    /// <summary>
    ///     区域变更
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 变更方式</para>
    ///     <list type="table">
    ///         <item>
    ///             <term>1</term>
    ///             <description>NPC 传送</description>
    ///         </item>
    ///         <item>
    ///             <term>3</term>
    ///             <description>边界过图</description>
    ///         </item>
    ///         <item>
    ///             <term>4</term>
    ///             <description>正常传送</description>
    ///         </item>
    ///         <item>
    ///             <term>7</term>
    ///             <description>返回</description>
    ///         </item>
    ///         <item>
    ///             <term>15</term>
    ///             <description>城内以太之晶</description>
    ///         </item>
    ///         <item>
    ///             <term>20</term>
    ///             <description>房区</description>
    ///         </item>
    ///     </list>
    ///     <para><c>param2</c>: 区域内位置变更方式</para>
    ///     <list type="table">
    ///         <item>
    ///             <term>1</term>
    ///             <description>剧情转移</description>
    ///         </item>
    ///         <item>
    ///             <term>2</term>
    ///             <description>返回到安全区</description>
    ///         </item>
    ///         <item>
    ///             <term>25</term>
    ///             <description>副本内过图</description>
    ///         </item>
    ///         <item>
    ///             <term>26</term>
    ///             <description>潜水</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="TerritoryCommand" />
    StartTerritoryTransport = 201,

    /// <summary>
    ///     传送至指定的以太之光
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 以太之光 ID</para>
    ///     <para><c>param2</c>: 是否使用传送券 (0 - 否, 1 - 是)</para>
    ///     <para><c>param3</c>: 以太之光 Sub ID</para>
    /// </remarks>
    /// <seealso cref="TeleportCommand" />
    Teleport = 202,

    /// <summary>
    ///     接受传送邀请
    /// </summary>
    AcceptTeleportOffer = 203,

    /// <summary>
    ///     取消传送
    /// </summary>
    CancelTeleport = 204,

    /// <summary>
    ///     拒绝复活请求
    /// </summary>
    RejectRevive = 205,

    /// <summary>
    ///     未知大型副本事件命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Event Kind</para>
    ///     <para><c>param2</c>: 未知</para>
    /// </remarks>
    PublicContentCommand206 = 206,

    /// <summary>
    ///     未知传送命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知参数的高 32 位</para>
    ///     <para><c>param2</c>: 未知参数</para>
    ///     <para><c>param3</c>: 未知参数</para>
    /// </remarks>
    TeleportCommand207 = 207,

    /// <summary>
    ///     请求好友房屋传送信息
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: AgentFriendlist.Instance() 的 + 160 偏移处的高 32 位</para>
    ///     <para><c>param2</c>: AgentFriendlist.Instance() 的 + 160 偏移处</para>
    /// </remarks>
    RequestFriendHousingTeleportInfo = 210,

    /// <summary>
    ///     传送至好友房屋
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: AgentTeleportHousingFriend.Instance() 的一个数组中数据的高 32 位</para>
    ///     <para><c>param2</c>: AgentTeleportHousingFriend.Instance() 的一个数组中数据</para>
    ///     <para><c>param3</c>: 以太之光 ID (个人房屋 - 61, 部队房屋 - 57)</para>
    ///     <para><c>param4</c>: 以太之光 Sub ID (疑似恒定为 1)</para>
    /// </remarks>
    TeleportToFriendHouse = 211,

    /// <summary>
    ///     若当前种族不是拉拉菲尔族, 则返回至当前区域的最近安全点
    /// </summary>
    ReturnToSafePointIfNotLalafell = 213,

    /// <summary>
    ///     若当前种族不是拉拉菲尔族, 则返回至返回点或副本内出生点
    /// </summary>
    ReturnIfNotLalafell = 214,

    /// <summary>
    ///     检查指定玩家
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 待对象 Object ID</para>
    /// </remarks>
    /// <seealso cref="InspectCommand" />
    Inspect = 300,

    /// <summary>
    ///     更改佩戴的称号
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 称号 ID</para>
    /// </remarks>
    /// <seealso cref="TitleCommand" />
    ChangeTitle = 302,

    /// <summary>
    ///     请求称号数据
    /// </summary>
    RequestTitles = 303,

    /// <summary>
    ///     标记已展示过某一新手指南
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HowTo ID</para>
    /// </remarks>
    MarkHowToSeen = 306,

    /// <summary>
    ///     请求过场剧情数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Cutscene ID</para>
    /// </remarks>
    RequestCutsceneInfo307 = 307,

    /// <summary>
    ///     请求挑战笔记具体类别下数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 类别索引 (从 1 开始)</para>
    /// </remarks>
    /// <seealso cref="ContentsNoteCommand" />
    RequestContentsNoteCategory = 310,

    /// <summary>
    ///     未知命令 (似乎与萌宠之王有一些联系)
    /// </summary>
    UnknownCommand312 = 312,

    /// <summary>
    ///     清除场地标点
    /// </summary>
    ClearFieldMarkers = 313,

    /// <summary>
    ///     配置项 AutoChangeCameraMode 相关上报
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, 可能为 1 或 0, 可能为状态值</para>
    ///     <para><c>param2</c>: 未知</para>
    /// </remarks>
    AutoChangeCameraModeCommand314 = 314,

    /// <summary>
    ///     将青魔法师技能交换或应用于有效技能
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 类型 (0 - 应用有效技能, 1 - 交换有效技能)</para>
    ///     <para><c>param2</c>: 格子序号 (从 0 开始, 小于 24)</para>
    ///     <para><c>param3</c>: 技能 ID / 格子序号 (从 0 开始, 小于 24)</para>
    /// </remarks>
    /// <seealso cref="BlueMagicCommand" />
    SetBlueAction = 315,

    /// <summary>
    ///     请求跨界传送数据
    /// </summary>
    RequestWorldTravel = 316,

    /// <summary>
    ///     放置场地标点
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 标点索引</para>
    ///     <para><c>param2</c>: 坐标 X * 1000</para>
    ///     <para><c>param3</c>: 坐标 Y * 1000</para>
    ///     <para><c>param4</c>: 坐标 Z * 1000</para>
    /// </remarks>
    /// <seealso cref="FieldMarkerCommand" />
    PlaceFieldMarker = 317,

    /// <summary>
    ///     移除场地标点
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 标点索引</para>
    /// </remarks>
    /// <seealso cref="FieldMarkerCommand" />
    RemoveFieldMarker = 318,

    /// <summary>
    ///     清除来自木人的仇恨
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 木人的 Object ID</para>
    /// </remarks>
    /// <seealso cref="StrikingDummyCommand" />
    ResetStrikingDummy = 319,

    /// <summary>
    ///     设置当前雇员市场出售物品价格
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 物品 Slot</para>
    ///     <para><c>param2</c>: 物品价格</para>
    /// </remarks>
    /// <seealso cref="RetainerCommand" />
    SetRetainerMarketPrice = 400,

    /// <summary>
    ///     请求讨伐笔记数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 讨伐笔记类别索引 (Agent 里的 byte MonsterNote)</para>
    ///     <para><c>param2</c>: 等级</para>
    ///     <para><c>param3</c>: 未知, Agent 调用似乎始终为 0, 但是也观察到其他不为 0 的情况</para>
    /// </remarks>
    RequestMonsterNote = 401,

    /// <summary>
    ///     清空回收仓库通知
    /// </summary>
    ClearReclaimNotification = 402,

    /// <summary>
    ///     取回全部 1.0 遗产物品或者房屋被拆除时的临时保管在 NPC 处的家具
    /// </summary>
    ReclaimItems = 403,

    /// <summary>
    ///     请求指定物品栏数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: InventoryType</para>
    /// </remarks>
    /// <seealso cref="InventoryCommand" />
    RequestInventory = 404,

    /// <summary>
    ///     在不同的物品栏间移动物品
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 来源 InventoryType</para>
    ///     <para><c>param2</c>: 目标 InventoryType</para>
    /// </remarks>
    MoveItemBetweenInventory = 405,

    /// <summary>
    ///     通知物品移动操作受阻 (主要是部队储物柜在使用, 比如储物柜加载未完成、其他玩家正在使用储物柜)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 来源 InventoryType</para>
    ///     <para><c>param2</c>: 目标 InventoryType</para>
    /// </remarks>
    NotifyBlockedInventoryOperation = 406,

    /// <summary>
    ///     进入镶嵌魔晶石状态
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 物品 ID</para>
    /// </remarks>
    /// <seealso cref="MateriaCommand" />
    EnterMateriaAttachState = 407,

    /// <summary>
    ///     完成镶嵌魔晶石
    /// </summary>
    FinishMateriaAttach = 408,

    /// <summary>
    ///     退出镶嵌魔晶石状态
    /// </summary>
    LeaveMateriaAttachState = 409,

    /// <summary>
    ///     进入魔晶石镶嵌请求状态
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, MateriaRequestManager 里的一个字段</para>
    /// </remarks>
    EnterMateriaAttachRequestState = 410,

    /// <summary>
    ///     离开魔晶石镶嵌请求状态
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, 0 或 1</para>
    ///     <para><c>param2</c>: 未知, 0 或 1</para>
    /// </remarks>
    LeaveMateriaAttachRequestState = 411,

    /// <summary>
    ///     请求帮助镶嵌魔晶石
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 目标 EntityID</para>
    /// </remarks>
    SendMateriaAttachRequest = 412,

    /// <summary>
    ///     为装备贴上/取下部队队徽
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: InventoryType</para>
    ///     <para><c>param2</c>: InventorySlot</para>
    ///     <para><c>param3</c>: 0 - 取下, 1 - 贴上</para>
    /// </remarks>
    ToggleFreeCompanyCrestDecal = 414,

    /// <summary>
    ///     批量为装备中装备贴上/取下部队队徽
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 0 - 取下, 1 - 贴上</para>
    /// </remarks>
    ToggleFreeCompanyCrestDecalBatchEquipped = 415,

    /// <summary>
    ///     批量为装备贴上/取下部队队徽
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 目标范围 (5 - 兵装库; 6 - 物品)</para>
    ///     <para><c>param2</c>: 0 - 取下, 1 - 贴上</para>
    /// </remarks>
    ToggleFreeCompanyCrestDecalBatch = 416,

    /// <summary>
    ///     因职业变更, 新职业不满足需求, 而自动取消魔晶石镶嵌委托
    /// </summary>
    CancelMateriaAttachRequestForced = 418,

    /// <summary>
    ///     完成了特定的物品栏操作 (雇员比较多)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Inventory Type</para>
    ///     <para><c>param2</c>: Inventory Slot</para>
    /// </remarks>
    FinishInventoryOperation = 419,

    /// <summary>
    ///     向部队储物柜存入金币
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 金额</para>
    ///     <para>需要先调用 <see cref="MoveItemBetweenInventory" /></para>
    /// </remarks>
    DepositFreeCompanyChestGil = 420,

    /// <summary>
    ///     从部队储物柜取出金币
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 金额</para>
    ///     <para>需要先调用 <see cref="MoveItemBetweenInventory" /></para>
    /// </remarks>
    WithdrawFreeCompanyChestGil = 421,

    /// <summary>
    ///     请求部队储物柜操作历史记录
    /// </summary>
    RequestFreeCompanyChestLog = 422,

    /// <summary>
    ///     请求收藏柜数据
    /// </summary>
    RequestCabinet = 423,

    /// <summary>
    ///     存入物品至收藏柜
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Cabinet RowID</para>
    /// </remarks>
    /// <seealso cref="CabinetCommand" />
    StoreToCabinet = 424,

    /// <summary>
    ///     从收藏柜中取回物品
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Cabinet RowID</para>
    /// </remarks>
    /// <seealso cref="CabinetCommand" />
    RestoreFromCabinet = 425,

    /// <summary>
    ///     未知收藏柜命令 (尝试执行会报 <c>剧情被中断</c>)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Cabinet ID</para>
    ///     <para><c>param2</c>: Inventory Type</para>
    ///     <para><c>param3</c>: Inventory Slot</para>
    /// </remarks>
    CabinetCommand426 = 426,

    /// <summary>
    ///     请求收藏柜数据完成 (会把标志位设为 1)
    /// </summary>
    FinishCabinetRequest = 427,

    /// <summary>
    ///     接受怪物狩猎通缉令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 在 UI.MobHunt.AvailableMarkId 中的索引</para>
    ///     <para><c>param2</c>: Mark ID</para>
    /// </remarks>
    AcceptMobHuntBill = 428,

    /// <summary>
    ///     放弃怪物狩猎通缉令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 在 UI.MobHunt.AvailableMarkId 中的索引</para>
    ///     <para><c>param2</c>: Mark ID</para>
    /// </remarks>
    AbandonMobHuntBill = 429,

    /// <summary>
    ///     精制魔晶石
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Inventory Type</para>
    ///     <para><c>param2</c>: Inventory Slot</para>
    /// </remarks>
    /// <seealso cref="MateriaCommand" />
    ExtractMateria = 437,

    /// <summary>
    ///     为雇员武具投影
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 目标 Inventory Type</para>
    ///     <para><c>param2</c>: 目标 Inventory Slot</para>
    ///     <para><c>param3</c>: 来源 Inventory Type</para>
    ///     <para><c>param4</c>: 来源 Inventory Slot</para>
    /// </remarks>
    CastRetainerGlamour = 438,

    /// <summary>
    ///     未知投影台指令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Inventory Type</para>
    ///     <para><c>param2</c>: Inventory Slot</para>
    /// </remarks>
    MiragePrismCommand439 = 439,

    /// <summary>
    ///     未知古武天球书卷指令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Inventory Type</para>
    ///     <para><c>param2</c>: Inventory Slot</para>
    /// </remarks>
    RelicSphereCommand440 = 440,

    /// <summary>
    ///     更换套装
    /// </summary>
    ChangeGearset = 441,

    /// <summary>
    ///     恢复被锁定/拦截的物品
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: InventoryType</para>
    ///     <para><c>param2</c>: InventorySlot</para>
    /// </remarks>
    RecoverBlockedItem = 442,

    /// <summary>
    ///     请求陆行鸟鞍囊的数据
    /// </summary>
    RequestSaddleBag = 444,

    /// <summary>
    ///     请求多玛飞地支援物资退还箱物资数据
    /// </summary>
    RequestEnclaveBuyBack = 445,

    /// <summary>
    ///     完成请求多玛飞地支援物资退还箱物资数据
    /// </summary>
    FinishRequestEnclaveBuyBack = 446,

    /// <summary>
    ///     未知物品操作命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Context ID (也可以为 0)</para>
    ///     <para><c>param2</c>: 操作类型 (也可以为 725)</para>
    /// </remarks>
    InventoryOperationCommand449 = 449,

    /// <summary>
    ///     发送修理委托
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 目标 Entity ID</para>
    /// </remarks>
    /// <seealso cref="RepairCommand" />
    SendRepairRequest = 450,

    /// <summary>
    ///     完成修理委托
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, AgentRepairRequest.Instance() 中一个字段</para>
    ///     <para><c>param2</c>: 未知, AgentRepairRequest.Instance() 中一个字段</para>
    ///     <para><c>param3</c>: 未知, AtkValue 传入的一个字段</para>
    /// </remarks>
    /// <seealso cref="RepairCommand" />
    FinishRepairRequest = 451,

    /// <summary>
    ///     开始修理委托
    /// </summary>
    StartRepairRequest = 452,

    /// <summary>
    ///     取消修理委托
    /// </summary>
    /// <seealso cref="RepairCommand" />
    CancelRepairRequest = 453,

    /// <summary>
    ///     确认本地已进行修理委托动作
    /// </summary>
    ConfirmRepairRequest = 454,

    /// <summary>
    ///     装备面部配饰
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Glasses Slot</para>
    ///     <para><c>param2</c>: Glasses ID</para>
    /// </remarks>
    EquipFacewear = 455,

    /// <summary>
    ///     打断当前正在进行的情感动作
    /// </summary>
    InterruptEmote = 502,

    /// <summary>
    ///     打断当前正在进行的特殊情感动作
    /// </summary>
    InterruptEmoteSpecial = 503,

    /// <summary>
    ///     更改闲置状态姿势
    /// </summary>
    /// <remarks>
    ///     <para><c>param2</c>: 姿势索引 (从 0 到 6)</para>
    /// </remarks>
    /// <seealso cref="IdlePostureCommand" />
    SetIdlePosture = 505,

    /// <summary>
    ///     进入闲置状态姿势
    /// </summary>
    /// <remarks>
    ///     <para><c>param2</c>: 姿势索引 (从 0 到 6)</para>
    /// </remarks>
    /// <seealso cref="IdlePostureCommand" />
    EnterIdlePosture = 506,

    /// <summary>
    ///     退出闲置状态姿势
    /// </summary>
    ExitIdlePosture = 507,

    /// <summary>
    ///     清理固定路径跳跃状态
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知 (看起来似乎是内部计数器)</para>
    /// </remarks>
    CleanupGimmickJumpState602 = 602,

    /// <summary>
    ///     未知玩家控制命令
    /// </summary>
    ControlCommand604 = 604,

    /// <summary>
    ///     未知玩家控制指令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: X 坐标</para>
    ///     <para><c>param2</c>: Y 坐标</para>
    ///     <para><c>param3</c>: Z 坐标</para>
    ///     <para><c>param4</c>: Character Rotation</para>
    /// </remarks>
    ControlCommand605 = 605,

    /// <summary>
    ///     未知玩家控制指令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知 (甚至找不到赋值的地方在哪)</para>
    /// </remarks>
    ControlCommand606 = 606,

    /// <summary>
    ///     进入游泳状态 (也会强制下坐骑)
    /// </summary>
    EnterSwimState = 608,

    /// <summary>
    ///     退出游泳状态
    /// </summary>
    LeaveSwimState = 609,

    /// <summary>
    ///     清理固定路径跳跃状态
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知 (看起来似乎是内部计数器)</para>
    /// </remarks>
    CleanupGimmickJumpState611 = 611,

    /// <summary>
    ///     清理固定路径跳跃状态
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知 (看起来似乎是内部计数器)</para>
    /// </remarks>
    CleanupGimmickJumpState613 = 613,

    /// <summary>
    ///     赋予/取消禁止骑乘坐骑状态
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 0 - 取消; 1 - 赋予</para>
    /// </remarks>
    /// <seealso cref="MountCommand" />
    DisableMounting = 612,

    /// <summary>
    ///     未知控制命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, 0 或 1</para>
    /// </remarks>
    ControlCommand614 = 614,

    /// <summary>
    ///     未知控制命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: X 坐标</para>
    ///     <para><c>param2</c>: Y 坐标</para>
    ///     <para><c>param3</c>: Z 坐标</para>
    /// </remarks>
    ControlCommand615 = 615,

    /// <summary>
    ///     进入飞行状态
    /// </summary>
    EnterFlightState = 616,

    /// <summary>
    ///     生产
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 类型 (0 - 普通制作, 1 - 简易制作; 2 - 制作练习)</para>
    ///     <para><c>param2</c>: Recipe ID</para>
    ///     <para><c>param3</c>: 简易制作 - 数量, 最多 255 个; 制作练习 - 初期品质</para>
    ///     <para><c>param4</c>: 制作练习 - 恒定“通常”制作状态时为 2，其余为 0</para>
    /// </remarks>
    /// <seealso cref="CraftCommand" />
    Craft = 700,

    /// <summary>
    ///     钓鱼
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 动作</para>
    ///     <list type="table">
    ///         <item>
    ///             <term>0</term>
    ///             <description>抛竿</description>
    ///         </item>
    ///         <item>
    ///             <term>1</term>
    ///             <description>收杆</description>
    ///         </item>
    ///         <item>
    ///             <term>2</term>
    ///             <description>提钩</description>
    ///         </item>
    ///         <item>
    ///             <term>3</term>
    ///             <description>以小钓大</description>
    ///         </item>
    ///         <item>
    ///             <term>4</term>
    ///             <description>换饵 — <c>param2</c> Item ID</description>
    ///         </item>
    ///         <item>
    ///             <term>5</term>
    ///             <description>坐下</description>
    ///         </item>
    ///         <item>
    ///             <term>6</term>
    ///             <description>垂钓之光</description>
    ///         </item>
    ///         <item>
    ///             <term>7</term>
    ///             <description>放生</description>
    ///         </item>
    ///         <item>
    ///             <term>9</term>
    ///             <description>撒饵</description>
    ///         </item>
    ///         <item>
    ///             <term>10</term>
    ///             <description>强力提钩</description>
    ///         </item>
    ///         <item>
    ///             <term>11</term>
    ///             <description>精准提钩</description>
    ///         </item>
    ///         <item>
    ///             <term>12</term>
    ///             <description>鱼眼</description>
    ///         </item>
    ///         <item>
    ///             <term>13</term>
    ///             <description>耐心</description>
    ///         </item>
    ///         <item>
    ///             <term>14</term>
    ///             <description>耐心II</description>
    ///         </item>
    ///         <item>
    ///             <term>15</term>
    ///             <description>以小钓大II</description>
    ///         </item>
    ///         <item>
    ///             <term>16</term>
    ///             <description>双重提钩</description>
    ///         </item>
    ///         <item>
    ///             <term>17</term>
    ///             <description>放生列表</description>
    ///         </item>
    ///         <item>
    ///             <term>19</term>
    ///             <description>熟练渔技</description>
    ///         </item>
    ///         <item>
    ///             <term>20</term>
    ///             <description>大鱼猎手</description>
    ///         </item>
    ///         <item>
    ///             <term>21</term>
    ///             <description>三重提钩</description>
    ///         </item>
    ///         <item>
    ///             <term>22</term>
    ///             <description>雄心之饵</description>
    ///         </item>
    ///         <item>
    ///             <term>23</term>
    ///             <description>谦逊之饵</description>
    ///         </item>
    ///         <item>
    ///             <term>24</term>
    ///             <description>熟练妙招</description>
    ///         </item>
    ///         <item>
    ///             <term>25</term>
    ///             <description>游动饵 — <c>param2</c> 饵索引</description>
    ///         </item>
    ///         <item>
    ///             <term>26</term>
    ///             <description>大鱼的知识</description>
    ///         </item>
    ///         <item>
    ///             <term>27</term>
    ///             <description>歇竿</description>
    ///         </item>
    ///         <item>
    ///             <term>28</term>
    ///             <description>未知</description>
    ///         </item>
    ///         <item>
    ///             <term>29</term>
    ///             <description>未知</description>
    ///         </item>
    ///         <item>
    ///             <term>30</term>
    ///             <description>内部通知</description>
    ///         </item>
    ///         <item>
    ///             <term>31</term>
    ///             <description>加钩</description>
    ///         </item>
    ///     </list>
    ///     <para><c>param3</c>: 始终为 0</para>
    ///     <para><c>param4</c>: 始终为 0</para>
    /// </remarks>
    /// <seealso cref="FishingCommand" />
    Fishing = 701,

    /// <summary>
    ///     请求鱼类图鉴数据 (钓鱼)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: FishingNoteInfo ID</para>
    /// </remarks>
    RequestFishingNote = 702,

    /// <summary>
    ///     请求鱼类图鉴数据 (刺鱼)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: FishingNoteInfo ID</para>
    /// </remarks>
    RequestSpearfishNote = 703,

    /// <summary>
    ///     未知任务命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, 1 或 2</para>
    ///     <para><c>param2</c>: 未知</para>
    ///     <para><c>param3</c>: 未知</para>
    /// </remarks>
    QuestCommand704 = 704,

    /// <summary>
    ///     标记上次阅读到的任务
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, 恒为 1</para>
    ///     <para><c>param2</c>: Quest ID (ushort)</para>
    /// </remarks>
    SetLastReadQuest = 705,

    /// <summary>
    ///     请求采集点数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: GatheringPoint ID</para>
    /// </remarks>
    RequestGatheringPoint = 706,

    /// <summary>
    ///     将采集笔记指定分区指定等级区间标记为已发现过
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Division Index</para>
    ///     <para><c>param2</c>: LevelRange Index</para>
    /// </remarks>
    MarkGatherDivisionLevelRangeSeen = 708,

    /// <summary>
    ///     将制作笔记指定分区指定等级区间标记为已发现过
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Division Index</para>
    ///     <para><c>param2</c>: LevelRange Index</para>
    /// </remarks>
    MarkCraftDivisionLevelRangeSeen = 711,

    /// <summary>
    ///     中止/完成简易制作
    /// </summary>
    LeaveQuickSynthesis = 712,

    /// <summary>
    ///     未知刺鱼命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: Action ID</para>
    ///     <para><c>param3</c>: 未知</para>
    /// </remarks>
    SpearFishingCommand713 = 713,

    /// <summary>
    ///     未知刺鱼命令 (似乎和刺鱼完成有一定关系)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知, 可能为 0, 1, 2</para>
    /// </remarks>
    SpearFishingCommand714 = 714,

    /// <summary>
    ///     标记出叉相关技能被使用 (刺鱼、电水流)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: PerformanceCount 的低 32 位</para>
    ///     <para><c>param2</c>: PerformanceCount 的高 32 位</para>
    /// </remarks>
    MarkSpearFishingActionUsage = 715,

    /// <summary>
    ///     未知刺鱼命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    /// </remarks>
    SpearFishingCommand716 = 716,

    /// <summary>
    ///     未知刺鱼命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    ///     <para><c>param3</c>: 未知</para>
    ///     <para><c>param4</c>: 未知</para>
    /// </remarks>
    SpearFishingCommand717 = 717,

    /// <summary>
    ///     未知刺鱼命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    /// </remarks>
    SpearFishingCommand718 = 718,

    /// <summary>
    ///     放弃任务
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 任务 ID (非 RowID)</para>
    /// </remarks>
    /// <seealso cref="QuestCommand" />
    AbandonQuest = 800,

    /// <summary>
    ///     刷新理符任务状态
    /// </summary>
    RefreshLeveQuest = 801,

    /// <summary>
    ///     放弃理符任务
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 理符任务 ID</para>
    /// </remarks>
    /// <seealso cref="LeveCommand" />
    AbandonLeveQuest = 802,

    /// <summary>
    ///     标记理符任务可被再次接取
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Leve ID</para>
    /// </remarks>
    MarkLeveReadyToAccept = 803,

    /// <summary>
    ///     开始理符任务
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Leve ID</para>
    ///     <para><c>param2</c>: 要提高的等级数</para>
    /// </remarks>
    /// <seealso cref="LeveCommand" />
    StartLeveQuest = 804,

    /// <summary>
    ///     未知军队理符任务命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Quest ID</para>
    /// </remarks>
    CompanyLeveQuestCommand = 805,

    /// <summary>
    ///     请求副本相关数据 (非常广泛, 哪个事件、副本都在用)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    ///     <para><c>param3</c>: 未知</para>
    ///     <para><c>param4</c>: 未知</para>
    /// </remarks>
    RequestContent = 808,

    /// <summary>
    ///     开始指定的临危受命任务
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: FATE ID</para>
    ///     <para><c>param2</c>: 目标 Object ID</para>
    /// </remarks>
    /// <seealso cref="FateCommand" />
    StartFate = 809,

    /// <summary>
    ///     加载临危受命信息
    ///     (在切换地图时会一次性加载完地图内所有 FATE 信息)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: FATE ID</para>
    /// </remarks>
    /// <seealso cref="FateCommand" />
    LoadFate = 810,

    /// <summary>
    ///     进入 临危受命 范围 (若 FATE 在脚底下生成则不会发送该命令)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: FATE ID</para>
    /// </remarks>
    /// <seealso cref="FateCommand" />
    EnterFate = 812,

    /// <summary>
    ///     为 临危受命 等级同步
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: FATE ID</para>
    ///     <para><c>param2</c>: 是否等级同步 (0 - 否, 1 - 是)</para>
    /// </remarks>
    /// <seealso cref="FateCommand" />
    SyncToFateLevel = 813,

    /// <summary>
    ///     临危受命 野怪生成
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 野怪 Entity ID</para>
    /// </remarks>
    /// <seealso cref="FateCommand" />
    LoadFateMob = 814,

    /// <summary>
    ///     区域变更完成
    /// </summary>
    FinishTerritoryTransport = 816,

    /// <summary>
    ///     保存魂武任务性别 (这是什么东西?)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, 位掩码</para>
    ///     <para><c>param2</c>: 未知, 0 或 1</para>
    /// </remarks>
    SaveAnimaWeaponQuestGender = 817,

    /// <summary>
    ///     未知节日任务设置命令 (在区域还没加载好的时候请求)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Festival ID</para>
    ///     <para><c>param2</c>: 未知</para>
    ///     <para><c>param3</c>: 未知</para>
    /// </remarks>
    FestivalQuestWorkCommand818 = 818,

    /// <summary>
    ///     离开副本
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 类型 (0 - 正常退本, 1 - 一段时间未操作)</para>
    /// </remarks>
    /// <seealso cref="DutyCommand" />
    LeaveDuty = 819,

    /// <summary>
    ///     同步本地时区偏移
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: UTC 偏移分钟数</para>
    ///     <para><c>param2</c>: 未知</para>
    ///     <para><c>param3</c>: 未知</para>
    ///     <para><c>param3</c>: 未知</para>
    /// </remarks>
    SyncTimezoneOffset = 820,

    /// <summary>
    ///     未知昔日重现中任务重做命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    /// </remarks>
    QuestRedoCommand821 = 821,

    /// <summary>
    ///     未知昔日重现中任务重做命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    /// </remarks>
    QuestRedoCommand822 = 822,

    /// <summary>
    ///     发送单人任务战斗请求
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 难度 (0 - 通常, 1 - 简单, 2 - 非常简单)</para>
    /// </remarks>
    /// <seealso cref="DutyCommand" />
    StartSoloQuestBattle = 823,

    /// <summary>
    ///     开始昔日重现
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: QuestRedo ID (0 - 退出昔日重现)</para>
    /// </remarks>
    /// <seealso cref="QuestRedoCommand" />
    QuestRedo = 824,

    /// <summary>
    ///     继续先前的昔日重现
    /// </summary>
    ContinueQuestRedo = 825,

    /// <summary>
    ///     删除已有的昔日重新存档
    /// </summary>
    DeleteQuestRedoSave = 826,

    /// <summary>
    ///     初始化昔日重现所需的界面信息
    /// </summary>
    ResetQuestRedoUI = 827,

    /// <summary>
    ///     自动触发的 FATE 等级同步请求
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Fate ID</para>
    ///     <para><c>param2</c>: 0 - 取消同步, 1 - 同步</para>
    /// </remarks>
    SyncToFateLevelAuto = 828,

    /// <summary>
    ///     未知临危受命命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: FATE ID</para>
    /// </remarks>
    FateCommand829 = 829,

    /// <summary>
    ///     刷新物品栏
    /// </summary>
    RefreshInventory = 830,

    /// <summary>
    ///     请求过场剧情数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 过场剧情在 Cutscene.csv 中的对应索引</para>
    /// </remarks>
    RequestCutscene831 = 831,

    /// <summary>
    ///     未知 EventFramework 命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    /// </remarks>
    EventFrameworkCommand832 = 832,

    /// <summary>
    ///     标记 EventTutorial 已阅读
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: EventTutorial ID</para>
    /// </remarks>
    MarkEventTutorialSeen = 833,

    /// <summary>
    ///     请求成就进度数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 成就在 Achievement.csv 中的对应索引</para>
    /// </remarks>
    /// <seealso cref="AchievementCommand" />
    RequestAchievement = 1000,

    /// <summary>
    ///     请求已完成成就概览
    /// </summary>
    RequestCompletedAchievement = 1001,

    /// <summary>
    ///     请求接近达成成就概览 (不含具体成就内容)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, 固定为 1</para>
    /// </remarks>
    /// <seealso cref="AchievementCommand" />
    RequestNearCompletedAchievement = 1002,

    /// <summary>
    ///     未知 ActorControl 命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    /// </remarks>
    ActorControlCommand1003 = 1003,

    /// <summary>
    ///     根据页面索引请求相关 FATE 关联成就数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 页面索引</para>
    /// </remarks>
    RequestFateProgressAchievement = 1009,

    /// <summary>
    ///     请求全部成就数据给界面显示
    /// </summary>
    RequestAllAchievements = 1010,

    /// <summary>
    ///     请求成就进度数据 (特殊)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Achievement Row ID</para>
    /// </remarks>
    /// <seealso cref="AchievementCommand" />
    RequestAchievementSpecial = 1011,

    /// <summary>
    ///     建造房屋
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Ward Index</para>
    /// </remarks>
    BuildHouseOnPlot = 1100,

    /// <summary>
    ///     进入外部装潢设置模式
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Ward Index</para>
    /// </remarks>
    EnterExteriorFixturesState = 1101,

    /// <summary>
    ///     进入内部装潢设置模式
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Ward Index</para>
    /// </remarks>
    EnterInteriorFixturesState = 1102,

    /// <summary>
    ///     拆除房屋
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Ward Index</para>
    /// </remarks>
    RemoveHouseFromPlot = 1103,

    /// <summary>
    ///     重置房屋区域内的数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 固定为 255</para>
    /// </remarks>
    RequestHousingArea = 1104,

    /// <summary>
    ///     请求抽选数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Territory Type</para>
    ///     <para><c>param2</c>: 地皮对应索引</para>
    ///     <code>
    ///     <![CDATA[ wardIndex * 256 + plotIndex]]>
    ///     </code>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    RequestHousingLottery = 1105,

    /// <summary>
    ///     请求门牌数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Territory Type</para>
    ///     <para><c>param2</c>: 地皮对应索引</para>
    ///     <code>
    ///     <![CDATA[ wardIndex * 256 + plotIndex]]>
    ///     </code>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    RequestHousingPlacard = 1106,

    /// <summary>
    ///     请求住宅区数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Territory Type</para>
    ///     <para><c>param2</c>: Ward Index</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    RequestHousingWard = 1107,

    /// <summary>
    ///     请求加载室外装潢背包数据
    /// </summary>
    LoadExteriorAppearanceInventory = 1108,

    /// <summary>
    ///     请求加载室内装潢背包数据
    /// </summary>
    LoadInteriorAppearanceInventory = 1109,

    /// <summary>
    ///     请求加载室外家具背包
    /// </summary>
    LoadExteriorFurnishInventory = 1110,

    /// <summary>
    ///     请求加载室内家具背包
    /// </summary>
    LoadInteriorFurnishInventory = 1111,

    /// <summary>
    ///     向房屋仓库存入指定的物品
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    ///     <para><c>param3</c>: InventoryType</para>
    ///     <para><c>param4</c>: InventorySlot</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    StoreFurniture = 1112,

    /// <summary>
    ///     从房屋中取回指定的家具
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    ///     <para><c>param3</c>: InventoryType (25000 至 25010 / 27000 至 27008)</para>
    ///     <para><c>param4</c>: InventorySlot (若 >65535 则将 slot 为 (i - 65536) 的家具收入仓库)</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    RestoreFurniture = 1113,

    /// <summary>
    ///     请求房屋名称设置数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    RequestHousingName = 1114,

    /// <summary>
    ///     请求房屋问候语设置数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    RequestHousingGreeting = 1115,

    /// <summary>
    ///     未知的房屋命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知 | 未知 &lt;&lt; 8</para>
    /// </remarks>
    HousingCommand1116 = 1116,

    /// <summary>
    ///     请求房屋访客权限设置数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    RequestHousingGuestAccess = 1117,

    /// <summary>
    ///     保存房屋访客权限设置
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    ///     <para><c>param3</c>: 设置枚举值组合 (已知: 1 - 传送权限; 65536 - 进入权限)</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    SetHousingGuestAccess = 1118,

    /// <summary>
    ///     请求房屋宣传设置数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    RequestHousingEstateTag = 1119,

    /// <summary>
    ///     保存房屋宣传设置
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    ///     <para><c>param3</c>: 设置枚举值组合 (注: 即使是相同名称的 Tag 在不同位置上对应的枚举值也不同)</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    SetHousingEstateTag = 1120,

    /// <summary>
    ///     刷新放置的家具数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 0 - 室外; 1 - 室内</para>
    /// </remarks>
    RequestPlacedFurnitures = 1121,

    /// <summary>
    ///     移动到庭院/房屋门前
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 地块索引 (室内的话就不需要传)</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    MoveToHouseFrontGate = 1122,

    /// <summary>
    ///     进入到"布置家具/庭具"状态
    /// </summary>
    /// <remarks>
    ///     <para><c>param2</c>: 房屋地块索引 (公寓为 0)</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    EnterFurnishState = 1123,

    /// <summary>
    ///     未知部队房屋命令
    /// </summary>
    FreeCompanyHousingCommand1124 = 1124,

    /// <summary>
    ///     未知部队房屋个人房间命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Room Index</para>
    /// </remarks>
    FreeCompanyHousingPersonalRoomCommand1125 = 1125,

    /// <summary>
    ///     查看房屋详情
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Territory Type</para>
    ///     <para><c>param2</c>: 地皮对应索引</para>
    ///     <code>
    ///     <![CDATA[ wardIndex * 256 + plotIndex]]>
    ///     </code>
    ///     <para><c>param3</c>: (若有)公寓房间索引</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    ViewHouseDetail = 1126,

    /// <summary>
    ///     未知房屋命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    /// </remarks>
    HousingCommand1127 = 1127,

    /// <summary>
    ///     请求房屋室外相关数据更新
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, 0 或 1</para>
    ///     <para><c>param2</c>: 未知, 0 或 1</para>
    /// </remarks>
    RequestHousingOutdoorTerritory = 1128,

    /// <summary>
    ///     未知游戏管理员命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    /// </remarks>
    GMCommand1129 = 1129,

    /// <summary>
    ///     未知游戏管理员命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    /// </remarks>
    GMCommand1130 = 1130,

    /// <summary>
    ///     未知房屋模特命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: IndoorTerritory 的 HouseID 地址的高 32 位</para>
    ///     <para><c>param2</c>: IndoorTerritory 的 HouseID</para>
    ///     <para><c>param3</c>: 未知</para>
    ///     <para><c>param4</c>: 未知</para>
    /// </remarks>
    MannequinCommand1132 = 1132,

    /// <summary>
    ///     移除部队房屋
    /// </summary>
    RemoveFreeCompanyHouse = 1133,

    /// <summary>
    ///     请求庭院雇员出售列表数据
    /// </summary>
    RequestHousingRetainerList = 1134,

    /// <summary>
    ///     请求房屋共享室友/强制驱离副权限人名单
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 0 - 房屋共享; 1 - 强制驱离副权限人</para>
    /// </remarks>
    RequestHousingShareHolders = 1135,

    /// <summary>
    ///     未知房屋命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 对应区域的 HouseID 的高 32 位</para>
    ///     <para><c>param2</c>: 对应区域的 HouseID</para>
    /// </remarks>
    HousingCommand1136 = 1136,

    /// <summary>
    ///     调整室内环境设置
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 亮度等级 (最亮为 0, 最暗为 5)</para>
    ///     <para><c>param2</c>: 是否关闭环境光遮蔽 (SSAO) (关闭为 1, 开启为 0)</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    SetIndoorEnvironment = 1137,

    /// <summary>
    ///     请求飞空艇数据
    /// </summary>
    RequestAirship = 1138,

    /// <summary>
    ///     未知飞空艇命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 飞空艇索引</para>
    /// </remarks>
    AirshipCommand1139 = 1139,

    /// <summary>
    ///     未知飞空艇命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 飞空艇索引</para>
    /// </remarks>
    AirshipCommand1140 = 1140,

    /// <summary>
    ///     未知飞空艇命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 当前选择飞空艇的某个字段</para>
    ///     <para><c>param2</c>: 未知</para>
    ///     <para><c>param3</c>: Inventory Type</para>
    ///     <para><c>param4</c>: Inventory Slot</para>
    /// </remarks>
    AirshipCommand1141 = 1141,

    /// <summary>
    ///     未知飞空艇命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 当前选择飞空艇的某个字段</para>
    ///     <para><c>param2</c>: 未知</para>
    /// </remarks>
    AirshipCommand1142 = 1142,

    /// <summary>
    ///     刷新部队合建物品交纳信息
    /// </summary>
    RequestCompanyProject = 1143,

    /// <summary>
    ///     请求潜水艇完成情况信息
    /// </summary>
    RequestSubmarine = 1144,

    /// <summary>
    ///     设置房屋背景音乐
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 管弦乐曲在 Orchestrion.csv 中的对应索引</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    SetHouseBackgroundMusic = 1145,

    /// <summary>
    ///     未知房屋命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    ///     <para><c>param3</c>: 未知</para>
    ///     <para><c>param4</c>: 未知</para>
    /// </remarks>
    HousingCommand1146 = 1146,

    /// <summary>
    ///     设置管弦乐琴播放列表
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 播放列表 ID</para>
    /// </remarks>
    SetOrchestrionPlaylist = 1147,

    /// <summary>
    ///     管弦乐琴播放 / 停止切换
    /// </summary>
    ToggleOrchestrion = 1148,

    /// <summary>
    ///     管弦乐琴下一曲 / 音量调整
    /// </summary>
    PlayNextOrchestrionTrack = 1149,

    /// <summary>
    ///     从房屋仓库中取出布置指定物品
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    ///     <para><c>param3</c>: InventoryType (25000 至 25010 / 27000 至 27008)</para>
    ///     <para><c>param4</c>: InventorySlot</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    PlaceFurnish = 1150,

    /// <summary>
    ///     请求房屋仓库状况数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// </remarks>
    RequestHousingStoreroom = 1151,

    /// <summary>
    ///     未知房屋命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    ///     <para><c>param3</c>: Inventory Type</para>
    ///     <para><c>param4</c>: Inventory Slot</para>
    /// </remarks>
    HousingCommand1152 = 1152,

    /// <summary>
    ///     修理潜水艇部件
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 潜水艇索引</para>
    ///     <para><c>param2</c>: 潜水艇部件索引</para>
    /// </remarks>
    /// <seealso cref="SubmarineCommand" />
    RepairSubmarinePart = 1153,

    /// <summary>
    ///     请求访客留言簿数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    ///     <para><c>param3</c>: 页索引</para>
    /// </remarks>
    RequestHousingGuestBook1154 = 1154,

    /// <summary>
    ///     请求访客留言簿数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    ///     <para><c>param3</c>: 页索引</para>
    /// </remarks>
    RequestHousingGuestBook1155 = 1155,

    /// <summary>
    ///     请求访客留言簿数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    ///     <para><c>param3</c>: 页索引</para>
    /// </remarks>
    RequestHousingGuestBook1156 = 1156,

    /// <summary>
    ///     请求访客留言簿数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    ///     <para><c>param3</c>: 页索引</para>
    /// </remarks>
    RequestHousingGuestBook1157 = 1157,

    /// <summary>
    ///     请求访客留言簿数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: HouseManager 相关区域的 HouseID 的高 32 位</para>
    ///     <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    ///     <para><c>param3</c>: 页索引</para>
    /// </remarks>
    RequestHousingGuestBook1158 = 1158,

    /// <summary>
    ///     未知房屋命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    /// </remarks>
    HousingCommand1159 = 1159,

    /// <summary>
    ///     打开售卖设置界面
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, 0 或 2</para>
    /// </remarks>
    OpenHousingRetainerSalesSettingUI = 1160,

    /// <summary>
    ///     未知雇员市场命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Inventory Type</para>
    ///     <para><c>param2</c>: Inventory Slot</para>
    ///     <para><c>param3</c>: 未知</para>
    ///     <para><c>param4</c>: 未知</para>
    /// </remarks>
    RetainerMarketCommand1161 = 1161,

    /// <summary>
    ///     未知房屋命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    ///     <para><c>param3</c>: 未知</para>
    /// </remarks>
    HousingCommand1162 = 1162,

    /// <summary>
    ///     打开购买界面
    /// </summary>
    OpenHousingRetainerBuyUI = 1163,

    /// <summary>
    ///     更新雇员姿势
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    ///     <para><c>param3</c>: 未知</para>
    ///     <para><c>param4</c>: 未知</para>
    /// </remarks>
    UpdateHousingRetainerPose = 1164,

    /// <summary>
    ///     设置雇员武器
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Inventory Type</para>
    ///     <para><c>param2</c>: Inventory Slot</para>
    /// </remarks>
    SetHousingRetainerWeapon = 1165,

    /// <summary>
    ///     切换房屋雇员是否显示主手武器
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 0 - 隐藏; 1 - 显示</para>
    /// </remarks>
    ToggleHousingRetainerWeapon = 1166,

    /// <summary>
    ///     请求房屋数据
    /// </summary>
    RequestHousing = 1167,

    /// <summary>
    ///     请求房屋内部改建信息
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, 固定为 255</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    RequestHousingInteriorDesign = 1168,

    /// <summary>
    ///     更改房屋内部装修风格
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 房屋索引 (从 0 开始, 59 结束)</para>
    ///     <para><c>param1</c>: 内部装修风格 (3 - 海雾村风格; 6 - 薰衣草苗圃风格; 9 - 高脚孤丘风格; 12 - 白银乡风格; 15 - 穹顶皓天风格; 18 - 简装风格; 21 - 深色简装风格)</para>
    /// </remarks>
    /// <seealso cref="HousingCommand" />
    ChangeHousingInteriorDesign = 1169,

    /// <summary>
    ///     未知房屋内部装修风格命令
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    /// </remarks>
    HouseInteriorPatternCommand1170 = 1170,

    /// <summary>
    ///     领取战利水晶
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 赛季 (0 - 本赛季; 1 - 上赛季)</para>
    /// </remarks>
    /// <seealso cref="PVPCommand" />
    CollectTrophyCrystal = 1200,

    /// <summary>
    ///     选择 PVP 职能技能
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 职能技能索引</para>
    /// </remarks>
    /// <seealso cref="PVPCommand" />
    SelectPVPRoleAction = 1201,

    /// <summary>
    ///     请求挑战笔记数据
    /// </summary>
    RequestContentsNote = 1301,

    /// <summary>
    ///     请求雇员探险时间信息
    /// </summary>
    RequestRetainerVentureTime = 1400,

    /// <summary>
    ///     在 NPC 处维修装备
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Inventory Type</para>
    ///     <para><c>param2</c>: Inventory Slot</para>
    ///     <para><c>param3</c>: Item ID</para>
    /// </remarks>
    /// <seealso cref="RepairCommand" />
    RepairItemNPC = 1600,

    /// <summary>
    ///     在 NPC 处批量维修装备
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 分类 (0 - 主手/副手; 1 - 头部/身体/手臂; 2 - 腿部/脚部; 3 - 耳部;颈部; 4 - 腕部;戒指; 5 - 物品)</para>
    /// </remarks>
    /// <seealso cref="RepairCommand" />
    RepairAllItemsNPC = 1601,

    /// <summary>
    ///     在 NPC 处批量维修装备中装备
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Inventory Type (固定为 1000)</para>
    /// </remarks>
    /// <seealso cref="RepairCommand" />
    RepairEquippedItemsNPC = 1602,

    /// <summary>
    ///     切换陆行鸟作战风格
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: BuddyAction 中的对应索引</para>
    /// </remarks>
    /// <seealso cref="BuddyCommand" />
    SetBuddyAction = 1700,

    /// <summary>
    ///     陆行鸟装甲
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 部位 (0 - 头部, 1 - 身体, 2 - 腿部)</para>
    ///     <para><c>param2</c>: 在 BuddyEquip 中对应的装备索引 (0 - 卸下装备)</para>
    /// </remarks>
    /// <seealso cref="BuddyCommand" />
    SetBuddyEquip = 1701,

    /// <summary>
    ///     陆行鸟学习技能
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Skill 索引</para>
    /// </remarks>
    /// <seealso cref="BuddyCommand" />
    LearnBuddySkill = 1702,

    /// <summary>
    ///     请求金碟游乐场面板 整体 信息
    /// </summary>
    RequestGoldSaucerGeneral = 1850,

    /// <summary>
    ///     请求金碟游乐场面板 陆行鸟 信息
    /// </summary>
    RequestGoldSaucerChocobo = 1900,

    /// <summary>
    ///     开始任务回顾
    /// </summary>
    StartDutyRecord = 1980,

    /// <summary>
    ///     结束任务回顾
    /// </summary>
    FinishDutyRecord = 1981,

    /// <summary>
    ///     请求金碟游乐场面板 萌宠之王 信息
    /// </summary>
    RequestGoldSaucerVerminion = 2010,

    /// <summary>
    ///     萌宠之王小宠物编队确认
    /// </summary>
    ConfirmVerminionPalette = 2011,

    /// <summary>
    ///     解除新人状态
    /// </summary>
    DissmissNoviceState = 2100,

    /// <summary>
    ///     成为新人状态
    /// </summary>
    SetNoviceState = 2101,

    /// <summary>
    ///     指导者启用/解除自动加入新人频道设置
    /// </summary>
    SetAutoJoinNoviceNetworkMentor = 2102,

    /// <summary>
    ///     是否接受新人频道邀请
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 0 - 接受; 1 - 拒绝</para>
    /// </remarks>
    AcceptNoviceNetworkInvitation = 2103,

    /// <summary>
    ///     解除回归者状态
    /// </summary>
    DismissReturnerState = 2104,

    /// <summary>
    ///     刷新新人频道状态 (在解除新人状态时会被调用)
    /// </summary>
    RefreshNoviceNetwork = 2106,

    /// <summary>
    ///     认领回归者时是否一并加入新人频道
    /// </summary>
    JoinNoviceNetworkReturner = 2107,

    /// <summary>
    ///     发起决斗
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 被决斗对象的 GameObject ID</para>
    /// </remarks>
    /// <seealso cref="PVPCommand" />
    SendDuel = 2200,

    /// <summary>
    ///     确认决斗申请
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 0 - 确认; 1 - 取消; 4 - 强制取消</para>
    /// </remarks>
    /// <seealso cref="PVPCommand" />
    RequestDuel = 2201,

    /// <summary>
    ///     同意决斗
    /// </summary>
    ConfirmDuel = 2202,

    /// <summary>
    ///     未知复活命令
    /// </summary>
    ReviveCommand2204 = 2204,

    /// <summary>
    ///     确认天书奇谈副本结果
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 索引 (从左到右从上到下, 从 0 开始)</para>
    /// </remarks>
    /// <seealso cref="WondrousTailsCommand" />
    ConfirmWondrousTailsSlot = 2253,

    /// <summary>
    ///     天书奇谈其他操作
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 操作 (0 - 再想想)</para>
    ///     <para><c>param2</c>: 索引 (从左到右从上到下, 从 0 开始)</para>
    /// </remarks>
    /// <seealso cref="WondrousTailsCommand" />
    WondrousTails = 2254,

    /// <summary>
    ///     请求 ENPC 数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: ENPC Data ID</para>
    /// </remarks>
    RequestENPC = 2300,

    /// <summary>
    ///     请求投影台数据
    /// </summary>
    RequestPrismBox = 2350,

    /// <summary>
    ///     取出投影台物品
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 投影台内部物品 ID (MirageManager.Instance().PrismBoxItemIds)</para>
    /// </remarks>
    /// <seealso cref="PrsimBoxCommand" />
    RestorePrsimBoxItem = 2352,

    /// <summary>
    ///     将投影台中的套装物品还原
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 投影台中索引</para>
    ///     <para><c>param2</c>: 物品位掩码低 32 位</para>
    ///     <para><c>param3</c>: 物品位掩码高 32 位</para>
    ///     <para>从第 1 位开始: 4, 8, 16, 32, 64, 128, 256...</para>
    /// </remarks>
    RestorePrsimBoxSetItem = 2353,

    /// <summary>
    ///     请求投影模板数据
    /// </summary>
    RequestGlamourPlate = 2355,

    /// <summary>
    ///     进入/退出投影模板选择状态
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 0 - 退出, 1 - 进入</para>
    ///     <para><c>param2</c>: 未知, 可能为 0 或 1</para>
    /// </remarks>
    /// <seealso cref="GlamourPlateCommand" />
    ToggleGlamourPlateState = 2356,

    /// <summary>
    ///     应用投影模板 (需要先进入投影模板选择状态)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 投影模板索引</para>
    /// </remarks>
    /// <seealso cref="GlamourPlateCommand" />
    ApplyGlamourPlate = 2357,

    /// <summary>
    ///     从投影台应用幻化模板
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 模板索引</para>
    /// </remarks>
    ApplyGlamourPlateFromPrismBox = 2358,

    /// <summary>
    ///     为装备解除投影
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: AgentMiragePrismMiragePlateData.DispellItemsSelectedBitmask</para>
    /// </remarks>
    DispellGlamours = 2359,

    /// <summary>
    ///     获取时尚品鉴每周参与奖励
    /// </summary>
    CliamFashionCheckEntryReward = 2450,

    /// <summary>
    ///     获取时尚品鉴每周额外奖励
    /// </summary>
    ClaimFashionCheckBonusReward = 2451,

    /// <summary>
    ///     时尚品鉴新增装备条目与额外奖励
    /// </summary>
    ClaimFashionCheckNewGearReward = 2452,

    /// <summary>
    ///     未知时尚品鉴指令
    /// </summary>
    FashionCheckCommand2453 = 2453,

    /// <summary>
    ///     请求重建多玛相关数据
    /// </summary>
    RequestEnclave = 2500,

    /// <summary>
    ///     买回多玛飞地支援物资
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 物品索引</para>
    /// </remarks>
    /// <seealso cref="DomanEnclaveCommand" />
    BuybackEnclaveItem = 2501,

    /// <summary>
    ///     请求金碟游乐场面板 多玛方城战 信息
    /// </summary>
    RequestGoldSaucerMahjong = 2550,

    /// <summary>
    ///     请求青魔法师每周挑战信息
    /// </summary>
    RequestBlueContentBriefing = 2600,

    /// <summary>
    ///     请求青魔法书数据
    /// </summary>
    RequstBlueNotebook = 2601,

    /// <summary>
    ///     请求亲信战友数据
    /// </summary>
    RequestTrustedFriend = 2651,

    /// <summary>
    ///     请求剧情辅助器数据
    /// </summary>
    RequestDutySupport = 2653,

    /// <summary>
    ///     发送剧情辅助器申请请求
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: DawnStroy 序号</para>
    ///     <para><c>param2</c>: 前四位 DawnStroyMemberUIParam 序号的幂次方 (a1 * 256^0 + a2 * 256^1 + a3 * 256^2 + a4 * 256^3)</para>
    ///     <para><c>param2</c>: 后三位 DawnStroyMemberUIParam 序号的幂次方 (a1 * 256^0 + a2 * 256^1 + a3 * 256^2)</para>
    /// </remarks>
    /// <seealso cref="SendDutySupportCommand" />
    SendDutySupport = 2654,

    /// <summary>
    ///     EventFramework 动作
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>：事件ID - 分解：3735552；回收魔晶石：3735553；修理：3735555</para>
    ///     <br />
    ///     <para>修理（3735555)</para>
    ///     <para><c>param2</c>：类型 - 修理装备中物品：2；修理分页物品：3；修理单独物品：物品（InventorySlot &lt;&lt; 16）| 1</para>
    ///     <para><c>param3</c>：修理装备中物品 - 固定为 InventoryType 1000；修理分页物品 - 分页索引；修理单独物品 - Inventory Type</para>
    ///     <para><c>param4</c>：修理单独物品 - 物品ID, HQ时增加 100_0000</para>
    ///     <br />
    ///     <para>回收魔晶石（3735553)</para>
    ///     <para><c>param2</c>：Inventory Type</para>
    ///     <para><c>param3</c>：Inventory Slot</para>
    ///     <para><c>param4</c>：物品ID, HQ时增加 100_0000</para>
    ///     <br />
    ///     <para>分解（3735552)</para>
    ///     <para><c>param2</c>：Inventory Type</para>
    ///     <para><c>param3</c>：Inventory Slot</para>
    ///     <para><c>param4</c>：物品ID, HQ时增加 100_0000</para>
    /// </remarks>
    EventFrameworkAction = 2800,

    /// <summary>
    ///     请求博兹雅战果记录数据更新
    /// </summary>
    RequestBozjaWarResultNotebook = 2900,

    /// <summary>
    ///     博兹雅分配失传技能库到技能槽
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 失传技能库索引</para>
    ///     <para><c>param2</c>: 要分配的槽位</para>
    /// </remarks>
    /// <seealso cref="BozjaCommand" />
    AssignBozjaActionFromHolster = 2950,

    /// <summary>
    ///     在博兹雅/高原副本区域以外地区查看失传技能库
    /// </summary>
    RequestBozjaHolsterOutside = 2951,

    /// <summary>
    ///     场景跳转 (Lua 触发)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    ///     <para><c>param3</c>: 未知</para>
    ///     <para><c>param4</c>: 未知</para>
    /// </remarks>
    PrepareSceneJump = 3000,

    /// <summary>
    ///     场景跳转 (Lua 触发)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知 (可能是 X 坐标)</para>
    ///     <para><c>param2</c>: 未知 (可能是 Y 坐标)</para>
    ///     <para><c>param3</c>: 未知 (可能是 Z 坐标)</para>
    ///     <para><c>param4</c>: 未知 (可能是面向)</para>
    /// </remarks>
    StartSceneJumpLua = 3001,

    /// <summary>
    ///     无人岛捕获动物
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 动物 BaseID</para>
    ///     <para><c>param2</c>: 动物 EntityID</para>
    ///     <para><c>param3</c>: MJIManager.CurrentMode</para>
    ///     <para><c>param4</c>: MJIManager.CurrentModeItem</para>
    /// </remarks>
    CaptureMJIAnimal = 3050,

    /// <summary>
    ///     请求部分物品的解锁状态
    /// </summary>
    RequestItemActionUnlockState = 3100,

    /// <summary>
    ///     请求服务器的特定值
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: ID</para>
    /// </remarks>
    GetServerValue = 3150,

    /// <summary>
    ///     请求肖像列表数据
    /// </summary>
    RequestPortrait = 3200,

    /// <summary>
    ///     请求铭牌数据
    /// </summary>
    RequestCharacterCard = 3201,

    /// <summary>
    ///     切换无人岛模式
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 模式 (0 - 自由; 1 - 收获; 2 - 播种; 3 - 浇水; 4 - 铲除; 6 - 喂食; 7 - 宠爱; 8 - 招呼; 9 - 捕兽)</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    SetMJIMode = 3250,

    /// <summary>
    ///     设置无人岛模式参数, 切换时会被设置为 0, 如播种、喂食、捕兽时会为对应的物品 ID
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 参数</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    SetMJIModeParam = 3251,

    /// <summary>
    ///     无人岛设置面板开关
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 状态 (1 - 开启; 0 - 关闭)</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    ToggleMJISettingPanel = 3252,

    /// <summary>
    ///     请求无人岛工房排班数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 具体天数 (0 为本周期第一天, 7 为下周期第一天)</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    RequestMJIWorkshop = 3254,

    /// <summary>
    ///     请求预计所需要消耗的无人岛材料
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知, 可能为 0, 1, 2</para>
    /// </remarks>
    RequestMJIWorkshopConsumption = 3255,

    /// <summary>
    ///     请求无人岛工房排班物品数据
    /// </summary>
    RequestMJIWorkshopAssignment = 3258,

    /// <summary>
    ///     无人岛工房排班
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 物品和排班时间段: (8 * (startingHour | (32 * craftObjectId)))</para>
    ///     <para><c>param2</c>: 具体天数 (0 - 本周期第一天, 7 - 下周期第一天)</para>
    ///     <para><c>param4</c>: 添加/删除 (0 - 添加, 1 - 删除)</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    AssignMJIWorkshop = 3259,

    /// <summary>
    ///     取消无人岛工坊排班
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 物品和排班时间段: (8 * (startingHour | (32 * craftObjectId)))</para>
    ///     <para><c>param2</c>: 具体天数 (0 - 本周期第一天, 7 - 下周期第一天)</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    CancelMJIWorkshopAssignment = 3260,

    /// <summary>
    ///     设置无人岛休息周期
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 休息日 1</para>
    ///     <para><c>param2</c>: 休息日 2</para>
    ///     <para><c>param3</c>: 休息日 3</para>
    ///     <para><c>param4</c>: 休息日 4</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    SetMJIWorkshopRest = 3261,

    /// <summary>
    ///     收取无人岛屯货仓库探索结果
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 仓库索引</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    CollectMJIGranary = 3262,

    /// <summary>
    ///     查看无人岛屯货仓库探索目的地
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 仓库索引</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    ViewMJIGranaryDestination = 3263,

    /// <summary>
    ///     无人岛屯货仓库派遣探险
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 仓库索引</para>
    ///     <para><c>param2</c>: 目的地索引</para>
    ///     <para><c>param3</c>: 探索天数</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    AssignMJIGranary = 3264,

    /// <summary>
    ///     在无人岛放养宠物
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 宠物 ID</para>
    ///     <para><c>param2</c>: 放生区域索引</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    ReleaseMJIMinion = 3265,

    /// <summary>
    ///     放生无人岛牧场动物
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 动物索引</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    ReleaseMJIAnimal = 3268,

    /// <summary>
    ///     收集无人岛牧场动物产物
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 动物索引</para>
    ///     <para><c>param2</c>: 收集标志</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    CollectMJIAnimalLeaving = 3269,

    /// <summary>
    ///     收取无人岛牧场全部动物产物
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 预期收集的产物数量 (MJIManager.Instance()->PastureHandler->AvailableMammetLeavings)</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    CollectMJIAllAnimalLeaving = 3271,

    /// <summary>
    ///     托管无人岛牧场动物
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 动物索引</para>
    ///     <para><c>param2</c>: 喂食物品 ID</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    EntrustMJIAnimal = 3272,

    /// <summary>
    ///     召回无人岛放生的宠物
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 宠物索引</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    RecallMJIMinion = 3277,

    /// <summary>
    ///     托管单块无人岛耕地
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 耕地索引</para>
    ///     <para><c>param2</c>: 种子物品 ID</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    EntrustMJIFarm = 3279,

    /// <summary>
    ///     取消托管单块无人岛耕地
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 耕地索引</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    DismissMJIFarmEntrust = 3280,

    /// <summary>
    ///     收取单块无人岛耕地
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 耕地索引</para>
    ///     <para><c>param2</c>: 收取后是否取消托管 (0 - 否, 1 - 是)</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    CollectMJIFarm = 3281,

    /// <summary>
    ///     收取全部无人岛耕地
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: *(int*)MJIManager.Instance()->GranariesState</para>
    /// </remarks>
    /// <seealso cref="MJICommand" />
    CollectMJIAllFarm = 3282,

    /// <summary>
    ///     旅馆内播放管弦乐琴乐谱
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Orchestrion ID</para>
    /// </remarks>
    PlayOrchestrionTrack = 3283,

    /// <summary>
    ///     请求无人岛工房需求数据
    /// </summary>
    RequestMJIWorkshopFavor = 3292,

    /// <summary>
    ///     移除收藏夹内的以太之光
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 以太之光 ID</para>
    /// </remarks>
    RemoveFavoriteAetheryte = 3350,

    /// <summary>
    ///     移除免费传送点
    /// </summary>
    RemoveFreeAetheryte = 3351,

    /// <summary>
    ///     移除 PlayStation Plus 会员可设置的免费传送点
    /// </summary>
    RemoveFreeAetherytePSPlus = 3352,

    /// <summary>
    ///     移除 Nintendo Switch Online 会员可设置的免费传送点
    /// </summary>
    RemoveFreeAetheryteNSO = 3353,

    /// <summary>
    ///     变更宇宙探索模式
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 模式索引</para>
    /// </remarks>
    /// <seealso cref="WKSCommand" />
    SetWKSMode = 3400,

    /// <summary>
    ///     宇宙探索结束交互
    /// </summary>
    FinishWKSInteraction3401 = 3401,

    /// <summary>
    ///     宇宙探索结束交互
    /// </summary>
    FinishWKSInteraction3402 = 3402,

    /// <summary>
    ///     未知的宇宙探索命令 (大概和星球建设进度推进至新阶段有关)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: WKSDevGrade 的 Unknown13</para>
    /// </remarks>
    WKSDevelopmentCommand = 3403,

    /// <summary>
    ///     宇宙探索接取任务
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: Mission Unit ID</para>
    /// </remarks>
    /// <seealso cref="WKSCommand" />
    AcceptWKSMission = 3440,

    /// <summary>
    ///     宇宙探索完成任务
    /// </summary>
    FinishWKSMission = 3441,

    /// <summary>
    ///     宇宙探索放弃任务
    /// </summary>
    AbandonWKSMission = 3442,

    /// <summary>
    ///     宇宙好运道开始抽奖
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 类型: 0 - 月球信用点; 1 - 法恩娜信用点; 2 - 俄匊斯信用点; 3 - 奥克塞西亚信用点</para>
    /// </remarks>
    /// <seealso cref="WKSCommand" />
    StartWKSLottery = 3450,

    /// <summary>
    ///     宇宙好运道选择转盘
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 类型: 0 - 月球信用点; 1 - 法恩娜信用点; 2 - 俄匊斯信用点; 3 - 奥克塞西亚信用点</para>
    ///     <para><c>param2</c>: 转盘类型 (左边 - 0, 右边 - 1)</para>
    /// </remarks>
    /// <seealso cref="WKSCommand" />
    ChooseWKSLotteryType = 3451,

    /// <summary>
    ///     宇宙好运道结束抽奖
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 类型: 0 - 月球信用点; 1 - 法恩娜信用点; 2 - 俄匊斯信用点; 3 - 奥克塞西亚信用点</para>
    /// </remarks>
    /// <seealso cref="WKSCommand" />
    FinishWKSLottery = 3452,

    /// <summary>
    ///     宇宙探索请求探索成果数据
    /// </summary>
    RequestWKSSuccesses = 3460,

    /// <summary>
    ///     宇宙探索请求机甲数据
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: WKSMechaEventData Row ID (0 - 当前未开始)</para>
    /// </remarks>
    /// <seealso cref="WKSCommand" />
    RequestWKSMecha = 3478,

    /// <summary>
    ///     请求副本物品栏 (在区域变化时自动发送)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: ContentInventoryProvider 偏移 8 处字段</para>
    /// </remarks>
    RequestContentInventory = 3500,

    /// <summary>
    ///     请求大型副本数据 (看起来会在某个内部字段过期后自动发送一次请求新的)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 未知</para>
    ///     <para><c>param2</c>: 未知</para>
    /// </remarks>
    RequestMassivePCContent = 3600,

    /// <summary>
    ///     未知的任务命令 (是不是任务相关都不好说)
    /// </summary>
    QuestCommand4000 = 4000,

    /// <summary>
    ///     掷骰子
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 类型 (固定为 0)</para>
    ///     <para><c>param2</c>: 最大值</para>
    /// </remarks>
    /// <seealso cref="DiceCommand" />
    RollDice = 9000,

    /// <summary>
    ///     请求妖怪手表联动活动信息 (每 5 秒发送一次)
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 固定为 39</para>
    /// </remarks>
    RequestYokaiWatchState = 9002,

    /// <summary>
    ///     雇员
    /// </summary>
    /// <remarks>
    ///     <para><c>param1</c>: 固定为 0</para>
    ///     <para><c>param2</c>: 3 - 返回雇员; 4 - 刷新信息; 5 - 委托探险; 6 - 撤销探险</para>
    ///     <para><c>param3</c>: 委托探险时 - RetainerTask ID</para>
    /// </remarks>
    Retainer = 9003,

    /// <summary>
    ///     设置角色显示范围
    /// </summary>
    /// ///
    /// <remarks>
    ///     <para><c>param1</c>: 类型 (0 - 标准; 1 - 较大; 2 - 最大)</para>
    /// </remarks>
    /// <seealso cref="AroundRangeCommand" />
    SetAroundRangeMode = 9005
}
