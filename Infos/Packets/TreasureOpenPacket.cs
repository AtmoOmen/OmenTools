using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 44)]
public struct TreasureOpenPacket(ulong gameObjectID) : IGamePacket
{
    [FieldOffset(0)]  public int   Opcode       = GamePacketOpcodes.TreasureOpenOpcode;
    [FieldOffset(8)]  public uint  Length       = 24;
    [FieldOffset(32)] public ulong GameObjectID = gameObjectID;

    public string Log()
        => $"Treasure Open 包体 ({Opcode} / 长度: {Length})\n" +
           $"Game Object ID: {GameObjectID}";

    public void Send() => GamePacketManager.SendPackt(this);
}
