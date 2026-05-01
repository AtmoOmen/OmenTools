using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FateMobSpawnCommand : ExecuteCommandBase
{
    /// <summary>
    ///     发送临危受命野怪生成命令
    /// </summary>
    public static void Spawn(uint objectID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FateMobSpawn, objectID);
}
