using System.Numerics;
using Dalamud.Interface.Textures;

namespace OmenTools.ImGuiOm.Widgets.MapRenderer;

public class ImGuiMapMarker
{
    public string ID { get; set; } = string.Empty;

    // 3D 游戏世界坐标
    public Vector3 Position { get; set; }

    // 渲染像素尺寸 (默认 24x24)
    public Vector2 Size { get; set; } = new(24f, 24f);

    // 基础色调 (ARGB)
    public uint Color { get; set; } = 0xFFFFFFFF;

    // 自定义纹理句柄 (例如 ISharedImmediateTexture)
    public ISharedImmediateTexture? Texture { get; set; }

    // 游戏内置图标 ID (底图绘制时将自动加载对应纹理并缓存)
    public uint? IconID { get; set; }

    // 基础显示名称
    public string Name { get; set; } = string.Empty;

    // 详细描述 (用于 Tooltip 悬浮展示)
    public string Description { get; set; } = string.Empty;

    // 常驻显示的文本标签
    public string Label      { get; set; } = string.Empty;
    public bool   ShowLabel  { get; set; } = true;
    public uint   LabelColor { get; set; } = 0xFFFFFFFF;

    // 角标或热键提示信息 (如 Ctrl+1)
    public string Hint      { get; set; } = string.Empty;
    public uint   HintColor { get; set; } = 0xFF808080;

    // 悬浮提示文本
    public bool   ShowTooltip { get; set; } = true;
    public string TooltipText { get; set; } = string.Empty;

    // 脉冲波特效
    public bool PulseEffect { get; set; }
    public uint PulseColor  { get; set; } = 0x8000FF00;

    // 自定义绘制接口 (接收画布屏幕绝对位置及当前 drawList)
    public Action<ImGuiMapMarker, Vector2, ImDrawListPtr>? OnCustomDraw { get; set; }

    // 交互行为回调
    public Action<ImGuiMapMarker>? OnClick      { get; set; }
    public Action<ImGuiMapMarker>? OnRightClick { get; set; }
    public Action<ImGuiMapMarker>? OnHover      { get; set; }
}
