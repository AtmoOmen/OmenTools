using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct UseActionLocationPacket(ActionType type, uint actionID, float rotation, Vector3 location) : IGamePacket
{
    [FieldOffset(0)]  public        int        Opcode = GamePacketOpcodes.UseActionLocationOpcode;
    [FieldOffset(8)]  public        uint       Length = 48;
    [FieldOffset(32)] public        byte       CastBuff;
    [FieldOffset(33)] public        ActionType Type                   = type;
    [FieldOffset(36)] public        uint       ID                     = actionID;
    [FieldOffset(40)] public unsafe ushort     LastUsedActionSequence = (ushort)(ActionManager.Instance()->LastUsedActionSequence + 1);
    [FieldOffset(42)] public        ushort     Rotation               = CharaRotationToPacketRotation(rotation);
    [FieldOffset(44)] public        ushort     RotationNew;
    [FieldOffset(46)] public        ushort     Charge;
    [FieldOffset(48)] public        Vector3    Location               = location;

    public string Log() => $"Use Action Location 包体 ({Opcode} / 长度: {Length})\n" +
                           $"Type: {Type} | ID: {ID}\n" +
                           $"Rotation: {Rotation} ({PacketRotationToCharaRotation(Rotation):F2}) | Rotation (New): {RotationNew} ({PacketRotationToCharaRotation(RotationNew):F2})\n" +
                           $"Location: {Location:F2} | Charge: {Charge}\n" +
                           $"Cast Buff: {CastBuff} | Action Sequence: {LastUsedActionSequence}";
    
    public void Send() => GamePacketManager.SendPackt(this);
}
