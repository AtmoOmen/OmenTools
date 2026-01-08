using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 52)]
public struct EventActionPacket(uint eventID, uint category, uint param0 = 0, uint param1 = 0, uint param2 = 0) : IGamePacket
{
    [FieldOffset(0)]  public int  Opcode   = GamePacketOpcodes.EventActionOpcode;
    [FieldOffset(8)]  public uint Length   = 32;
    [FieldOffset(32)] public uint EventID  = eventID;
    [FieldOffset(36)] public uint Category = category;
    [FieldOffset(40)] public uint Param0   = param0;
    [FieldOffset(44)] public uint Param1   = param1;
    [FieldOffset(48)] public uint Param2   = param2;

    public string Log() =>
        $"Event Action 包体 ({Opcode} / 长度: {Length})\n" +
        $"Event ID: {EventID} | Category: {Category} \n" +
        $"Param0: {Param0} | Param1: {Param1} | Param2: {Param2}";
    
    public void Send() => GamePacketManager.Instance().SendPackt(this);
}
