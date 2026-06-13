namespace OmenTools.OmenService.ImGuiZoneObject;

/// <summary>
///     区域物体标记的句柄, 用于取消注册或更新文字与自定义绘制
///     轻量值类型, 仅持有全局唯一 ID, 对已失效的句柄操作会静默忽略
/// </summary>
public readonly struct ZoneIndicatorHandle
{
    internal ulong ID { get; init; }

    /// <summary>
    ///     句柄是否有效 (仅表示曾被分配, 不代表当前仍处于注册状态)
    /// </summary>
    public bool IsValid =>
        ID != 0;

    /// <summary>
    ///     取消注册该标记, 句柄已失效或服务不可用时返回 false
    /// </summary>
    public bool Unreg() =>
        IsValid && (Service?.UnregisterByID(ID) ?? false);

    /// <summary>
    ///     就地更新该标记的可变内容, 通过 <paramref name="mutator" /> 修改任意内容字段
    ///     仅修改 <paramref name="mutator" /> 实际触及的字段, 其余保持不变
    ///     句柄已失效或服务不可用时返回 false
    /// </summary>
    public bool Update(Action<IZoneIndicatorMutable> mutator)
    {
        ArgumentNullException.ThrowIfNull(mutator);
        return IsValid && (Service?.UpdateByID(ID, mutator) ?? false);
    }

    private static ZoneIndicatorRenderer? Service =>
        DService.Instance().GetOmenService<ZoneIndicatorRenderer>();
}
