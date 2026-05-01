using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class CollectTrophyCrystalCommand : ExecuteCommandBase
{
    public override ExecuteCommandFlag Flag => ExecuteCommandFlag.CollectTrophyCrystal;

    /// <summary>
    ///     领取战利水晶
    /// </summary>
    public void Collect(Season season = Season.Current) =>
        ExecuteCommandManager.Instance().ExecuteCommand(Flag, (uint)season);

    public enum Season : uint
    {
        Current  = 0,
        Previous = 1
    }
}
