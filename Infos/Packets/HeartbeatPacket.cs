﻿using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct HeartbeatPacket() : IGamePacket
{
    [FieldOffset(0)] public int  Opcode = GamePacketOpcodes.HeartbeatOpcode;
    [FieldOffset(8)] public uint Length = 48;
    
    public string Log() => $"Heartbeat 包体 ({Opcode} / 长度: {Length})";

    public void Send() => GamePacketManager.SendPackt(this);
}
