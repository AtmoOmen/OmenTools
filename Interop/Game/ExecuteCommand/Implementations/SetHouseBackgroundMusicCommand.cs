using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SetHouseBackgroundMusicCommand : ExecuteCommandBase
{
    /// <summary>
    ///     设置房屋背景音乐
    /// </summary>
    public static void Set(uint orchestrionRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SetHouseBackgroundMusic, orchestrionRowID);
}
