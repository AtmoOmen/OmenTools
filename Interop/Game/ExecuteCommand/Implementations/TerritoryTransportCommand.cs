using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class TerritoryTransportCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.TerritoryTransport;

    /// <summary>
    ///     执行区域变更
    /// </summary>
    public void Transport(TransportKind transportKind, InnerTransportKind innerTransportKind = 0) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)transportKind, (uint)innerTransportKind);

    public enum TransportKind : uint
    {
        NPC = 1,
        Boundary = 3,
        Teleport = 4,
        Return = 7,
        Aethernet = 15,
        Housing = 20
    }

    public enum InnerTransportKind : uint
    {
        None = 0,
        Cutscene = 1,
        ReturnToSafeArea = 2,
        DutyTransport = 25,
        Diving = 26
    }
}