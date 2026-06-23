using System.Numerics;
using System.Runtime.InteropServices;

namespace OmenTools.Interop.Game.Models.Native;

// Also MoveControllerSubMemberForMine
[StructLayout(LayoutKind.Explicit, Size = 320)]
public unsafe struct PlayerMoveControllerWalk
{
    [FieldOffset(16)]
    public Vector3 MovementDirection;

    /// <summary>
    ///     指向所属 PlayerController 的反向指针
    /// </summary>
    [FieldOffset(32)]
    public PlayerController* Parent;

    /// <summary>
    ///     移动速度倍率 (钳制到 0.6-1.0)
    /// </summary>
    [FieldOffset(56)]
    public float SpeedMultiplier;

    /// <summary>
    ///     自动前进 (Auto-Run Forward) 激活标志
    /// </summary>
    [FieldOffset(60)]
    public bool IsAutoRunForward;

    /// <summary>
    ///     自动侧移 (Auto-Run Strafe) 激活标志
    /// </summary>
    [FieldOffset(61)]
    public bool IsAutoRunStrafe;

    /// <summary>
    ///     摄像机相对移动模式 (鼠标/手柄模式)
    /// </summary>
    [FieldOffset(62)]
    public bool IsCameraRelativeMode;

    /// <summary>
    ///     未知条件标志 (在 vf1 中检查)
    /// </summary>
    [FieldOffset(63)]
    private bool UnknownFlag63;

    /// <summary>
    ///     跳跃/触发标志: 激活时执行输入锁定逻辑
    /// </summary>
    [FieldOffset(64)]
    public bool IsJumping;

    /// <summary>
    ///     当前的移动速度
    /// </summary>
    [FieldOffset(68)]
    public float CurrentSpeed;

    /// <summary>
    ///     基础移动速度
    /// </summary>
    [FieldOffset(88)]
    public float BaseMovementSpeed;

    /// <summary>
    ///     是否有移动输入
    /// </summary>
    [FieldOffset(136)]
    public bool IsMovementInput;

    [FieldOffset(140)]
    private float Unknown140;

    /// <summary>
    /// 当前移动方向相对于角色面向的偏航角
    /// </summary>
    [FieldOffset(144)]
    public float MovementDirRelToCharacterFacing;

    /// <summary>
    ///     强制移动标志 / 有移动输入
    /// </summary>
    [FieldOffset(148)]
    public byte IsForced;

    /// <summary>
    ///     世界空间移动方向输出 (Vector3)
    /// </summary>
    [FieldOffset(160)]
    public Vector3 MovementDirectionWorld;

    /// <summary>
    ///     旋转方向
    /// </summary>
    [FieldOffset(176)]
    public float RotationDirection;

    /// <summary>
    ///     内部 vtable 指针 #1
    /// </summary>
    [FieldOffset(184)]
    private void* InternalVtable1;

    /// <summary>
    ///     内部 vtable 指针 #2
    /// </summary>
    [FieldOffset(192)]
    private void* InternalVtable2;

    /// <summary>
    ///     哨兵值 -1.0f 数组;
    /// </summary>
    [FieldOffset(200)]
    public fixed float SentinelFloat[5];

    /// <summary>
    ///     内部 vtable 指针 #3
    /// </summary>
    [FieldOffset(264)]
    private void* InternalVtable3;

    /// <summary>
    ///     移动模式 (值 3 表示特定模式如自动奔跑)
    /// </summary>
    [FieldOffset(272)]
    public uint MovementState;

    [FieldOffset(276)]
    public float MovementLeft;

    [FieldOffset(280)]
    public float MovementForward;

    /// <summary>
    ///     是否拒绝移动输入
    /// </summary>
    [FieldOffset(292)]
    public bool IsMovementInputLocked;

    /// <summary>
    ///     摄像机倾斜切换 (检测 CAM_TILT_UP/DOWN 输入)
    /// </summary>
    [FieldOffset(298)]
    public bool CameraTiltToggle;
    
    private delegate float GetTargetSpeedPtrDelegate(PlayerMoveControllerWalk* controller);
    private static readonly GetTargetSpeedPtrDelegate GetTargetSpeedPtr =
        new CompSig("E8 ?? ?? ?? ?? 0F 28 F0 E9 ?? ?? ?? ?? 8B 81").GetDelegate<GetTargetSpeedPtrDelegate>();

    /// <summary>
    ///     获取目标速度
    /// </summary>
    public float GetTargetSpeed()
    {
        fixed (PlayerMoveControllerWalk* controller = &this)
            return GetTargetSpeedPtr(controller);
    }

    private delegate bool IsAbleToSetMisdirectionStateDelegate(PlayerMoveControllerWalk* controller);
    private static readonly IsAbleToSetMisdirectionStateDelegate IsAbleToSetMisdirectionStatePtr =
        new CompSig("40 53 48 83 EC 20 48 8B D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 75 78").GetDelegate<IsAbleToSetMisdirectionStateDelegate>();

    /// <summary>
    ///     检查是否可以设置精神错乱状态
    /// </summary>
    public bool IsAbleToSetMisdirectionState()
    {
        fixed (PlayerMoveControllerWalk* controller = &this)
            return IsAbleToSetMisdirectionStatePtr(controller);
    }

    private static readonly CompSig                   InstanceSig = new("48 8D 0D ?? ?? ?? ?? 45 0F 28 D4");
    private static          PlayerMoveControllerWalk* instance;

    public static PlayerMoveControllerWalk* Instance()
    {
        if (instance == null)
            instance = InstanceSig.GetStatic<PlayerMoveControllerWalk>();

        return instance;
    }
}
