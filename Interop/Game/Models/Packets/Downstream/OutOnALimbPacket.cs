using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Network;
using OmenTools.Interop.Game.Models.Packets.Abstractions;

namespace OmenTools.Interop.Game.Models.Packets.Downstream;

/// <summary>
///     金碟游戏机“孤树无援”游玩结果
///     <seealso cref="PacketDispatcher.Delegates.HandleEventYieldPacket" />
/// </summary>
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
        $"孤树无援游玩结果包体\n" +
        $"游玩结果: {Result} | 体力: {Health} | 奖励等级: {BonusLevel}";
}

public enum OutOnALimbResult : uint
{
    Fail    = 0,
    Normal  = 1,
    Great   = 2,
    Perfect = 3,
    Start   = 10
}
