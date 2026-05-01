using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class MJIFarmEntrustSingleCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.MJIFarmEntrustSingle;

    /// <summary>
    ///     托管单块无人岛耕地
    /// </summary>
    public void Entrust(uint farmIndex, uint seedItemID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, farmIndex, seedItemID);
}
