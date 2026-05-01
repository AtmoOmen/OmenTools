using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ApplyGlamourPlateCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.ApplyGlamourPlate;

    /// <summary>
    ///     应用投影模板
    /// </summary>
    public void Apply(uint plateIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, plateIndex);
}
