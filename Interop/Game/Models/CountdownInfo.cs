namespace OmenTools.Interop.Game.Models;

/// <summary>
///     战斗倒计时的托管数据快照，不持有游戏内存指针。
/// </summary>
public sealed record CountdownInfo
{
    /// <summary>
    ///     发起者的账号 ID。
    /// </summary>
    public ulong AccountID { get; init; }

    /// <summary>
    ///     发起者的角色内容 ID。
    /// </summary>
    public ulong ContentID { get; init; }

    /// <summary>
    ///     发起者的实体 ID。
    /// </summary>
    public uint EntityID { get; init; }

    /// <summary>
    ///     发起者的原始服务器 ID。
    /// </summary>
    public ushort HomeWorldID { get; init; }

    /// <summary>
    ///     服务器数据中的发起者名称。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     初始倒计时持续时间，单位为秒。
    /// </summary>
    public ushort DurationSeconds { get; init; }

    /// <summary>
    ///     服务器是否因战斗状态拒绝开始倒计时。
    /// </summary>
    public bool IsCombatBlocked { get; init; }

    /// <summary>
    ///     是否由文本指令发起。
    /// </summary>
    public bool IsTextCommand { get; init; }

    /// <summary>
    ///     是否由任务记录准备确认发起。
    /// </summary>
    public bool IsRecordReadyCheck { get; init; }
}
