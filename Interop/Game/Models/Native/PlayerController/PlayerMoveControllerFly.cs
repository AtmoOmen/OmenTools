using System.Numerics;
using System.Runtime.InteropServices;

namespace OmenTools.Interop.Game.Models.Native;

[StructLayout(LayoutKind.Explicit, Size = 176)]
public struct PlayerMoveControllerFly
{
    /// <summary>
    ///     当前飞行旋转 (偏航角)
    /// </summary>
    [FieldOffset(0)]
    public float CurrentFlyRotation;

    /// <summary>
    ///     当前飞行速度 (位移量平方根)
    /// </summary>
    [FieldOffset(4)]
    public float CurrentFlySpeed;

    /// <summary>
    ///     这里是本地玩家的坐骑坐标, 当玩家处于飞行状态或者潜水时候, 修改这个坐标会修改本地玩家的位置
    /// </summary>
    [FieldOffset(16)]
    public Vector3 MountPosition;

    /// <summary>
    ///     起飞状态标志 1: 比较 ==1 然后设为 2 (状态机转换)
    /// </summary>
    [FieldOffset(38)]
    public byte FlyStateFlag1;

    /// <summary>
    ///     起飞状态标志 2: 混合完成时设为 1
    /// </summary>
    [FieldOffset(44)]
    public byte FlyStateFlag2;

    [FieldOffset(64)]
    private float Unknown40;

    [FieldOffset(68)]
    private float Unknown44;

    [FieldOffset(72)]
    private uint Unknown48;

    [FieldOffset(76)]
    private uint Unknown4C;

    [FieldOffset(80)]
    private uint Unknown50;

    [FieldOffset(88)]
    private float Unknown58;

    [FieldOffset(92)]
    private float Unknown5C;

    /// <summary>
    ///     是否正在起飞 (非零阻止特定地面移动转换)
    /// </summary>
    [FieldOffset(100)]
    public byte IsTakingOff;

    /// <summary>
    /// 是否正在飞行
    /// </summary>
    [FieldOffset(102)]
    public byte IsFlying;
    
    /// <summary>
    ///     飞行过渡起始位置 X
    /// </summary>
    [FieldOffset(112)]
    public long FlyTransitionStartPosX;

    /// <summary>
    ///     飞行过渡起始位置 Y
    /// </summary>
    [FieldOffset(120)]
    public long FlyTransitionStartPosY;

    /// <summary>
    ///     飞行过渡起始位置 Z
    /// </summary>
    [FieldOffset(128)]
    public long FlyTransitionStartPosZ;

    /// <summary>
    ///     起飞混合时长 0 (哨兵 -1.0f, 飞行过渡时长)
    /// </summary>
    [FieldOffset(136)]
    public float FlyBlendDuration0;

    /// <summary>
    ///     起飞混合时长 1 (哨兵 -1.0f)
    /// </summary>
    [FieldOffset(140)]
    public float FlyBlendDuration1;

    /// <summary>
    ///     起飞混合时长 2 (哨兵 -1.0f)
    /// </summary>
    [FieldOffset(144)]
    public float FlyBlendDuration2;

    /// <summary>
    ///     起飞动画混合时长 (哨兵 -1.0f)
    /// </summary>
    [FieldOffset(148)]
    public float FlyTakeoffBlendDuration;

    /// <summary>
    ///     降落动画混合时长 (哨兵 -1.0f)
    /// </summary>
    [FieldOffset(152)]
    public float FlyLandBlendDuration;

    /// <summary>
    ///     起飞阶段 (枚举: 0-2=空闲, 3-5=混合中)
    /// </summary>
    [FieldOffset(156)]
    public int FlyTakeoffPhase;

    /// <summary>
    ///     未知 QWORD
    /// </summary>
    [FieldOffset(160)]
    private long FlyUnkQword160;

    /// <summary>
    ///     降落阶段 (枚举: 0-2=空闲, 3-5=混合中)
    /// </summary>
    [FieldOffset(168)]
    public int FlyLandPhase;

    /// <summary>
    ///     飞行标志 A
    /// </summary>
    [FieldOffset(172)]
    public byte FlyFlagA;
}
