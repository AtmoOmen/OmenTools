using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct UseActionLocationPacket(ActionType type, uint actionID, float rotation, Vector3 location) : IGamePacket
{
    [FieldOffset(0)]  public        int     Opcode   = GamePacketOpcodes.UseActionLocationOpcode;
    [FieldOffset(8)]  public        uint    Length   = 48;
    [FieldOffset(32)] public        uint    ActionID = actionID;
    [FieldOffset(36)] public        byte    CastBuff;
    [FieldOffset(37)] public        byte    ActionType             = (byte)type;
    [FieldOffset(38)] public unsafe ushort  LastUsedActionSequence = (ushort)(ActionManager.Instance()->LastUsedActionSequence + 1);
    [FieldOffset(40)] public        ushort  Rotation               = CharaRotationToPacketRotation(rotation);
    [FieldOffset(42)] public        ushort  RotationNew;
    [FieldOffset(44)] public        Vector3 Location               = location;

    public string Log() => $"Use Action Location 包体 ({Opcode} / 长度: {Length})\n" +
                           $"Type: {(ActionType)ActionType} | ID: {ActionID}\n" +
                           $"Rotation: {Rotation} ({PacketRotationToCharaRotation(Rotation):F2}) | Rotation (New): {RotationNew} ({PacketRotationToCharaRotation(RotationNew):F2})\n" +
                           $"Location: {Location:F2}\n" +
                           $"Cast Buff: {CastBuff} | Action Sequence: {LastUsedActionSequence}";
    
    public void Send() => GamePacketManager.SendPackt(this);
}
