using FFXIVClientStructs.FFXIV.Client.Game.Object;
using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class StatusOffCommand : ExecuteCommandBase
{
    /// <summary>
    ///     取消任意来源的首个指定状态效果
    /// </summary>
    public static void Remove(uint statusID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.StatusOff, statusID, 0, 0xE0000000);

    /// <summary>
    ///     取消指定来源的状态效果
    /// </summary>
    public static void Remove(uint statusID, GameObjectId sourceGameObjectID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.StatusOff, statusID, 0, (uint)sourceGameObjectID);
}
