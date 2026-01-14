using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 52)]
public struct EventStartPackt(ulong objectID, uint eventID, uint category = 0, uint param = 0) : IUpstreamPacket
{
    [FieldOffset(0)]  public int    Opcode        = UpstreamOpcode.EventStartOpcode;
    [FieldOffset(8)]  public uint   Length        = 32;
    [FieldOffset(32)] public ulong  EventObjectID = objectID;
    [FieldOffset(40)] public uint   EventID       = eventID;
    [FieldOffset(44)] public uint   Category      = category;
    [FieldOffset(48)] public uint   Param         = param;
    [FieldOffset(40)] public ushort EntryID;

    public string Log() =>
        $"Event Start 包体 ({Opcode} / 长度: {Length})\n"                                     +
        $"Event Object ID: {EventObjectID} | Event ID: {EventID} | Entry ID: {EntryID}\n" +
        $"Category: {Category} | Param: {Param}";

    public void Send() => GamePacketManager.Instance().SendPackt(this);
}
