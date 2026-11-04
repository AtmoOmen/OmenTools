using OmenTools.Interop.Game.Models;

namespace OmenTools.Info.Game.Packets.Upstream;

/// <summary>
///     统一解析客户端上行数据包的操作码。
/// </summary>
public static class UpstreamOpcode
{
    /// <summary>
    ///     事件开始数据包的上行操作码。
    /// </summary>
    public static int EventStartOpcode { get; }

    /// <summary>
    ///     事件操作数据包的上行操作码。
    /// </summary>
    public static int EventActionOpcode { get; }

    /// <summary>
    ///     事件完成数据包的上行操作码。
    /// </summary>
    public static int EventCompleteOpcode { get; }

    /// <summary>
    ///     开始潜水数据包的上行操作码。
    /// </summary>
    public static int DiveStartOpcode { get; }

    /// <summary>
    ///     位置更新数据包的上行操作码。
    /// </summary>
    public static int PositionUpdateOpcode { get; }

    /// <summary>
    ///     副本位置更新数据包的上行操作码。
    /// </summary>
    public static int PositionUpdateInstanceOpcode { get; }

    /// <summary>
    ///     开启宝箱数据包的上行操作码。
    /// </summary>
    public static int TreasureOpenOpcode { get; }

    /// <summary>
    ///     心跳数据包的上行操作码。
    /// </summary>
    public static int HeartbeatOpcode { get; }

    /// <summary>
    ///     使用技能数据包的上行操作码。
    /// </summary>
    public static int UseActionOpcode { get; }

    /// <summary>
    ///     指定位置使用技能数据包的上行操作码。
    /// </summary>
    public static int UseActionLocationOpcode { get; }

    /// <summary>
    ///     无人岛交互数据包的上行操作码。
    /// </summary>
    public static int MJIInteractOpcode { get; }

    /// <summary>
    ///     执行指令数据包的上行操作码。
    /// </summary>
    public static int ExecuteCommandOpcode { get; }

    /// <summary>
    ///     打开冒险者铭牌数据包的上行操作码。
    /// </summary>
    public static int CharaCardOpenOpcode { get; }

    /// <summary>
    ///     交付物品数据包的上行操作码。
    /// </summary>
    public static int HandOverItemOpcode { get; }

    static UpstreamOpcode()
    {
        EventStartOpcode             = PacketOpcodeReader.Read("EventStart",             (EventStartOpcodeBaseSig, 0x4));
        EventActionOpcode            = PacketOpcodeReader.Read("EventAction",            (EventActionOpcodeBaseSig, 0x9E));
        EventCompleteOpcode          = PacketOpcodeReader.Read("EventComplete",          (EventCompleteOpcodeBaseSig, 0x117));
        DiveStartOpcode              = PacketOpcodeReader.Read("DiveStart",              (DiveStartOpcodeBaseSig, 0x4));
        PositionUpdateInstanceOpcode = PacketOpcodeReader.Read("PositionUpdateInstance", (PositionUpdateInstanceBaseSig, 0x4));
        TreasureOpenOpcode           = PacketOpcodeReader.Read("TreasureOpen",           (TreasureOpenOpcodeBaseSig, 0x4));
        HeartbeatOpcode              = PacketOpcodeReader.Read("Heartbeat",              (HeartbeatOpcodeBaseSig, 0x4));
        UseActionOpcode              = PacketOpcodeReader.Read("UseAction",              (UseActionOpcodeBaseSig, 0x4));
        UseActionLocationOpcode      = PacketOpcodeReader.Read("UseActionLocation",      (UseActionLocationOpcodeBaseSig, 0x4));
        MJIInteractOpcode            = PacketOpcodeReader.Read("MJIInteract",            (MJIInteractOpcodeBaseSig, 0x4));
        ExecuteCommandOpcode         = PacketOpcodeReader.Read("ExecuteCommand",         (ExecuteCommandOpcodeBaseSig, 0x6D));
        CharaCardOpenOpcode          = PacketOpcodeReader.Read("CharaCardOpen",          (CharaCardOpenOpcodeBaseSig, 0x4));
        HandOverItemOpcode           = PacketOpcodeReader.Read("HandOverItem",           (HandOverItemOpcodeBaseSig, 0xBD));
        PositionUpdateOpcode         = PacketOpcodeReader.Read("PositionUpdate",         (PositionUpdateBaseSig, 0x4));
    }

    #region 常量

    private static readonly CompSig EventStartOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 48 C7 44 24 ?? ?? ?? ?? ?? 89 5C 24 ?? 0F 85");

    private static readonly CompSig EventActionOpcodeBaseSig =
        new("E8 ?? ?? ?? ?? EB 3A 40 84 FF");

    private static readonly CompSig EventCompleteOpcodeBaseSig =
        new("E8 ?? ?? ?? ?? EB 10 48 8B 0D ?? ?? ?? ??");

    private static readonly CompSig DiveStartOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 48 C7 44 24 ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? B0");

    private static readonly CompSig PositionUpdateBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? F7 D9");

    private static readonly CompSig PositionUpdateInstanceBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 48 8D 54 24 ?? 48 C7 44 24 ?? ?? ?? ?? ?? 0F 11 44 24");

    private static readonly CompSig TreasureOpenOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 48 C7 44 24 ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 ?? 45 33 C9");

    private static readonly CompSig HeartbeatOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 48 F7 F1");

    private static readonly CompSig UseActionOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 45 33 C0 48 C7 44 24 ?? ?? ?? ?? ?? 89 7C 24");

    private static readonly CompSig UseActionLocationOpcodeBaseSig =
        new("C7 44 24 ?? ?? ?? ?? ?? 48 C7 44 24 ?? ?? ?? ?? ?? 89 74 24 ?? 40 88 6C 24");

    private static readonly CompSig MJIInteractOpcodeBaseSig =
        new
        (
            "C7 44 24 ?? ?? ?? ?? ?? 48 C7 44 24 ?? ?? ?? ?? ?? 0F 11 44 24 ?? E8 ?? ?? ?? ?? 48 8B AC 24 ?? ?? ?? ?? 48 8B 8C 24 ?? ?? ?? ?? 48 33 CC E8 ?? ?? ?? ?? 48 81 C4 ?? ?? ?? ?? 41 5F 41 5E 41 5D 41 5C 5F 5E 5B C3 CC CC CC CC CC CC CC CC CC CC CC CC 40 53"
        );

    private static readonly CompSig ExecuteCommandOpcodeBaseSig =
        new("E8 ?? ?? ?? ?? FE 43 18");

    private static readonly CompSig CharaCardOpenOpcodeBaseSig =
        new
        (
            "C7 44 24 ?? ?? ?? ?? ?? 45 33 C0 48 C7 44 24 ?? ?? ?? ?? ?? C7 44 24 ?? ?? ?? ?? ?? C6 44 24 ?? ?? E8 ?? ?? ?? ?? 48 8B 8C 24 ?? ?? ?? ?? 48 33 CC E8 ?? ?? ?? ?? 48 81 C4 ?? ?? ?? ?? C3"
        );

    private static readonly CompSig HandOverItemOpcodeBaseSig =
        new("E8 ?? ?? ?? ?? EB 3A 40 84 FF");

    #endregion
}
