using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit)]
public struct OutOnALimbPacket : IDownstreamPacket
{
    [FieldOffset(4)]
    public OutOnALimbResult Result;

    [FieldOffset(8)]
    public uint Health;

    [FieldOffset(20)]
    public byte BonusLevel;

    public string Log() =>
        $"Out On A Limb 包体\n" +
        $"游玩结果: {Result} | 体力: {Health} | 奖励等级: {BonusLevel}";
    
    
    public static CompSig Signature { get; } = 
        new("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 49 8B D9 41 0F B6 F8 0F B7 F2 8B E9 E8 ?? ?? ?? ?? 44 0F B6 54 24 ?? 44 0F B6 CF 44 88 54 24 ?? 44 0F B7 C6 8B D5");

    public unsafe delegate void* Delegate(uint eventID, ushort category, ushort stage, OutOnALimbPacket* packet, byte a5);
}

public enum OutOnALimbResult : uint
{
    Fail    = 0,
    Normal  = 1,
    Great   = 2,
    Perfect = 3,
    Start   = 10
}
