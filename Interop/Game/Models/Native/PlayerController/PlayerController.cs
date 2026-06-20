using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace OmenTools.Interop.Game.Models.Native;

[StructLayout(LayoutKind.Explicit, Size = 1424)]
public unsafe struct PlayerController
{
    [FieldOffset(16)]
    public PlayerMoveControllerWalk MoveControllerWalk;

    [FieldOffset(336)]
    public PlayerMoveControllerFly MoveControllerFly;
    
    /// <summary>
    ///     移动模式
    /// </summary>
    [FieldOffset(512)]
    public MovementModeType MovementMode;

    /// <summary>
    ///     缓存的走路速度
    /// </summary>
    [FieldOffset(472)]
    public float CachedWalkSpeed;

    /// <summary>
    ///     缓存的奔跑速度
    /// </summary>
    [FieldOffset(476)]
    public float CachedRunSpeed;

    /// <summary>
    ///     走路速度目标过渡时长 (哨兵 -1.0f)
    /// </summary>
    [FieldOffset(484)]
    public float WalkSpeedBlendDuration;

    /// <summary>
    ///     跑步速度目标过渡时长 (哨兵 -1.0f)
    /// </summary>
    [FieldOffset(488)]
    public float RunSpeedBlendDuration;

    /// <summary>
    ///     速度过渡状态: 3-5 表示混合进行中
    /// </summary>
    [FieldOffset(492)]
    public int SpeedTransitionState;

    /// <summary>
    ///     俯仰/仰角状态标志: 5/6/7 等
    /// </summary>
    [FieldOffset(500)]
    public int MovementModeFlags;

    /// <summary>
    ///     是否为跑步状态
    /// </summary>
    [FieldOffset(510)]
    public byte IsRunning;

    /// <summary>
    ///     自动移动标志
    /// </summary>
    [FieldOffset(511)]
    public byte IsAutoMove;

    /// <summary>
    ///     未知移动标志
    /// </summary>
    [FieldOffset(514)]
    private byte UnknownMoveFlag514;

    /// <summary>
    ///     走路混合当前值
    /// </summary>
    [FieldOffset(536)]
    public float WalkBlendCurrent;

    /// <summary>
    ///     走路混合前一帧值
    /// </summary>
    [FieldOffset(540)]
    public float WalkBlendPrevious;

    /// <summary>
    ///     跑步混合当前值
    /// </summary>
    [FieldOffset(568)]
    public float RunBlendCurrent;

    /// <summary>
    ///     跑步混合前一帧值
    /// </summary>
    [FieldOffset(572)]
    public float RunBlendPrevious;

    /// <summary>
    ///     辅助移动控制器
    /// </summary>
    [FieldOffset(592)]
    private fixed byte MoveSubController592[116];

    [FieldOffset(708)]
    private byte UnknownFlag708;

    [FieldOffset(709)]
    private byte UnknownFlag709;

    /// <summary>
    ///     坐骑/伴随状态标志
    /// </summary>
    [FieldOffset(710)]
    public byte IsMountedOrCompanion;

    /// <summary>
    ///     移动子控制器前半部分
    /// </summary>
    [FieldOffset(720)]
    private fixed byte MoveSubController720A[36];

    /// <summary>
    ///     是否本地玩家
    /// </summary>
    [FieldOffset(756)]
    public byte IsLocalPlayerCached;

    /// <summary>
    ///     服务端跑步状态
    /// </summary>
    [FieldOffset(757)]
    public byte IsRunningServerSide;

    /// <summary>
    ///     移动子控制器后半部分
    /// </summary>
    [FieldOffset(758)]
    private fixed byte MoveSubController720B[26];
    
    /// <summary>
    ///     移动路径状态 (6 = 初始化)
    /// </summary>
    [FieldOffset(768)]
    public int MovementPathState;

    /// <summary>
    ///     移动方向 X (归一化) / 位置混合起始X (Mode 0)
    /// </summary>
    [FieldOffset(784)]
    public float MoveDirX;

    /// <summary>
    ///     移动方向 Z (归一化) / 位置混合起始Y (Mode 0)
    /// </summary>
    [FieldOffset(788)]
    public float MoveDirZ;

    /// <summary>
    ///     Mode 1: 推挤初始速度 (二次减速: d = v₀t - ½at², a=0.015)
    ///     Mode 0: 位置混合起始Z
    /// </summary>
    [FieldOffset(792)]
    public float PushInitialSpeed;

    /// <summary>
    ///     Mode 2: 动画驱动移动速度倍率
    ///     Mode 0: 位置混合结束X
    /// </summary>
    [FieldOffset(796)]
    public float AnimMoveSpeedMul;

    /// <summary>
    ///     位置混合结束Y
    /// </summary>
    [FieldOffset(800)]
    public float BlendEndY;

    /// <summary>
    ///     垂直速度 (跳跃/重力: v -= 30*dt, 终端速度 -25)
    ///     Mode 0: 位置混合结束Z
    /// </summary>
    [FieldOffset(804)]
    public float VerticalVelocity;

    /// <summary>
    ///     爆发/冲量方向 X (临时定向推动效果)
    ///     Mode 0: 角度混合起始
    /// </summary>
    [FieldOffset(808)]
    public float BurstDirX;

    /// <summary>
    ///     爆发/冲量方向 Z
    ///     Mode 0: 角度混合结束
    /// </summary>
    [FieldOffset(812)]
    public float BurstDirZ;

    /// <summary>
    ///     帧时间累加器 (被 FrameTimeLimit 钳制)
    ///     Mode 0: 位置混合时长
    /// </summary>
    [FieldOffset(816)]
    public float AccumulatedTime;

    /// <summary>
    ///     爆发/冲量剩余时间预算
    ///     Mode 0: 角度混合时长
    /// </summary>
    [FieldOffset(820)]
    public float BurstRemaining;

    /// <summary>
    ///     最大速度 / Mode 0: 位置混合起始阈值
    /// </summary>
    [FieldOffset(824)]
    public float MaxSpeed;

    /// <summary>
    ///     帧时间上限 / Mode 0: 角度混合起始阈值
    /// </summary>
    [FieldOffset(828)]
    public float FrameTimeLimit;

    /// <summary>
    ///     Mode 0: 混合已用时间 (每帧累加 deltaTime)
    /// </summary>
    [FieldOffset(832)]
    public float BlendElapsed;

    /// <summary>
    ///     运动模式: 0=正常(Lerp), 1=推挤(二次减速), 2=动画曲线驱动
    ///     (与 BlendElapsed 共享低字节)
    /// </summary>
    [FieldOffset(832)]
    public byte MoveMode;

    /// <summary>
    ///     Mode 2: 动画曲线索引 (传入 EvaluateMovementCurve)
    /// </summary>
    [FieldOffset(833)]
    public byte AnimCurveIndex;

    /// <summary>
    ///     角度混合标志 (传入 EvaluateMovementCurve 作为曲线类型)
    /// </summary>
    [FieldOffset(837)]
    public byte BlendAngleFlag;

    /// <summary>
    ///     位置混合完成标志: 置位后触发状态转换
    /// </summary>
    [FieldOffset(838)]
    public byte BlendPosComplete;

    /// <summary>
    ///     立即设置标志: 位置混合开始时强制设置状态
    /// </summary>
    [FieldOffset(839)]
    public byte BlendImmediateSet;

    /// <summary>
    ///     目标实体 ID (0xE0000000 = 无实体)
    /// </summary>
    [FieldOffset(840)]
    public uint TargetEntityID;

    /// <summary>
    ///     目标实体 ID 高字
    /// </summary>
    [FieldOffset(844)]
    public uint TargetEntityIDHigh;

    /// <summary>
    ///     移动输入待处理标志
    /// </summary>
    [FieldOffset(848)]
    public bool MovementInputPending;

    /// <summary>
    ///     移动活跃标志
    /// </summary>
    [FieldOffset(849)]
    public bool MovementActive;

    /// <summary>
    ///     控制标志 Byte5 (非零时设置控制位掩码)
    /// </summary>
    [FieldOffset(853)]
    public bool ControlFlagByte5;

    /// <summary>
    ///     强制移动重置
    /// </summary>
    [FieldOffset(854)]
    public bool ForceMovementReset;

    /// <summary>
    ///     移动跳跃类型 (读取 HIBYTE: ==1 分支跳跃模式 vs 普通模式)
    /// </summary>
    [FieldOffset(855)]
    public byte MovementHopType;

    /// <summary>
    ///     位置更新完成标志 (SetPosition+SetRotation 后设为 1)
    /// </summary>
    [FieldOffset(856)]
    public bool PositionUpdateComplete;

    /// <summary>
    ///     位置更新标志 2 (sub_1417FA110 区域读写)
    /// </summary>
    [FieldOffset(859)]
    public bool PositionUpdateFlag2;

    /// <summary>
    ///     GainStatus 速度参数 (sub_1418109B0 从 OnGainStatus 写入)
    /// </summary>
    [FieldOffset(864)]
    public float GainStatusSpeedParam;

    /// <summary>
    ///     待处理移动速度
    /// </summary>
    [FieldOffset(868)]
    public float PendingMoveSpeed;

    /// <summary>
    ///     GainStatus 状态 ID
    /// </summary>
    [FieldOffset(872)]
    public ushort GainStatusID;

    /// <summary>
    ///     未知/填充
    /// </summary>
    [FieldOffset(876)]
    private uint Padding876;

    /// <summary>
    ///     移动路径句柄
    /// </summary>
    [FieldOffset(880)]
    public void* MovementPathHandle;

    /// <summary>
    ///     未知/填充区域 (888-895)
    /// </summary>
    [FieldOffset(888)]
    private fixed byte UnknownPadding888[8];
    
    /// <summary>
    ///     垂直/飞行子控制器
    /// </summary>
    [FieldOffset(896)]
    private fixed byte VerticalSubController[8];

    /// <summary>
    ///     指向状态结构的指针 (首字段为 DWORD 状态码)
    ///     当 *ptr == 1 时更新 VerticalSubController
    /// </summary>
    [FieldOffset(904)]
    public void* StatusStructPtr;

    /// <summary>
    ///     动作移动子控制器
    /// </summary>
    [FieldOffset(912)]
    private fixed byte ActionMoveSubController[8];

    /// <summary>
    ///     动作类型指针 (可空, *ptr == 2 时触发特殊代码路径)
    /// </summary>
    [FieldOffset(920)]
    public void* ActionTypePtr;

    /// <summary>
    ///     目标位置 X
    /// </summary>
    [FieldOffset(936)]
    public long TargetPositionX;

    /// <summary>
    ///     目标位置 Y / 目标路点 ID
    /// </summary>
    [FieldOffset(944)]
    public long TargetPositionY;

    /// <summary>
    ///     目标位置 Z
    /// </summary>
    [FieldOffset(952)]
    public int TargetPositionZ;

    /// <summary>
    ///     是否有目标位置标志
    /// </summary>
    [FieldOffset(956)]
    public byte HasTargetFlag;

    /// <summary>
    ///     目标额外数据
    /// </summary>
    [FieldOffset(960)]
    public fixed byte TargetExtraData[32];
    
    /// <summary>
    ///     受控的 BattleChara/GameObject 指针
    /// </summary>
    [FieldOffset(992)]
    public GameObject* ControlledChara;
    
    /// <summary>
    ///     动作额外参数 (使用后连同 ActionState/CallbackIndex 一起清零)
    /// </summary>
    [FieldOffset(1004)]
    public uint ActionExtraParam;

    /// <summary>
    ///     动作状态: 0=空闲, 3=施法中, 7-9=热键栏切换
    /// </summary>
    [FieldOffset(1008)]
    public byte ActionState;

    /// <summary>
    ///     回调索引 (0-9, 1-10 映射到 vtable 回调)
    ///     0 表示跳过调用
    /// </summary>
    [FieldOffset(1009)]
    public byte CallbackIndex;

    /// <summary>
    ///     期望移动速度
    /// </summary>
    [FieldOffset(1020)]
    public float DesiredSpeed;

    /// <summary>
    ///     当前绝对移动速度
    /// </summary>
    [FieldOffset(1024)]
    public float CurrentAbsoluteSpeed;

    /// <summary>
    ///     坐骑过渡状态
    /// </summary>
    [FieldOffset(1028)]
    public int MountTransitionState;

    /// <summary>
    ///     指令定时器
    /// </summary>
    [FieldOffset(1032)]
    public float CommandTimer;

    /// <summary>
    ///     地面钳制冷却计时器 (最多 10 次迭代)
    /// </summary>
    [FieldOffset(1036)]
    public int GroundClampCooldown;
    
    /// <summary>
    ///     子移动状态 (1-3 会触发别的函数)
    /// </summary>
    [FieldOffset(1040)]
    public byte SubMoveState;

    /// <summary>
    ///     位置同步标志 (调用 SetPosition/SetRotation 后设置为 2)
    /// </summary>
    [FieldOffset(1041)]
    public ushort PositionSyncFlag;
    
    /// <summary>
    ///     在新跟随开始时, 这里会被设置
    /// </summary>
    [FieldOffset(1072)]
    public GameObjectId FollowTargetIDStart;
    
    /// <summary>
    ///     行为标志 1:
    ///       1 = 活跃移动中
    ///       2 = 锁定 Y 坐标 (地面/动画锁定)
    ///       0x20 = 坐骑状态
    /// </summary>
    [FieldOffset(1080)]
    public byte BehaviorFlags;

    /// <summary>
    ///     行为标志 2:
    ///       2 = 完全跳过物理更新 (过场/传送/冻结)
    /// </summary>
    [FieldOffset(1081)]
    public byte SkipPhysicsFlags;

    /// <summary>
    ///     输入冷却定时器 (每帧减去 FrameDeltaTime)
    /// </summary>
    [FieldOffset(1084)]
    public float CountdownTimer;
    
    /// <summary>
    ///     跳跃前保存的安全地面位置
    /// </summary>
    [FieldOffset(1088)]
    public Vector3 SavedGroundPosition;

    /// <summary>
    ///     FollowRequest 结构体
    /// </summary>
    [FieldOffset(1104)]
    public FollowRequest FollowRequestData;
    
    /// <summary>
    ///     跟随行为 vtable[0] (类型 3 - 移动跟随)
    /// </summary>
    [FieldOffset(1120)]
    private void* FollowBehaviorVtable0;

    /// <summary>
    ///     已保存的步行状态 (缓存 IsWalking, 开始类型 3 跟随时设置)
    /// </summary>
    [FieldOffset(1128)]
    public byte SavedWalkingState;

    /// <summary>
    ///     跟随移动激活标志 (1=活跃, 0=停止)
    /// </summary>
    [FieldOffset(1129)]
    public ushort FollowMovementActive;

    /// <summary>
    ///     跟随行为 vtable[1] (类型 4 - 目标跟随)
    /// </summary>
    [FieldOffset(1136)]
    private void* FollowBehaviorVtable1;

    /// <summary>
    ///     跟随目标位置
    /// </summary>
    [FieldOffset(1152)]
    public Vector3 FollowDestination;

    /// <summary>
    ///     最后已知目标位置
    /// </summary>
    [FieldOffset(1168)]
    public Vector3 FollowLastKnownPosition;
    
    /// <summary>
    ///     跟随目标 GameObjectID (主引用, StartFollow 类型 4 设置)
    /// </summary>
    [FieldOffset(1200)]
    public GameObjectId FollowPrimaryTargetID;

    /// <summary>
    ///     当前跟随 GameObjectID (空闲时为 0xE0000000)
    /// </summary>
    [FieldOffset(1208)]
    public GameObjectId FollowCurrentTargetID;

    /// <summary>
    ///     跟随路点/计数器索引
    /// </summary>
    [FieldOffset(1216)]
    public uint FollowWaypointIndex;

    /// <summary>
    ///     跟随距离阈值 (默认 256 单位)
    /// </summary>
    [FieldOffset(1220)]
    public ushort FollowDistanceThreshold;

    /// <summary>
    ///     跟随忙碌/处理中标志
    /// </summary>
    [FieldOffset(1221)]
    public bool IsFollowBusy;

    /// <summary>
    ///     跟随行为 vtable[2] (类型 2 - 对象跟随)
    /// </summary>
    [FieldOffset(1232)]
    private void* FollowBehaviorVtable2;

    /// <summary>
    ///     跟随次要目标 GameObjectID (类型 2 对象跟随时设置)
    /// </summary>
    [FieldOffset(1240)]
    public GameObjectId FollowSecondaryTargetID;

    /// <summary>
    ///     跟随行为 vtable[3] (类型 5 - 卡住/恢复)
    /// </summary>
    [FieldOffset(1248)]
    private void* FollowBehaviorVtable3;

    /// <summary>
    ///     跟随卡住/移动恢复标志 (类型 5 设为 1, 类型 6 清零)
    /// </summary>
    [FieldOffset(1256)]
    public byte FollowStuckFlag;

    /// <summary>
    ///     AreaVfx 指针环形缓冲区 (6×8 字节, 索引由 AreaVfxRingIndex 管理)
    /// </summary>
    [FieldOffset(1264)]
    private fixed long AreaVfxRingBuffer[6];

    /// <summary>
    ///     PathRequestHandle 查找表 (10 个 uint64 条目, 索引由 CallbackIndex 管理)
    ///     用于进行路径执行调度
    /// </summary>
    [FieldOffset(1312)]
    public long PathRequestHandle0;

    [FieldOffset(1320)]
    public long PathRequestHandle1;

    /// <summary>
    ///     ClientPath 布局实例 ID (同时作为 PathRequestHandle[2], 传入 LayoutWorld::GetLayoutInstanceStatic)
    /// </summary>
    [FieldOffset(1328)]
    public uint ClientPathLayoutInstanceID;

    [FieldOffset(1332)]
    private uint Padding1332;

    /// <summary>
    ///     移动路径实例句柄 (同时作为 PathRequestHandle[3], 进行移动初始化)
    /// </summary>
    [FieldOffset(1336)]
    public long MovementPathInstanceHandle;

    [FieldOffset(1344)]
    public long PathRequestHandle4;

    [FieldOffset(1352)]
    public long PathRequestHandle5;

    [FieldOffset(1360)]
    public long PathRequestHandle6;

    [FieldOffset(1368)]
    public long PathRequestHandle7;

    /// <summary>
    ///     PathRequestHandle[8] / 定时移动调度句柄 (传入 sub_141809230)
    /// </summary>
    [FieldOffset(1376)]
    public long TimedMoveDispatchHandle;

    /// <summary>
    ///     最后碰撞检测时间戳 (QueryPerformanceCounter, 1000ms 冷却检查)
    /// </summary>
    [FieldOffset(1392)]
    public long LastCollisionCheckTimestamp;

    /// <summary>
    ///     碰撞/路径门控标志 (==0.0f 时允许触发)
    /// </summary>
    [FieldOffset(1400)]
    public float CollisionPathGateFlag;

    /// <summary>
    ///     跟随 AreaVfx 定时器
    /// </summary>
    [FieldOffset(1400)]
    public float FollowAreaVfxTimer;

    /// <summary>
    ///     跟随条件/权限标志
    /// </summary>
    [FieldOffset(1404)]
    public uint FollowPermissionFlag;

    /// <summary>
    ///     ControlStateBitmask
    /// </summary>
    [FieldOffset(1408)]
    public uint ControlStateBitmask;

    /// <summary>
    ///     MoveState 兼容属性 (ControlStateBitmask 的 byte 1)
    ///     0 - 无, 1 - 正常行走, 3 - 自动前进, 4 - 跟随
    /// </summary>
    public byte MoveState
    {
        get => (byte)(ControlStateBitmask >> 8);
        set => ControlStateBitmask = (ControlStateBitmask & 0xFFFF00FF) | ((uint)value << 8);
    }

    /// <summary>
    ///     跟随类型 / 控制模式:
    ///     0 = 未跟随
    ///     2 = 对象跟随
    ///     3 = 移动跟随
    ///     4 = 目标跟随
    ///     5 = 卡住/恢复
    ///     InterruptFollow 处理后清零;
    /// </summary>
    [FieldOffset(1413)]
    public byte ControlMode;

    /// <summary>
    ///     移动控制器写入, MoveController 读取
    /// </summary>
    [FieldOffset(1414)]
    private byte Field7636;

    /// <summary>
    ///     IsWalking 标志
    /// </summary>
    [FieldOffset(1415)]
    public byte IsWalking;

    /// <summary>
    ///     field_7638[0..7] 标志数组
    /// </summary>
    [FieldOffset(1416)]
    private fixed byte Field7638[8];
    
    
    private static readonly CompSig           InstanceSig = new("48 8D 0D ?? ?? ?? ?? 0F B6 9A");
    private static          PlayerController* instance;

    public static PlayerController* Instance()
    {
        if (instance == null)
            instance = InstanceSig.GetStatic<PlayerController>();

        return instance;
    }

    private static readonly CompSig                 InterruptFollowSig = new("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B D9 48 8B FA 0F B6 89");
    private delegate        void                    InterruptFollowDelegate(PlayerController* playerController, FollowRequest* followRequest);
    private static readonly InterruptFollowDelegate InterruptFollowPtr = InterruptFollowSig.GetDelegate<InterruptFollowDelegate>();

    /// <summary>
    ///     中断跟随
    /// </summary>
    public void InterruptFollow()
    {
        fixed (PlayerController* controller = &this)
        fixed (FollowRequest* followRequest = &FollowRequestData)
            InterruptFollowPtr(controller, followRequest);
    }

    private static readonly CompSig             StartFollowSig = new("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC ?? 44 0F B6 7A 08");
    private delegate        void                StartFollowDelegate(PlayerController* playerController, FollowRequest* followRequest);
    private static readonly StartFollowDelegate StartFollowPtr = StartFollowSig.GetDelegate<StartFollowDelegate>();

    /// <summary>
    ///     开始跟随, 先改 FollowRequestData
    ///     类型 2 = 对象跟随, 类型 3 = 移动跟随, 类型 4 = 目标跟随, 类型 5 = 卡住/恢复
    /// </summary>
    public void StartFollow()
    {
        fixed (PlayerController* controller = &this)
        fixed (FollowRequest* followRequest = &FollowRequestData)
            StartFollowPtr(controller, followRequest);
    }

    private static readonly CompSig              StopMovementSig = new("48 89 5C 24 08 57 48 83 EC 30 48 8B D9 0F B6 FA");
    private delegate        void                 StopMovementDelegate(PlayerController* playerController, bool viaCommand);
    private static readonly StopMovementDelegate StopMovementPtr = StopMovementSig.GetDelegate<StopMovementDelegate>();

    /// <summary>
    ///     停止移动
    ///     viaCommand=true: 发送 ExecuteCommand.LeaveSwimState
    ///     viaCommand=false: 清除动作请求, 发送命令 ExecuteCommand.EnterSwimState
    /// </summary>
    public void StopMovement(bool viaCommand = false)
    {
        fixed (PlayerController* controller = &this)
            StopMovementPtr(controller, viaCommand);
    }

    private static readonly CompSig                CancelMovementSig = new("40 57 48 83 EC 20 0F B6 81 10 04 00 00 48 8B F9");
    private delegate        void                   CancelMovementDelegate(PlayerController* playerController);
    private static readonly CancelMovementDelegate CancelMovementPtr = CancelMovementSig.GetDelegate<CancelMovementDelegate>();

    /// <summary>
    ///     取消移动
    ///     设置移动状态为 2 (已取消), 重置导航状态, 取消动作请求, 停止表情
    /// </summary>
    public void CancelMovement()
    {
        fixed (PlayerController* controller = &this)
            CancelMovementPtr(controller);
    }

    public enum MovementModeType
    {
        Ground  = 0,
        Fly     = 2,
        Dive    = 3,
        Special = 5
    }

    public enum ActionStateType
    {
        Idle          = 0,
        Casting       = 3,
        HotBarSwitch7 = 7,
        HotBarSwitch8 = 8,
        HotBarSwitch9 = 9,
    }
}
