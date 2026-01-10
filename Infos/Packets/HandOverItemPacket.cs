using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 56)]
public struct HandOverItemPacket(uint eventID, uint category = 0, uint itemID = 0, uint itemCount = 0, uint leveID = 0) : IGamePacket
{
    [FieldOffset(0)] public int  Opcode = GamePacketOpcodes.HandOverItemOpcode;
    [FieldOffset(8)] public uint Length = 40;
    
    [FieldOffset(32)] public uint EventID   = eventID;
    [FieldOffset(36)] public uint Category  = category;
    [FieldOffset(40)] public uint ItemID    = itemID;
    [FieldOffset(44)] public uint ItemCount = itemCount;
    [FieldOffset(48)] public uint Param0    = 1;
    [FieldOffset(52)] public uint LeveID    = leveID;

    public string Log() => $"Hand Over Item 包体 ({Opcode} / 长度: {Length})\n" +
                           $"Event ID: {EventID} / Category: {Category}\n"  +
                           $"Item ID: {ItemID} / Item Count: {ItemCount}\n" +
                           $"Param0: {Param0} / Leve ID: {LeveID}";

    public void Send() => GamePacketManager.Instance().SendPackt(this);
}
