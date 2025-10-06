using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 40)]
public struct MJIInteractPacket(uint layoutID, uint dataID = 2012985) : IGamePacket
{
    [FieldOffset(0)]  public int  Opcode   = GamePacketOpcodes.HeartbeatOpcode;
    [FieldOffset(8)]  public uint Length   = 40;
    [FieldOffset(32)] public uint LayoutID = layoutID;
    [FieldOffset(36)] public uint DataID   = dataID;

    public string Log() => $"MJIInteractPacket 包体 ({Opcode} / 长度: {Length})\n" +
                           $"Layout ID: {LayoutID} | Data ID: {DataID}";

    public void Send() => GamePacketManager.SendPackt(this);
}
