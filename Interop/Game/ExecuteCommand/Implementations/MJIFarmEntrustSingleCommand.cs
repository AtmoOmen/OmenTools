using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIFarmEntrustSingleCommand : ExecuteCommandBase
{
    /// <summary>
    ///     托管单块无人岛耕地
    /// </summary>
    public static void Entrust(uint farmIndex, uint seedItemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.MJIFarmEntrustSingle, farmIndex, seedItemID);
}
