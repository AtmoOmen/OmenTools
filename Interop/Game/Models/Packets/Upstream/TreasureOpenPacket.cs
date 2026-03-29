using System.Runtime.InteropServices;
using OmenTools.Interop.Game.Models.Packets.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.Models.Packets.Upstream;

[StructLayout(LayoutKind.Explicit, Size = 44)]
public struct TreasureOpenPacket
(
    ulong gameObjectID
) : IUpstreamPacket
{
    [FieldOffset(0)]
    public int Opcode = UpstreamOpcode.TreasureOpenOpcode;

    [FieldOffset(8)]
    public uint Length = 24;

    [FieldOffset(32)]
    public ulong GameObjectID = gameObjectID;

    public string Log()
        => $"Treasure Open 包体 ({Opcode} / 长度: {Length})\n" +
           $"Game Object ID: {GameObjectID}";

    public void Send() => GamePacketManager.Instance().SendPackt(this);
}
