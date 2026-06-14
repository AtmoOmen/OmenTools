using System.Numerics;

namespace OmenTools.OmenService.ImGuiZoneObject;

/// <summary>
///     区域物体标记的可变内容视图, 通过 <see cref="ZoneIndicatorHandle.Update" /> 在运行期就地更新
///     仅暴露可变内容, 不含 ID、区域、临时 / 永久等身份与生命周期字段
/// </summary>
public interface IZoneIndicatorMutable
{
    /// <summary>
    ///     位置获取器, 仅 <see cref="ObjectGetter" /> 为 null 的固定位置条目生效
    /// </summary>
    Func<List<Vector3>>? PositionGetter { get; set; }

    /// <summary>
    ///     物体获取器, 非 null 时条目跟随物体, 为 null 时回退位置获取器
    /// </summary>
    Func<List<nint>>? ObjectGetter { get; set; }

    /// <summary>
    ///     按物体获取文字, 仅跟随物体条目使用; null 表示不绘制
    /// </summary>
    Func<nint, ZoneIndicatorText>? ObjTextGetter { get; set; }

    /// <summary>
    ///     按位置获取文字, 仅固定位置条目使用; null 表示不绘制
    /// </summary>
    Func<Vector3, ZoneIndicatorText>? PosTextGetter { get; set; }

    /// <summary>
    ///     自定义绘制逻辑, null 表示仅绘制文字
    /// </summary>
    Action<ZoneIndicatorDrawContext>? OnDraw { get; set; }

    /// <summary>
    ///     渲染半径范围 (yalm), 目标超出该距离时不渲染
    /// </summary>
    uint RenderRadius { get; set; }

    /// <summary>
    ///     目标地点被地形遮挡时是否不渲染
    /// </summary>
    bool HiddenWhenBlocked { get; set; }

    /// <summary>
    ///     目标周围的包围形状, null 表示不绘制
    /// </summary>
    ZoneIndicatorSurrounding? Surrounding { get; set; }
}
