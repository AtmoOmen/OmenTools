using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace OmenTools.Infos;

// TODO: 偏移了
[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct UseActionPacket(ActionType type, uint actionID, ulong targetID, float rotation) : IGamePacket
{
    [FieldOffset(0)]  public        int    Opcode   = GamePacketOpcodes.UseActionOpcode;
    [FieldOffset(8)]  public        uint   Length   = 48;
    [FieldOffset(32)] public        uint   ActionID = actionID;
    [FieldOffset(36)] public        byte   CastBuff;
    [FieldOffset(37)] public        byte   ActionType             = (byte)type;
    [FieldOffset(38)] public unsafe ushort LastUsedActionSequence = (ushort)(ActionManager.Instance()->LastUsedActionSequence + 1);
    [FieldOffset(40)] public        ushort Rotation               = CharaRotationToPacketRotation(rotation);
    [FieldOffset(42)] public        ushort RotationNew            = CharaRotationToPacketRotation(rotation);
    [FieldOffset(44)] public        ushort Charge;
    [FieldOffset(46)] public        ushort ExtraParam;
    [FieldOffset(48)] public        ulong  TargetID               = targetID;

    public string Log() => $"Use Action 包体 ({Opcode} / 长度: {Length})\n" +
                           $"Type: {(ActionType)ActionType} | ID: {ActionID} | Target ID: {TargetID}\n" +
                           $"Rotation: {Rotation} ({PacketRotationToCharaRotation(Rotation):F2}) | Rotation (New): {RotationNew} ({PacketRotationToCharaRotation(RotationNew):F2})\n" +
                           $"Cast Buff: {CastBuff} | Action Sequence: {LastUsedActionSequence}\n" +
                           $"Charge: {Charge} | Extra Param: {ExtraParam}";
    
    public void Send() => GamePacketManager.SendPackt(this);
}
