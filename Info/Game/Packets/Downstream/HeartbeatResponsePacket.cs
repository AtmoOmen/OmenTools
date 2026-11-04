using System.Runtime.InteropServices;
using OmenTools.Info.Game.Packets.Abstractions;

namespace OmenTools.Info.Game.Packets.Downstream;

/// <summary>
///     游戏服务器返回的心跳包体，不包含 IPC 包头。
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct HeartbeatResponsePacket : IDownstreamPacket
{
    /// <summary>
    ///     客户端发送心跳时的单调时钟毫秒数低 32 位，用于计算本次往返耗时。
    /// </summary>
    [FieldOffset(0)]
    public uint ClientTime;

    /// <summary>
    ///     心跳包携带的往返耗时字段，单位为毫秒。
    /// </summary>
    [FieldOffset(4)]
    public uint RoundTripTime;

    /// <summary>
    ///     获取心跳回包的客户端时间戳与往返耗时字段日志。
    /// </summary>
    public string Log() =>
        $"Heartbeat 回包 ({DownstreamOpcode.HeartbeatOpcode})\n" +
        $"客户端时间戳: {ClientTime} / 往返耗时字段: {RoundTripTime} 毫秒";
}
