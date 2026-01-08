using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct ExecuteCommandPacket(ExecuteCommandFlag flag, int param1 = 0, int param2 = 0, int param3 = 0, int param4 = 0) : IGamePacket
{
    [FieldOffset(0)]  public int                Opcode = GamePacketOpcodes.ExecuteCommandOpcode;
    [FieldOffset(8)]  public uint               Length = 48;
    [FieldOffset(32)] public ExecuteCommandFlag Flag   = flag;
    [FieldOffset(36)] public int                Param1 = param1;
    [FieldOffset(40)] public int                Param2 = param2;
    [FieldOffset(44)] public int                Param3 = param3;
    [FieldOffset(48)] public int                Param4 = param4;

    public string Log() =>
        $"Execute Command Packet 包体 ({Opcode} / 长度: {Length})\n" +
        $"Flag: {Flag}\n"                                       +
        $"P1: {Param1} | P2: {Param2} | P3: {Param3} | P4: {Param4}";

    public void Send() => GamePacketManager.Instance().SendPackt(this);
}
