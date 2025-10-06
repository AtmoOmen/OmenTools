﻿using System.Numerics;
using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct DiveStartPacket(Vector3 pos, float rotation) : IGamePacket
{
    [FieldOffset(0)]  public int     Opcode   = GamePacketOpcodes.DiveStartOpcode;
    [FieldOffset(8)]  public uint    Length   = 48;
    [FieldOffset(32)] public float   Rotation = rotation;
    [FieldOffset(36)] public Vector3 Position = pos;

    public string Log() => $"Dive Start 包体 ({Opcode} / 长度: {Length})\n" +
                           $"面向: {Rotation} | 位置: {Position:F2}";

    public void Send() => GamePacketManager.SendPackt(this);
}
