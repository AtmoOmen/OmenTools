using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class SetHouseBackgroundMusicCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.SetHouseBackgroundMusic;

    /// <summary>
    ///     设置房屋背景音乐
    /// </summary>
    public void Set(uint orchestrionRowID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, orchestrionRowID);
}