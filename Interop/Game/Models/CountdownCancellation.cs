using OmenTools.Info.Models;

namespace OmenTools.Interop.Game.Models;

/// <summary>
///     战斗倒计时取消时的托管数据快照。
/// </summary>
public sealed record CountdownCancellation
{
    /// <summary>
    ///     被取消的倒计时数据。
    /// </summary>
    public required CountdownInfo Countdown { get; init; }

    /// <summary>
    ///     取消者的实体 ID。
    /// </summary>
    public uint EntityID { get; init; }

    /// <summary>
    ///     取消者的原始服务器 ID。
    /// </summary>
    public ushort HomeWorldID { get; init; }

    /// <summary>
    ///     原生取消函数接收到的取消者名称，系统清理时为空字符串。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     是否为系统发起的无取消者清理，此时原生函数不发送取消消息。
    /// </summary>
    public bool IsSystemCancellation { get; init; }
}
