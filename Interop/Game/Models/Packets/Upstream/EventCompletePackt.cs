using System.Runtime.InteropServices;
using OmenTools.Interop.Game.Models.Packets.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.Models.Packets.Upstream;

[StructLayout(LayoutKind.Explicit, Size = 52)]
public struct EventCompletePackt
(
    uint eventID,
    uint category,
    uint param0 = 0,
    uint param1 = 0,
    uint param2 = 0
)
    : IUpstreamPacket
{
    [FieldOffset(0)]
    public int Opcode = UpstreamOpcode.EventCompleteOpcode;

    [FieldOffset(8)]
    public uint Length = 32;

    [FieldOffset(32)]
    public uint EventID = eventID;

    [FieldOffset(36)]
    public uint Category = category;

    [FieldOffset(40)]
    public uint Param0 = param0;

    [FieldOffset(44)]
    public uint Param1 = param1;

    [FieldOffset(48)]
    public uint Param2 = param2;

    public string Log() =>
        $"Event Complete 包体 ({Opcode} / 长度: {Length})\n" +
        $"Event ID: {EventID} | Category: {Category} \n" +
        $"Param0: {Param0} | Param1: {Param1} | Param2: {Param2}";

    public void Send() => GamePacketManager.Instance().SendPackt(this);
}
