using OmenTools.Interop.Game.Models;

namespace OmenTools.Info.Game.Packets.Downstream;

/// <summary>
///     统一解析客户端下行数据包的操作码。
/// </summary>
public static class DownstreamOpcode
{
    /// <summary>
    ///     从国际服或国服客户端心跳处理逻辑中解析的下行心跳操作码。
    /// </summary>
    public static int HeartbeatOpcode { get; }

    static DownstreamOpcode() =>
        HeartbeatOpcode = PacketOpcodeReader.Read
        (
            "Heartbeat",
            (HeartbeatOpcodeInternationalBaseSig, 2),
            (HeartbeatOpcodeChinaBaseSig, 1)
        );

    #region 常量

    private static readonly CompSig HeartbeatOpcodeInternationalBaseSig =
        new("41 BC ?? ?? ?? ?? 49 8D 8E");

    private static readonly CompSig HeartbeatOpcodeChinaBaseSig =
        new("2D ?? ?? ?? ?? 74 ?? 83 F8 ?? 0F 85 ?? ?? ?? ?? 49 8B 8E");

    #endregion
}
