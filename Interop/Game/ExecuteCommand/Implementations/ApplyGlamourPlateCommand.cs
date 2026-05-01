using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class ApplyGlamourPlateCommand : ExecuteCommandBase
{
    /// <summary>
    ///     应用投影模板
    /// </summary>
    public static void Apply(uint plateIndex) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ApplyGlamourPlate, plateIndex);
}
