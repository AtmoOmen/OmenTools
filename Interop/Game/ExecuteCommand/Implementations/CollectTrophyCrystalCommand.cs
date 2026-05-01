using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class CollectTrophyCrystalCommand : ExecuteCommandBase
{
    /// <summary>
    ///     领取战利水晶
    /// </summary>
    public static void Collect(Season season = Season.Current) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.CollectTrophyCrystal, (uint)season);

    public enum Season : uint
    {
        Current  = 0,
        Previous = 1
    }
}
