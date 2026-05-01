using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class FateCommand : ExecuteCommandBase
{
    /// <summary>
    ///     进入临危受命范围
    /// </summary>
    public static void Enter(uint fateID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FateEnter, fateID);
    
    /// <summary>
    ///     为临危受命设置等级同步状态
    /// </summary>
    public static void SyncLevel(uint fateID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FateLevelSync, fateID);
    
    /// <summary>
    ///     临危受命退出等级同步状态
    /// </summary>
    public static void UnsyncLevel(uint fateID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FateLevelSync, fateID);
    
    /// <summary>
    ///     加载临危受命信息
    /// </summary>
    public static void Load(uint fateID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FateLoad, fateID);
    
    /// <summary>
    ///     临危受命怪物生成
    /// </summary>
    public static void Spawn(uint entityID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FateMobSpawn, entityID);
    
    /// <summary>
    ///     开始指定的临危受命任务
    /// </summary>
    public static void Start(uint fateID, uint targetObjectID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.FateStart, fateID, targetObjectID);
}
