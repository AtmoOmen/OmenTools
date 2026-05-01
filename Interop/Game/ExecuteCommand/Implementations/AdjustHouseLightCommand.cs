using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class AdjustHouseLightCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.AdjustHouseLight;

    /// <summary>
    ///     调整房间亮度
    /// </summary>
    public void Adjust(uint lightLevel) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, lightLevel);
}