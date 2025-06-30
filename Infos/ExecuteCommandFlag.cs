namespace OmenTools.Infos;

public enum ExecuteCommandFlag
{
    /// <summary>
    /// 拔出/收回武器
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 1 - 拔出, 0 - 收回</para>
    /// <para><c>param2</c>: 未知, 固定为 1</para>
    /// </remarks>
    DrawOrSheatheWeapon = 1,

    /// <summary>
    /// 自动攻击
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 是否开启自动攻击 (0 - 否, 1 - 是)</para>
    /// <para><c>param2</c>: 目标对象ID</para>
    /// <para><c>param3</c>: 是否为手动操作 (0 - 否, 1 - 是)</para>
    /// </remarks>
    AutoAttack = 2,

    /// <summary>
    /// 选中目标
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 目标 Entity ID (无目标为: 0xE0000000)</para>
    /// </remarks>
    Target = 3,
    
    /// <summary>
    /// PVP 快捷发言
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: QuickChat Row ID</para>
    /// <para><c>param2</c>: 参数 1</para>
    /// <para><c>param3</c>: 参数 2</para>
    /// </remarks>
    PVPQuickChat = 5,

    /// <summary>
    /// 下坐骑
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 0 - 不进入队列; 1 - 进入队列</para>
    /// </remarks>
    Dismount = 101,

    /// <summary>
    /// 召唤宠物
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 宠物 ID</para>
    /// </remarks>
    SummonPet = 102,

    /// <summary>
    /// 收回宠物
    /// </summary>
    WithdrawPet = 103,
    
    /// <summary>
    /// 取消身上指定的状态效果
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Status ID</para>
    /// <para><c>param3</c>: Status 来源的 GameObjectID，亦可用 0xE0000000 指定清除任意来源的首个该状态 </para>
    /// </remarks>
    StatusOff = 104,

    /// <summary>
    /// 中断咏唱
    /// </summary>
    CancelCast = 105,

    /// <summary>
    /// 共同骑乘
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 目标 ID</para>
    /// <para><c>param2</c>: 位置索引</para>
    /// </remarks>
    RidePillion = 106,

    /// <summary>
    /// 收起时尚配饰
    /// </summary>
    WithdrawParasol109 = 109,

    /// <summary>
    /// 收起时尚配饰
    /// </summary>
    WithdrawParasol110 = 110,

    /// <summary>
    /// 复活
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 操作 (5 - 接受复活; 8 - 确认返回返回点)</para>
    /// </remarks>
    Revive = 200,

    /// <summary>
    /// 区域变更
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 变更方式</para>
    /// <list type="table">
    ///     <item>
    ///         <term>1</term>
    ///         <description>NPC 传送</description>
    ///     </item>
    ///     <item>
    ///         <term>3</term>
    ///         <description>边界过图</description>
    ///     </item>
    ///     <item>
    ///         <term>4</term>
    ///         <description>正常传送</description>
    ///     </item>
    ///     <item>
    ///         <term>7</term>
    ///         <description>返回</description>
    ///     </item>
    ///     <item>
    ///         <term>15</term>
    ///         <description>城内以太之晶</description>
    ///     </item>
    ///     <item>
    ///         <term>20</term>
    ///         <description>房区</description>
    ///     </item>
    /// </list>
    /// <para><c>param2</c>: 区域内位置变更方式</para>
    /// <list type="table">
    ///     <item>
    ///         <term>1</term>
    ///         <description>剧情转移</description>
    ///     </item>
    ///     <item>
    ///         <term>2</term>
    ///         <description>返回到安全区</description>
    ///     </item>
    ///     <item>
    ///         <term>25</term>
    ///         <description>副本内过图</description>
    ///     </item>
    ///     <item>
    ///         <term>26</term>
    ///         <description>潜水</description>
    ///     </item>
    /// </list>
    /// </remarks>
    TerritoryTransport = 201,

    /// <summary>
    /// 传送至指定的以太之光
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 以太之光 ID</para>
    /// <para><c>param2</c>: 是否使用传送券 (0 - 否, 1 - 是)</para>
    /// <para><c>param3</c>: 以太之光 Sub ID</para>
    /// </remarks>
    Teleport = 202,

    /// <summary>
    /// 接受传送邀请
    /// </summary>
    AcceptTeleportOffer = 203,

    /// <summary>
    /// 请求好友房屋传送信息
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 未知</para>
    /// <para><c>param2</c>: 未知</para>
    /// </remarks>
    RequestFriendHouseTeleport = 210,

    /// <summary>
    /// 传送至好友房屋
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 未知</para>
    /// <para><c>param2</c>: 未知</para>
    /// <para><c>param3</c>: 以太之光 ID (个人房屋 - 61, 部队房屋 - 57)</para>
    /// <para><c>param4</c>: 以太之光 Sub ID (疑似恒定为 1)</para>
    /// </remarks>
    TeleportToFriendHouse = 211,

    /// <summary>
    /// 若当前种族不是拉拉菲尔族, 则返回至当前地图的最近安全点
    /// </summary>
    ReturnIfNotLalafell = 213,

    /// <summary>
    /// 立即返回至返回点, 若在副本内则返回至副本内重生点
    /// </summary>
    InstantReturn = 214,

    /// <summary>
    /// 检查指定玩家
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 待对象 Object ID</para>
    /// </remarks>
    Inspect = 300,

    /// <summary>
    /// 更改佩戴的称号
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 称号 ID</para>
    /// </remarks>
    ChangeTitle = 302,

    /// <summary>
    /// 请求过场剧情数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 过场剧情在 Cutscene.csv 中的对应索引</para>
    /// </remarks>
    RequestCutscene307 = 307,

    /// <summary>
    /// 请求挑战笔记具体类别下数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 类别索引 (从 1 开始)</para>
    /// </remarks>
    RequestContentsNoteCategory = 310,

    /// <summary>
    /// 清除场地标点
    /// </summary>
    ClearFieldMarkers = 313,

    /// <summary>
    /// 将青魔法师技能交换或应用于有效技能
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 类型 (0 为应用有效技能, 1 为交换有效技能)</para>
    /// <para><c>param2</c>: 格子序号 (从 0 开始, 小于 24)</para>
    /// <para><c>param3</c>: 技能 ID / 格子序号 (从 0 开始, 小于 24)</para>
    /// </remarks>
    AssignBLUActionToSlot = 315,

    /// <summary>
    /// 请求跨界传送数据
    /// </summary>
    RequestWorldTravel = 316,

    /// <summary>
    /// 放置场地标点
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 标点索引</para>
    /// <para><c>param2</c>: 坐标 X * 1000</para>
    /// <para><c>param3</c>: 坐标 Y * 1000</para>
    /// <para><c>param4</c>: 坐标 Z * 1000</para>
    /// </remarks>
    PlaceFieldMarker = 317,

    /// <summary>
    /// 移除场地标点
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 标点索引</para>
    /// </remarks>
    RemoveFieldMarker = 318,

    /// <summary>
    /// 清除来自木人的仇恨
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 木人的 Object ID</para>
    /// </remarks>
    ResetStrikingDummy = 319,
    
    /// <summary>
    /// 设置当前雇员市场出售物品价格
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 物品 Slot</para>
    /// <para><c>param2</c>: 物品价格</para>
    /// </remarks>
    SetRetainerMarketPrice = 400,

    /// <summary>
    /// 请求指定物品栏数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: (int)InventoryType</para>
    /// </remarks>
    RequestInventory = 405,

    /// <summary>
    /// 进入镶嵌魔晶石状态
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 物品 ID</para>
    /// </remarks>
    EnterMateriaAttachState = 408,

    /// <summary>
    /// 退出镶嵌魔晶石状态
    /// </summary>
    LeaveMateriaAttachState = 410,

    /// <summary>
    /// 取消魔晶石镶嵌委托
    /// </summary>
    CancelMateriaMeldRequest = 419,

    /// <summary>
    /// 请求收藏柜的数据
    /// </summary>
    RequestCabinet = 424,

    /// <summary>
    /// 存入物品至收藏柜
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 物品在 Cabinet.csv 中的对应索引</para>
    /// </remarks>
    StoreToCabinet = 425,

    /// <summary>
    /// 从收藏柜中取回物品
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 物品在 Cabinet.csv 中的对应索引</para>
    /// </remarks>
    RestoreFromCabinet = 426,

    /// <summary>
    /// 维修装备
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Inventory Type</para>
    /// <para><c>param2</c>: Inventory Slot</para>
    /// <para><c>param3</c>: Item ID</para>
    /// </remarks>
    RepairItem = 434,

    /// <summary>
    /// 批量维修装备中装备
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Inventory Type (固定为 1000)</para>
    /// </remarks>
    RepairEquippedItems = 435,
    
    /// <summary>
    /// 批量维修装备
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 分类 (0 - 主手/副手; 1 - 头部/身体/手臂; 2 - 腿部/脚部; 3 - 耳部;颈部; 4 - 腕部;戒指; 5 - 物品)</para>
    /// </remarks>
    RepairAllItems = 436,

    /// <summary>
    /// 精制魔晶石
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Inventory Type</para>
    /// <para><c>param1</c>: Inventory Slot</para>
    /// </remarks>
    ExtractMateria = 437,

    /// <summary>
    /// 更换套装
    /// </summary>
    GearsetChange = 441,

    /// <summary>
    /// 请求陆行鸟鞍囊的数据
    /// </summary>
    RequestSaddleBag = 444,
    
    /// <summary>
    /// 发送修理委托
    /// </summary>
    /// /// <remarks>
    /// <para><c>param1</c>: 目标 Entity ID</para>
    /// </remarks>
    SendRepairRequest = 450,
    
    /// <summary>
    /// 取消修理委托
    /// </summary>
    /// /// <remarks>
    /// <para><c>param1</c>: 目标 Entity ID</para>
    /// </remarks>
    CancelRepairRequest = 453,

    /// <summary>
    /// 打断当前正在进行的情感动作
    /// </summary>
    InterruptEmote = 502,
    
    /// <summary>
    /// 打断当前正在进行的特殊情感动作
    /// </summary>
    InterruptEmoteSpecial = 503,
    
    /// <summary>
    /// 更改闲置状态姿势
    /// </summary>
    /// <remarks>
    /// <para><c>param2</c>: 姿势索引</para>
    /// </remarks>
    IdlePostureChange = 505,

    /// <summary>
    /// 进入闲置状态姿势
    /// </summary>
    /// <remarks>
    /// <para><c>param2</c>: 姿势索引</para>
    /// </remarks>
    IdlePostureEnter = 506,

    /// <summary>
    /// 退出闲置状态姿势
    /// </summary>
    IdlePostureExit = 507,

    /// <summary>
    /// 进入游泳状态 (也会强制下坐骑)
    /// </summary>
    EnterSwim = 608,

    /// <summary>
    /// 退出游泳状态
    /// </summary>
    LeaveSwim = 609,

    /// <summary>
    /// 赋予/取消禁止骑乘坐骑状态
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 0 - 取消; 1 - 赋予</para>
    /// </remarks>
    DisableMounting = 612,

    /// <summary>
    /// 进入飞行状态
    /// </summary>
    EnterFlight = 616,

    /// <summary>
    /// 生产
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 类型 (0 - 普通制作, 1 - 简易制作; 2 - 制作练习)</para>
    /// <para><c>param2</c>: 配方 ID (在 Recipe.csv 中)</para>
    /// <para><c>param3</c>: 额外参数 (简易制作 - 数量, 最多 255 个)</para>
    /// </remarks>
    Craft = 700,

    /// <summary>
    /// 钓鱼
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 动作 (0 - 抛竿, 1 - 收杆, 2 - 提钩, 4 - 换饵, 5 - 坐下, 10 - 强力提杆, 11 - 精准提钩, 13 - 耐心, 14 - 耐心2, 24 - 熟练妙招, 25 - 游动饵)
    /// </para>
    /// <para><c>param2</c>: 额外参数 (若为换饵, 则为物品 ID; 若为游动饵, 则为饵索引)</para>
    /// </remarks>
    Fish = 701,

    /// <summary>
    /// 加载制作笔记数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 职业索引 (从左至右, 从 0 开始, 至 7 结束)</para>
    /// </remarks>
    LoadCraftLog = 710,

    /// <summary>
    /// 结束制作
    /// </summary>
    ExitCraft = 711,

    /// <summary>
    /// 放弃任务
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 任务 ID (非 RowID)</para>
    /// </remarks>
    AbandonQuest = 800,

    /// <summary>
    /// 刷新理符任务状态
    /// </summary>
    RefreshLeveQuest = 801,
    
    /// <summary>
    /// 放弃理符任务
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 理符任务 ID</para>
    /// </remarks>
    AbandonLeveQuest = 802,

    /// <summary>
    /// 开始理符任务
    /// <remarks>
    /// <para><c>param1</c>: 理符任务 ID</para>
    /// <para><c>param2</c>: 要提高的等级数</para>
    /// </remarks>
    /// </summary>
    StartLeveQuest = 804,
    
    /// <summary>
    /// 副本相关
    /// </summary>
    Content = 808,

    /// <summary>
    /// 开始指定的临危受命任务
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: FATE ID</para>
    /// <para><c>param2</c>: 目标 Object ID</para>
    /// </remarks>
    FateStart = 809,

    /// <summary>
    /// 加载临危受命信息
    /// (在切换地图时会一次性加载完地图内所有 FATE 信息)
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: FATE ID</para>
    /// </remarks>
    FateLoad = 810,

    /// <summary>
    /// 进入 临危受命 范围 (若 FATE 在脚底下生成则不会发送该命令)
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: FATE ID</para>
    /// </remarks>
    FateEnter = 812,

    /// <summary>
    /// 为 临危受命 等级同步
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: FATE ID</para>
    /// <para><c>param2</c>: 是否等级同步 (0 - 否, 1 - 是)</para>
    /// </remarks>
    FateLevelSync = 813,

    /// <summary>
    /// 临危受命 野怪生成
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Object ID</para>
    /// </remarks>
    FateMobSpawn = 814,
    
    /// <summary>
    /// 区域变更完成
    /// </summary>
    TerritoryTransportFinish = 816,

    /// <summary>
    /// 离开副本
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 类型 (0 - 正常退本, 1 - 一段时间未操作)</para>
    /// </remarks>
    LeaveDuty = 819,
    
    /// <summary>
    /// 发送单人任务战斗请求
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 难度 (0 - 通常, 1 - 简单, 2 - 非常简单)</para>
    /// </remarks>
    StartSoloQuestBattle = 823,

    /// <summary>
    /// 昔日重现模式
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: QuestRedo.csv 中对应的昔日重现章节序号 (0 - 退出昔日重现)</para>
    /// </remarks>
    QuestRedo = 824,

    /// <summary>
    /// 刷新物品栏
    /// </summary>
    InventoryRefresh = 830,

    /// <summary>
    /// 请求过场剧情数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 过场剧情在 Cutscene.csv 中的对应索引</para>
    /// </remarks>
    RequestCutscene831 = 831,

    /// <summary>
    /// 请求成就进度数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 成就在 Achievement.csv 中的对应索引</para>
    /// </remarks>
    RequestAchievement = 1000,

    /// <summary>
    /// 请求所有成就概览 (不含具体成就内容)
    /// </summary>
    RequestAllAchievement = 1001,

    /// <summary>
    /// 请求接近达成成就概览 (不含具体成就内容)
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 未知, 固定为 1</para>
    /// </remarks>
    RequestNearCompletionAchievement = 1002,

    /// <summary>
    /// 请求抽选数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Territory Type</para>
    /// <para><c>param2</c>: 地皮对应索引</para>
    /// <code>
    /// <![CDATA[
    /// const int HousesPerArea = 60;
    /// const int AreaOffset = 256;
    /// 
    /// // 第 1 区 第 41 号
    /// var wardID = 0;
    /// var districtOffset = wardID * AreaOffset;
    /// var houseID = 40;
    /// var position = districtOffset + houseID]]>
    /// </code>
    /// </remarks>
    RequestLotteryData = 1105,

    /// <summary>
    /// 请求门牌数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Territory Type</para>
    /// <para><c>param2</c>: 地皮对应索引</para>
    /// <code>
    /// <![CDATA[
    /// const int HousesPerArea = 60;
    /// const int AreaOffset = 256;
    /// 
    /// // 第 1 区 第 41 号
    /// var wardID = 0;
    /// var districtOffset = wardID * AreaOffset;
    /// var houseID = 40;
    /// var position = districtOffset + houseID]]>
    /// </code>
    /// </remarks>
    RequestPlacardData = 1106,

    /// <summary>
    /// 请求住宅区数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Territory Type</para>
    /// <para><c>param2</c>: 分区索引</para>
    /// </remarks>
    RequestHousingAreaData = 1107,

    /// <summary>
    /// 向房屋仓库存入指定的物品
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    /// <code>*(long*)((nint)HousingManager.Instance()->IndoorTerritory + 38560) >> 32</code>
    /// <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// <para><c>param3</c>: InventoryType</para>
    /// <para><c>param4</c>: InventorySlot</para>
    /// </remarks>
    StoreFurniture = 1112,

    /// <summary>
    /// 从房屋中取回指定的家具
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    /// <code>(long)((nint)HousingManager.Instance()->IndoorTerritory + 38560) >> 32</code>
    /// <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// <para><c>param3</c>: InventoryType (25000 至 25010 / 27000 至 27008)</para>
    /// <para><c>param4</c>: InventorySlot (若 >65535 则将 slot 为 (i - 65536) 的家具收入仓库)</para>
    /// </remarks>
    RestoreFurniture = 1113,

    /// <summary>
    /// 请求房屋名称设置数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    /// <code>*(long*)((nint)HousingManager.Instance()->IndoorTerritory + 38560) >> 32</code>
    /// <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// </remarks>
    RequestHousingName = 1114,
    
    /// <summary>
    /// 请求房屋问候语设置数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    /// <code>*(long*)((nint)HousingManager.Instance()->IndoorTerritory + 38560) >> 32</code>
    /// <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// </remarks>
    RequestHousingGreeting = 1115,
    
    /// <summary>
    /// 请求房屋访客权限设置数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    /// <code>*(long*)((nint)HousingManager.Instance()->IndoorTerritory + 38560) >> 32</code>
    /// <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// </remarks>
    RequestHousingGuestAccess = 1117,
    
    /// <summary>
    /// 保存房屋访客权限设置
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    /// <code>*(long*)((nint)HousingManager.Instance()->IndoorTerritory + 38560) >> 32</code>
    /// <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// <para><c>param3</c>: 设置枚举值组合 (已知: 1 - 传送权限; 65536 - 进入权限)</para>
    /// </remarks>
    SaveHousingGuestAccess = 1118,
    
    /// <summary>
    /// 请求房屋宣传设置数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    /// <code>*(long*)((nint)HousingManager.Instance()->IndoorTerritory + 38560) >> 32</code>
    /// <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// </remarks>
    RequestHousingEstateTag = 1119,
    
    /// <summary>
    /// 保存房屋宣传设置
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    /// <code>*(long*)((nint)HousingManager.Instance()->IndoorTerritory + 38560) >> 32</code>
    /// <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// <para><c>param3</c>: 设置枚举值组合 (注: 即使是相同名称的 Tag 在不同位置上对应的枚举值也不同)</para>
    /// </remarks>
    SaveHousingEstateTag = 1120,
    
    /// <summary>
    /// 移动到庭院门前
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 地块索引</para>
    /// </remarks>
    MoveToHouseFrontGate = 1122,

    /// <summary>
    /// 进入到"布置家具/庭具"状态
    /// </summary>
    /// <remarks>
    /// <para><c>param2</c>: 房屋地块索引 (公寓为 0)</para>
    /// </remarks>
    FurnishState = 1123,
    
    /// <summary>
    /// 查看房屋详情
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Territory Type</para>
    /// <para><c>param2</c>: 地皮对应索引</para>
    /// <code>
    /// <![CDATA[
    /// const int HousesPerArea = 60;
    /// const int AreaOffset = 256;
    /// 
    /// // 第 1 区 第 41 号
    /// var wardID = 0;
    /// var districtOffset = wardID * AreaOffset;
    /// var houseID = 40;
    /// var position = districtOffset + houseID]]>
    /// </code>
    /// <para><c>param3</c>: (若有)公寓房间索引</para>
    /// </remarks>
    ViewHouseDetail = 1126,

    /// <summary>
    /// 调整房间亮度
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 亮度等级 (最亮为 0, 最暗为 5)</para>
    /// </remarks>
    AdjustHouseLight = 1137,

    /// <summary>
    /// 刷新部队合建物品交纳信息
    /// </summary>
    RefreshFCMaterialDelivery = 1143,
    
    /// <summary>
    /// 设置房屋背景音乐
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 管弦乐曲在 Orchestrion.csv 中的对应索引</para>
    /// </remarks>
    SetHouseBackgroundMusic = 1145,

    /// <summary>
    /// 从房屋仓库中取出布置指定物品
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: HouseManager 相关区域的 HouseID 地址的高 32 位</para>
    /// <code>*(long*)((nint)HousingManager.Instance()->IndoorTerritory + 38560) >> 32</code>
    /// <para><c>param2</c>: HouseManager 相关区域的 HouseID</para>
    /// <para><c>param3</c>: InventoryType (25000 至 25010 / 27000 至 27008)</para>
    /// <para><c>param4</c>: InventorySlot</para>
    /// </remarks>
    Furnish = 1150,

    /// <summary>
    /// 修理潜水艇部件
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 潜水艇索引</para>
    /// <para><c>param2</c>: 潜水艇部件索引</para>
    /// </remarks>
    RepairSubmarinePart = 1153,
    
    /// <summary>
    /// 请求房屋内部改建信息
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 房屋索引 (从 0 开始, 59 结束)</para>
    /// </remarks>
    HouseInteriorDesignRequest = 1169,
    
    /// <summary>
    /// 更改房屋内部装修风格
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 房屋索引 (从 0 开始, 59 结束)</para>
    /// <para><c>param1</c>: 内部装修风格 (3 - 海雾村风格; 6 - 薰衣草苗圃风格; 9 - 高脚孤丘风格; 12 - 白银乡风格; 15 - 穹顶皓天风格; 18 - 简装风格)</para>
    /// </remarks>
    HouseInteriorDesignChange = 1170,

    /// <summary>
    /// 领取战利水晶
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 赛季 (0 - 本赛季; 1 - 上赛季)</para>
    /// </remarks>
    CollectTrophyCrystal = 1200,

    /// <summary>
    /// 请求挑战笔记数据
    /// </summary>
    RequestContentsNote = 1301,
    
    /// <summary>
    /// 请求雇员探险时间信息
    /// </summary>
    RequestRetainerVentureTime = 1400,

    /// <summary>
    /// 在 NPC 处维修装备
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Inventory Type</para>
    /// <para><c>param2</c>: Inventory Slot</para>
    /// <para><c>param3</c>: Item ID</para>
    /// </remarks>
    RepairItemNPC = 1600,
    
    /// <summary>
    /// 在 NPC 处批量维修装备
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 分类 (0 - 主手/副手; 1 - 头部/身体/手臂; 2 - 腿部/脚部; 3 - 耳部;颈部; 4 - 腕部;戒指; 5 - 物品)</para>
    /// </remarks>
    RepairAllItemsNPC = 1601,

    /// <summary>
    /// 在 NPC 处批量维修装备中装备
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Inventory Type (固定为 1000)</para>
    /// </remarks>
    RepairEquippedItemsNPC = 1602,

    /// <summary>
    /// 切换陆行鸟作战风格
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: BuddyAction.csv 中的对应索引</para>
    /// </remarks>
    BuddyAction = 1700,
    
    /// <summary>
    /// 陆行鸟装甲
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 部位 (0 - 头部, 1 - 身体, 2 - 腿部)</para>
    /// <para><c>param2</c>: 在 BuddyEquip.csv 中对应的装备索引 (0 - 卸下装备)</para>
    /// </remarks>
    BuddyEquip = 1701,
    
    /// <summary>
    /// 陆行鸟学习技能
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Skill 索引</para>
    /// </remarks>
    BuddyLearnSkill = 1702,

    /// <summary>
    /// 请求金碟游乐场面板 整体 信息
    /// </summary>
    RequestGSGeneral = 1850,
    
    /// <summary>
    /// 开始任务回顾
    /// </summary>
    StartDutyRecord = 1980,
    
    /// <summary>
    /// 结束任务回顾
    /// </summary>
    EndDutyRecord = 1981,

    /// <summary>
    /// 请求金碟游乐场面板 萌宠之王 信息
    /// </summary>
    RequestGSLordofVerminion = 2010,

    /// <summary>
    /// 启用/解除自动加入新人频道设置
    /// </summary>
    EnableAutoJoinNoviceNetwork = 2102,

    /// <summary>
    /// 发起决斗
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 被决斗对象的 GameObject ID</para>
    /// </remarks>
    SendDuel = 2200,

    /// <summary>
    /// 确认决斗申请
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 0 - 确认; 1 - 取消; 4 - 强制取消</para>
    /// </remarks>
    RequestDuel = 2201,

    /// <summary>
    /// 同意决斗
    /// </summary>
    ConfirmDuel = 2202,

    /// <summary>
    /// 确认天书奇谈副本结果
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 索引 (从左到右从上到下, 从 0 开始)</para>
    /// </remarks>
    WondrousTailsConfirm = 2253,

    /// <summary>
    /// 天书奇谈其他操作
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 操作 (0 - 再想想)</para>
    /// <para><c>param2</c>: 索引 (从左到右从上到下, 从 0 开始)</para>
    /// </remarks>
    WondrousTailsOperate = 2253,

    /// <summary>
    /// 请求投影台数据
    /// </summary>
    RequestPrismBox = 2350,

    /// <summary>
    /// 取出投影台物品
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 投影台内部物品 ID (MirageManager.Instance().PrismBoxItemIds)</para>
    /// </remarks>
    RestorePrsimBoxItem = 2352,

    /// <summary>
    /// 请求投影模板数据
    /// </summary>
    RequestGlamourPlates = 2355,

    /// <summary>
    /// 进入/退出投影模板选择状态
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 0 - 退出, 1 - 进入</para>
    /// <para><c>param2</c>: 未知, 可能为 0 或 1</para>
    /// </remarks>
    EnterGlamourPlateState = 2356,

    /// <summary>
    /// 应用投影模板 (需要先进入投影模板选择状态)
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 投影模板索引</para>
    /// </remarks>
    ApplyGlamourPlate = 2357,
    
    /// <summary>
    /// 获取时尚品鉴每周参与奖励
    /// </summary>
    FashionCheckEntryReward = 2450,
    
    /// <summary>
    /// 获取时尚品鉴每周额外奖励
    /// </summary>
    FashionCheckBonusReward = 2451,

    /// <summary>
    /// 请求金碟游乐场面板 多玛方城战 信息
    /// </summary>
    RequestGSMahjong = 2550,

    /// <summary>
    /// 请求青魔法书数据
    /// </summary>
    RequstAOZNotebook = 2601,

    /// <summary>
    /// 请求亲信战友数据
    /// </summary>
    RequestTrustedFriend = 2651,

    /// <summary>
    /// 请求剧情辅助器数据
    /// </summary>
    RequestDutySupport = 2653,
    
    /// <summary>
    /// 发送剧情辅助器申请请求
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: DawnStroy 序号</para>
    /// <para><c>param2</c>: 前四位 DawnStroyMemberUIParam 序号的幂次方 (a1 * 256^0 + a2 * 256^1 + a3 * 256^2 + a4 * 256^3)</para>
    /// <para><c>param2</c>: 后三位 DawnStroyMemberUIParam 序号的幂次方 (a1 * 256^0 + a2 * 256^1 + a3 * 256^2)</para>
    /// </remarks>
    SendDutySupport = 2654,

    /// <summary>
    /// 分解指定的物品 / 回收指定物品的魔晶石 / 精选指定物品
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 分解: 3735552; 回收魔晶石: 3735553; 精选: 3735554</para>
    /// <para><c>param2</c>: Inventory Type</para>
    /// <para><c>param3</c>: Inventory Slot</para>
    /// <para><c>param4</c>: 物品 ID</para>
    /// </remarks>
    Desynthesize = 2800,
    
    /// <summary>
    /// 博兹雅分配失传技能库到技能槽
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 失传技能库索引</para>
    /// <para><c>param2</c>: 要分配的槽位</para>
    /// </remarks>
    BozjaUseFromHolster = 2950,
    
    /// <summary>
    /// 请求肖像列表数据
    /// </summary>
    RequestPortraits = 3200,
    
    /// <summary>
    /// 切换无人岛模式
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 模式 (0 - 自由; 1 - 收获; 2 - 播种; 3 - 浇水; 4 - 铲除; 6 - 喂食; 7 - 宠爱; 8 - 招呼; 9 - 捕兽)</para>
    /// </remarks>
    MJISetMode = 3250,
    
    /// <summary>
    /// 设置无人岛模式参数, 切换时会被设置为 0, 如播种、喂食、捕兽时会为对应的物品 ID
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 参数</para>
    /// </remarks>
    MJISetModeParam = 3251,
    
    /// <summary>
    /// 无人岛设置面板开关
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 状态 (1 - 开启; 0 - 关闭)</para>
    /// </remarks>
    MJISettingPanelToggle = 3252,

    /// <summary>
    /// 请求无人岛工房排班数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 具体天数 (0 为本周期第一天, 7 为下周期第一天)</para>
    /// </remarks>
    MJIWorkshopRequest = 3254,
    
    /// <summary>
    /// 请求无人岛工房排班物品数据
    /// </summary>
    MJIWorkshopRequestItem = 3258,

    /// <summary>
    /// 无人岛工房排班
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 物品和排班时间段: (8 * (startingHour | (32 * craftObjectId)))</para>
    /// <para><c>param2</c>: 具体天数 (0 - 本周期第一天, 7 - 下周期第一天)</para>
    /// <para><c>param4</c>: 添加/删除 (0 - 添加, 1 - 删除)</para>
    /// </remarks>
    MJIWorkshopAssign = 3259,

    /// <summary>
    /// 取消无人岛工坊排班
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 物品和排班时间段: (8 * (startingHour | (32 * craftObjectId)))</para>
    /// <para><c>param2</c>: 具体天数 (0 - 本周期第一天, 7 - 下周期第一天)</para>
    /// </remarks>
    MJIWorkshopCancel = 3260,
    
    /// <summary>
    /// 设置无人岛休息周期
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 休息日 1</para>
    /// <para><c>param2</c>: 休息日 2</para>
    /// <para><c>param3</c>: 休息日 3</para>
    /// <para><c>param4</c>: 休息日 4</para>
    /// </remarks>
    MJISetRestCycles = 3261,

    /// <summary>
    /// 收取无人岛屯货仓库探索结果
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 仓库索引</para>
    /// </remarks>
    MJIGranaryCollect = 3262,

    /// <summary>
    /// 查看无人岛屯货仓库探索目的地
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 仓库索引</para>
    /// </remarks>
    MJIGranaryViewDestinations = 3263,

    /// <summary>
    /// 无人岛屯货仓库派遣探险
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 仓库索引</para>
    /// <para><c>param2</c>: 目的地索引</para>
    /// <para><c>param3</c>: 探索天数</para>
    /// </remarks>
    MJIGranaryAssign = 3264,
    
    /// <summary>
    /// 在无人岛放养宠物
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 宠物 ID</para>
    /// <para><c>param2</c>: 放生区域索引</para>
    /// </remarks>
    MJIReleaseMinion = 3265,
    
    /// <summary>
    /// 放生无人岛牧场动物
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 动物索引</para>
    /// </remarks>
    MJIReleaseAnimal = 3268,
    
    /// <summary>
    /// 收集无人岛牧场动物产物
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 动物索引</para>
    /// <para><c>param2</c>: 收集标志</para>
    /// </remarks>
    MJICollectAnimalLeavings = 3269,
    
    /// <summary>
    /// 收取无人岛牧场全部动物产物
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 预期收集的产物数量 (MJIManager.Instance()->PastureHandler->AvailableMammetLeavings)</para>
    /// </remarks>
    MJICollectAllAnimalLeavings = 3271,
    
    /// <summary>
    /// 托管无人岛牧场动物
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 动物索引</para>
    /// <para><c>param2</c>: 喂食物品 ID</para>
    /// </remarks>
    MJIEntrustAnimal = 3272,
    
    /// <summary>
    /// 召回无人岛放生的宠物
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 宠物索引</para>
    /// </remarks>
    MJIRecallMinion = 3277,

    /// <summary>
    /// 托管单块无人岛耕地
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 耕地索引</para>
    /// <para><c>param2</c>: 种子物品 ID</para>
    /// </remarks>
    MJIFarmEntrustSingle = 3279,

    /// <summary>
    /// 取消托管单块无人岛耕地
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 耕地索引</para>
    /// </remarks>
    MJIFarmDismiss = 3280,

    /// <summary>
    /// 收取单块无人岛耕地
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 耕地索引</para>
    /// <para><c>param2</c>: 收取后是否取消托管 (0 - 否, 1 - 是)</para>
    /// </remarks>
    MJIFarmCollectSingle = 3281,

    /// <summary>
    /// 收取全部无人岛耕地
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: *(int*)MJIManager.Instance()->GranariesState</para>
    /// </remarks>
    MJIFarmCollectAll = 3282,

    /// <summary>
    /// 请求无人岛工房需求数据
    /// </summary>
    MJIFavorStateRequest = 3292,
    
    /// <summary>
    /// 变更宇宙探索模式
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 模式索引</para>
    /// </remarks>
    WKSChangeMode = 3400,
    
    /// <summary>
    /// 宇宙探索结束交互1
    /// </summary>
    WKSEndInteraction1 = 3401,
    
    /// <summary>
    /// 宇宙探索结束交互2
    /// </summary>
    WKSEndInteraction2 = 3402,
    
    /// <summary>
    /// 宇宙探索接取任务
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: Mission Unit ID</para>
    /// </remarks>
    WKSStartMission = 3440,
    
    /// <summary>
    /// 宇宙探索完成任务
    /// </summary>
    WKSCompleteMission = 3441,
    
    /// <summary>
    /// 宇宙探索放弃任务
    /// </summary>
    WKSAbandonMission = 3442,
    
    /// <summary>
    /// 宇宙好运道开始抽奖
    /// </summary>
    WKSStartLottery = 3450,
    
    /// <summary>
    /// 宇宙好运道选择转盘
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 转盘类型 (左边 - 0, 右边 - 1)</para>
    /// </remarks>
    WKSChooseLottery = 3451,
    
    /// <summary>
    /// 宇宙好运道结束抽奖
    /// </summary>
    WKSEndLottery = 3452,
    
    /// <summary>
    /// 宇宙探索请求机甲数据
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: WKSMechaEventData Row ID (0 - 当前未开始)</para>
    /// </remarks>
    WKSRequestMecha = 3478,

    /// <summary>
    /// 掷骰子
    /// </summary>
    /// <remarks>
    /// <para><c>param1</c>: 类型 (固定为 0)</para>
    /// <para><c>param2</c>: 最大值</para>
    /// </remarks>
    RollDice = 9000,

    /// <summary>
    /// 雇员
    /// </summary>
    Retainer = 9003,
}
