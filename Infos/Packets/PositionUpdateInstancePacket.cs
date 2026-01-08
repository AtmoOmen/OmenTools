using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 56)]
public struct PositionUpdateInstancePacket(
    float                                 rotation,
    Vector3                               position,
    PositionUpdateInstancePacket.MoveType moveType = PositionUpdateInstancePacket.MoveType.NormalMove0) : IGamePacket
{
    [FieldOffset(0)]  public int      Opcode      = GamePacketOpcodes.PositionUpdateInstanceOpcode;
    [FieldOffset(8)]  public uint     Length      = 56;
    [FieldOffset(32)] public float    Rotation    = rotation;
    [FieldOffset(36)] public float    RotationNew = rotation;
    [FieldOffset(40)] public MoveType Move        = moveType;
    [FieldOffset(44)] public Vector3  Position    = position;
    [FieldOffset(56)] public Vector3  PositionNew = position;

    public string Log()
        => $"Position Update Instance 包体 ({Opcode} / 长度: {Length})\n" +
           $"旧面向: {Rotation} | 新面向: {RotationNew}\n"                  +
           $"位置: {Position:F2} | 新位置: {PositionNew:F2}\n"             +
           $"移动类型: {Move}";

    public void Send() => GamePacketManager.Instance().SendPackt(this);
    
    private static readonly ConcurrentDictionary<string, MoveType[]> MoveTypeCache = new();
    
    public static void Send(float rotation, Vector3 position, MoveType moveType = MoveType.NormalMove0)
    {
        var moveStr = moveType.ToString();
        var name    = moveStr[..^1];
        var allMoveTypes = MoveTypeCache.GetOrAdd(name, key =>
                                                      Enum.GetValues<MoveType>()
                                                          .Where(x => x.ToString().Contains(key))
                                                          .ToArray());
        
        foreach (var move in allMoveTypes)
            new PositionUpdateInstancePacket(rotation, position, move).Send();
    }

    public enum MoveType : uint
    {
        NormalMove0       = 0,
        NormalMove1       = 0x10000,
        NormalMove2       = 0x20000,
        NormalMove3       = 0x30000,
        ActionMove0       = 0x200000,
        Fly0              = 1,
        Fly1              = 0x10001,
        Fly2              = 0x20001,
        Fly3              = 0x30001,
        WalkOrSlowSwim0   = 2,
        WalkOrSlowSwim1   = 0x10002,
        WalkOrSlowSwim2   = 0x20002,
        WalkOrSlowSwim3   = 0x30002,
        SlowFly0          = 3,
        SlowFly1          = 0x10003,
        SlowFly2          = 0x20003,
        SlowFly3          = 0x30003,
        ActionMoveEnd0    = 0x1000,
        SmallMove0        = 0x404000,
        SmallMove1        = 0x414000,
        SmallMove2        = 0x424000,
        SmallMove3        = 0x434000,
        SmallFlight0      = 0x404001,
        SmallFlight1      = 0x414001,
        SmallFlight2      = 0x424001,
        SmallFlight3      = 0x434001,
        Falling0          = 0x100000,
        Falling1          = 0x110000,
        Falling2          = 0x120000,
        Falling3          = 0x130000,
        JumpStart0        = 0x400100,
        JumpStart1        = 0x410100,
        JumpStart2        = 0x420100,
        JumpStart3        = 0x430100,
        JumpProcess0      = 0x504000,
        JumpProcess1      = 0x514000,
        JumpProcess2      = 0x524000,
        JumpProcess3      = 0x534000,
        JumpHighestPoint0 = 0x510400,
        JumpEnd0          = 0x400200,
        JumpEnd1          = 0x410200,
        JumpEnd2          = 0x420200,
        JumpEnd3          = 0x430200,
    }
}
