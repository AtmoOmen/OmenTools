using OmenTools.Info.Game.Enums;
using OmenTools.Interop.Game.ExecuteCommand.Abstractions;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.ExecuteCommand.Implementations;

public sealed class OrchestrionCommand : ExecuteCommandBase
{
    /// <summary>
    ///     设置管弦乐琴播放列表
    /// </summary>
    public static void SetPlaylist(uint playlistID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.SetOrchestrionPlaylist, playlistID);

    /// <summary>
    ///     管弦乐琴播放或停止切换
    /// </summary>
    public static void TogglePlay() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.ToggleOrchestrion);

    /// <summary>
    ///     管弦乐琴下一曲或音量调整
    /// </summary>
    public static void NextTrack() =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.PlayNextOrchestrionTrack);

    /// <summary>
    ///     旅馆内播放管弦乐琴乐谱
    /// </summary>
    public static void PlayTrack(uint orchestrionID) =>
        ExecuteCommandManager.Instance().ExecuteCommand(ExecuteCommandFlag.PlayOrchestrionTrack, orchestrionID);
}
