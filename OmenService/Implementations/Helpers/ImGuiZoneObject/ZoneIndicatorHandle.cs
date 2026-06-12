using System.Numerics;

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
    public bool Unregister() =>
        IsValid && (Service?.UnregisterByID(ID) ?? false);

    /// <summary>
    ///     更新标记文字与颜色, 句柄已失效或服务不可用时返回 false
    /// </summary>
    public bool UpdateText(string? text, Vector4? textColor = null) =>
        IsValid && (Service?.UpdateTextByID(ID, text, textColor) ?? false);

    /// <summary>
    ///     更新自定义绘制逻辑, 句柄已失效或服务不可用时返回 false
    /// </summary>
    public bool UpdateDraw(Action<ZoneIndicatorDrawContext>? onDraw) =>
        IsValid && (Service?.UpdateDrawByID(ID, onDraw) ?? false);

    private static ImGuiZoneObjectIndicator? Service =>
        DService.Instance().GetOmenService<ImGuiZoneObjectIndicator>();
}
