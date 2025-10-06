using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct UseActionPacket(ActionType type, uint actionID, ulong targetID, float rotation) : IGamePacket
{
    [FieldOffset(0)]  public        int        Opcode = GamePacketOpcodes.UseActionOpcode;
    [FieldOffset(8)]  public        uint       Length = 48;
    [FieldOffset(32)] public        byte       CastBuff;
    [FieldOffset(33)] public        ActionType Type                   = type;
    [FieldOffset(36)] public        uint       ID                     = actionID;
    [FieldOffset(40)] public unsafe ushort     LastUsedActionSequence = (ushort)(ActionManager.Instance()->LastUsedActionSequence + 1);
    [FieldOffset(42)] public        ushort     Rotation               = CharaRotationToPacketRotation(rotation);
    [FieldOffset(44)] public        ushort     RotationNew            = CharaRotationToPacketRotation(rotation);
    [FieldOffset(46)] public        ushort     Charge;
    [FieldOffset(48)] public        ulong      TargetID               = targetID;

    public string Log() => $"Use Action 包体 ({Opcode} / 长度: {Length})\n" +
                           $"Type: {Type} | ID: {ID} | Target ID: {TargetID}\n" +
                           $"Rotation: {Rotation} ({PacketRotationToCharaRotation(Rotation):F2}) | Rotation (New): {RotationNew} ({PacketRotationToCharaRotation(RotationNew):F2})\n" +
                           $"Cast Buff: {CastBuff} | Action Sequence: {LastUsedActionSequence} | Charge: {Charge}";
    
    public void Send() => GamePacketManager.SendPackt(this);
}
