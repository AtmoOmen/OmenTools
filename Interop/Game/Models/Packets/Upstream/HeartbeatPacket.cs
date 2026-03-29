using System.Runtime.InteropServices;
using OmenTools.Interop.Game.Models.Packets.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.Models.Packets.Upstream;

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct HeartbeatPacket() : IUpstreamPacket
{
    [FieldOffset(0)]
    public int Opcode = UpstreamOpcode.HeartbeatOpcode;

    [FieldOffset(8)]
    public uint Length = 48;

    public string Log() => $"Heartbeat 包体 ({Opcode} / 长度: {Length})";

    public void Send() => GamePacketManager.Instance().SendPackt(this);
}
