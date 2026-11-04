using System.Runtime.InteropServices;
using OmenTools.Info.Game.Packets.Abstractions;

namespace OmenTools.Info.Game.Packets.Upstream;

/// <summary>
///     客户端心跳发送缓冲区，包含发送描述头和完整的 32 字节包体。
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct HeartbeatPacket() : IUpstreamPacket
{
    /// <summary>
    ///     心跳包的上行操作码。
    /// </summary>
    [FieldOffset(0)]
    public int Opcode = UpstreamOpcode.HeartbeatOpcode;

    /// <summary>
    ///     IPC 包头与包体的总长度，不包含发送描述头额外占用的 16 字节。
    /// </summary>
    [FieldOffset(8)]
    public uint Length = 48;

    /// <summary>
    ///     客户端单调时钟毫秒数的低 32 位，由服务器原样返回。
    /// </summary>
    [FieldOffset(32)]
    public uint ClientTime;

    /// <summary>
    ///     客户端上一次计算的往返耗时，单位为毫秒。
    /// </summary>
    [FieldOffset(36)]
    public uint RoundTripTime;

    /// <summary>
    ///     从本地网络模块读取的客户端心跳状态数据。
    /// </summary>
    [FieldOffset(44)]
    public Guid ClientData;

    /// <summary>
    ///     获取心跳包的日志描述。
    /// </summary>
    public string Log() =>
        $"Heartbeat 包体 ({Opcode} / 长度: {Length})\n" +
        $"客户端时间戳: {ClientTime} / 上次往返耗时: {RoundTripTime} 毫秒 / 客户端状态: {ClientData}";

    /// <summary>
    ///     将心跳发送缓冲区提交到游戏的区域服务器连接。
    /// </summary>
    public void Send() => GamePacketManager.Instance().SendPackt(this);
}
