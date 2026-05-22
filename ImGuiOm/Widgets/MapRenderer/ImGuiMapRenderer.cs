using System.Numerics;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Helpers;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.MapRenderer;

public class ImGuiMapRenderer
{
    // 配置项
    public bool  Zoomable  { get; set; } = true;
    public bool  Pannable  { get; set; } = true;
    public float MinZoom   { get; set; } = 0.2f;
    public float MaxZoom   { get; set; } = 5.0f;
    public float LerpSpeed { get; set; } = 15.0f;

    // 是否启用 Resize Grip 动态拉伸
    public bool EnableResizeGrip { get; set; } = true;

    // 当前地图 ID 和 Map 记录
    public uint MapID      { get; private set; }
    public Map? CurrentMap { get; private set; }

    // 地图纹理
    private ISharedImmediateTexture? mapTexture;

    // 缓动平移与缩放平滑参数
    private Vector2 currentPan  = Vector2.Zero;
    private Vector2 targetPan   = Vector2.Zero;
    private float   currentZoom = 1.0f;
    private float   targetZoom  = 1.0f;

    // 拖拽手势检测状态
    private bool             isDragging;
    private bool             isMouseDown;
    private Vector2          mouseDownPos    = Vector2.Zero;
    private ImGuiMouseButton mouseDownButton = ImGuiMouseButton.Left;

    // 用户拉伸保存的尺寸
    public Vector2 CustomViewportSize { get; set; } = Vector2.Zero;

    // 地图元素 Marker 列表
    private readonly List<ImGuiMapMarker> markers = [];

    // 缓存的游戏 Icon 纹理 (避免重复拉取)
    private readonly Dictionary<uint, ISharedImmediateTexture> iconTextureCache = [];

    // 点击地图空白处或任意位置回调 (参数：渲染器, 3D世界坐标, 2D纹理像素坐标, 点击按键)
    public Action<ImGuiMapRenderer, Vector3, Vector2, ImGuiMouseButton>? OnMapClicked { get; set; }

    // 鼠标在地图上移动时回调
    public Action<ImGuiMapRenderer, Vector3, Vector2>? OnMapHovered { get; set; }

    // 视口大小发生拉伸变化时的回调
    public Action<Vector2>? OnViewportResized { get; set; }

    // 自定义底图图层绘制 (在底图之上、Marker 之下)
    public Action<ImGuiMapRenderer, ImDrawListPtr>? OnCustomMapDraw { get; set; }

    // 自定义顶层绘制 (在所有 Marker 和 Label 之后)
    public Action<ImGuiMapRenderer, ImDrawListPtr>? OnCustomForegroundDraw { get; set; }

    // 当前渲染视口的缓存状态 (用于免参数便捷坐标转换)
    private Vector2 currentViewportMin  = Vector2.Zero;
    private Vector2 currentViewportSize = Vector2.Zero;
    private Vector2 currentBaseSize     = Vector2.Zero;
    private float   currentBaseScale    = 1.0f;

    public void SetMap(uint mapID)
    {
        if (MapID == mapID && CurrentMap != null) return;

        MapID = mapID;
        var row = LuminaGetter.GetRow<Map>(mapID);

        if (row.HasValue)
        {
            CurrentMap = row.Value;
            var texturePath = CurrentMap.Value.GetTexturePath();
            mapTexture = DService.Instance().Texture.GetFromGame(texturePath);
        }
        else
        {
            CurrentMap = null;
            mapTexture = null;
        }

        ResetView();
    }

    public void ResetView()
    {
        currentPan  = Vector2.Zero;
        targetPan   = Vector2.Zero;
        currentZoom = 1.0f;
        targetZoom  = 1.0f;
    }

    public void AddMarker(ImGuiMapMarker marker)
    {
        if (string.IsNullOrEmpty(marker.ID)) marker.ID = Guid.NewGuid().ToString();

        markers.RemoveAll(x => x.ID == marker.ID);
        markers.Add(marker);
    }

    public bool RemoveMarker(string id) =>
        markers.RemoveAll(x => x.ID == id) > 0;

    public void ClearMarkers() =>
        markers.Clear();

    public ImGuiMapMarker? GetMarker(string id) =>
        markers.FirstOrDefault(x => x.ID == id);

    // 将 3D 世界坐标映射为屏幕 Canvas 坐标
    public Vector2 WorldToScreen(Vector3 worldPos, Vector2 viewportMin, Vector2 viewportSize, Vector2 baseSize, float baseScale)
    {
        if (CurrentMap == null) return Vector2.Zero;

        // 游戏 3D 坐标 -> 纹理像素坐标
        var texturePos = PositionHelper.WorldToTexture(worldPos, CurrentMap.Value);

        // 纹理像素坐标 -> 屏幕像素位置 (结合平移与缩放)
        var actualMin = viewportMin + ((viewportSize - (baseSize * currentZoom)) / 2f) + currentPan;
        return actualMin + (texturePos * baseScale * currentZoom);
    }

    // 免参数便捷转换：将 3D 世界坐标映射为当前渲染周期下的屏幕 Canvas 坐标
    public Vector2 WorldToScreen(Vector3 worldPos) =>
        WorldToScreen(worldPos, currentViewportMin, currentViewportSize, currentBaseSize, currentBaseScale);

    // 将屏幕 Canvas 坐标反推回 3D 世界坐标
    public Vector3 ScreenToWorld(Vector2 screenPos, Vector2 viewportMin, Vector2 viewportSize, Vector2 baseSize, float baseScale)
    {
        if (CurrentMap == null) return Vector3.Zero;

        var actualMin  = viewportMin + ((viewportSize - (baseSize * currentZoom)) / 2f) + currentPan;
        var texturePos = (screenPos - actualMin) / (baseScale                     * currentZoom);

        return PositionHelper.TextureToWorld(texturePos, CurrentMap.Value).ToVector3(0);
    }

    // 免参数便捷转换：将当前渲染周期下的屏幕 Canvas 坐标反推回 3D 世界坐标
    public Vector3 ScreenToWorld(Vector2 screenPos) =>
        ScreenToWorld(screenPos, currentViewportMin, currentViewportSize, currentBaseSize, currentBaseScale);

    public void Draw(Vector2 drawSize)
    {
        if (CurrentMap == null || mapTexture == null) return;

        var warp = mapTexture.GetWrapOrEmpty();
        if (warp.Handle == nint.Zero || warp.Width < 64 || warp.Height < 64) return;

        var drawList    = ImGui.GetWindowDrawList();
        var viewportMin = ImGui.GetCursorScreenPos();

        // 自适应视口尺寸计算
        var viewportSize                         = drawSize;
        if (viewportSize.X <= 0f) viewportSize.X = ImGui.GetContentRegionAvail().X;
        if (viewportSize.Y <= 0f) viewportSize.Y = ImGui.GetContentRegionAvail().Y;

        // 如果外部拉伸过，覆盖为自定义尺寸
        if (CustomViewportSize is { X: > 50f, Y: > 50f }) viewportSize = CustomViewportSize;

        // 占位，防止元素重叠，并拦截鼠标左键拖拽以防止外部容器移动
        ImGui.InvisibleButton("##MapCanvas", viewportSize);

        // 渲染视口矩形
        var viewportMax = viewportMin + viewportSize;
        var isHovered   = ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);

        // 判定是否在拉伸手柄内，避免拖拽与缩放手势冲突
        var isMouseInGrip = false;

        if (EnableResizeGrip)
        {
            var gripSize = 14f * ImGuiHelpers.GlobalScale;
            var gripMin  = viewportMax - new Vector2(gripSize, gripSize);
            var m        = ImGui.GetMousePos();
            isMouseInGrip = m.X >= gripMin.X && m.X <= viewportMax.X && m.Y >= gripMin.Y && m.Y <= viewportMax.Y;
        }

        // 记录鼠标按下起点，用于拖拽与点击防抖判定
        if (isHovered && !isMouseInGrip)
        {
            for (var btn = ImGuiMouseButton.Left; btn <= ImGuiMouseButton.Right; btn++)
                if (ImGui.IsMouseClicked(btn))
                {
                    mouseDownPos    = ImGui.GetMousePos();
                    isMouseDown     = true;
                    mouseDownButton = btn;
                    isDragging      = false;
                    break;
                }
        }

        // 底图最大边自适应缩放比例
        var baseScale = Math.Min(viewportSize.X / warp.Width, viewportSize.Y / warp.Height);
        var baseSize  = new Vector2(warp.Width  * baseScale, warp.Height     * baseScale);

        // 缓存当前视口参数，支持免参 WorldToScreen / ScreenToWorld
        currentViewportMin  = viewportMin;
        currentViewportSize = viewportSize;
        currentBaseSize     = baseSize;
        currentBaseScale    = baseScale;

        // 缓动平滑插值 (Pan & Zoom Lerp)
        var dt    = ImGui.GetIO().DeltaTime;
        var lerpT = Math.Min(dt * LerpSpeed, 1.0f);

        var lastZoom = currentZoom;
        currentZoom = currentZoom + ((targetZoom - currentZoom) * lerpT);

        // 缩放中心对齐鼠标
        if (Zoomable && isHovered && ImGui.GetIO().MouseWheel != 0f)
        {
            var wheel      = ImGui.GetIO().MouseWheel;
            var zoomFactor = 1.0f + (wheel * 0.12f);
            var nextZoom   = Math.Clamp(targetZoom * zoomFactor, MinZoom, MaxZoom);

            var relMouse    = ImGui.GetMousePos() - viewportMin;
            var mapPixelPos = (relMouse - ((viewportSize - (baseSize * targetZoom)) / 2f) - targetPan) / lastZoom;

            targetPan  = relMouse - ((viewportSize - (baseSize * nextZoom)) / 2f) - (mapPixelPos * nextZoom);
            targetZoom = nextZoom;
        }

        currentPan = currentPan + ((targetPan - currentPan) * lerpT);

        // 拖拽平移逻辑
        if (Pannable)
        {
            if (isMouseDown && ImGui.IsMouseDown(mouseDownButton))
            {
                var dragDelta = ImGui.GetMousePos() - mouseDownPos;

                if (isDragging || dragDelta.Length() > 16f)
                {
                    isDragging = true;
                    var delta = ImGui.GetIO().MouseDelta;

                    if (delta != Vector2.Zero)
                    {
                        targetPan  += delta;
                        currentPan += delta;
                    }
                }
            }
            else
            {
                // 只有当按键既没有按着也没有释放时，才清除按下状态（避免在释放那一帧将 isMouseDown 误清零导致点击失效）
                if (isMouseDown && !ImGui.IsMouseDown(mouseDownButton) && !ImGui.IsMouseReleased(mouseDownButton)) isMouseDown = false;
            }

            // 双击空白处复位
            if (isHovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                ResetView();
                isMouseDown = false;
                isDragging  = false;
            }
        }

        // 视口边界裁剪
        drawList.PushClipRect(viewportMin, viewportMax, true);

        // 绘制阴影与背景底图
        var actualMin = viewportMin + ((viewportSize - (baseSize * currentZoom)) / 2f) + currentPan;
        var actualMax = actualMin   + (baseSize                                  * currentZoom);

        drawList.AddRectFilled(viewportMin, viewportMax, 0x1A000000, 4f);
        drawList.AddImage(warp.Handle, actualMin, actualMax);

        // 允许调用方在底图之上、Marker 之下绘制连线、多边形等自定义背景
        OnCustomMapDraw?.Invoke(this, drawList);

        // 用于存储所有待渲染 of Label 及其绘制坐标，以防 Label 之间重叠，或需要 Clamp 在地图边缘内
        var labelDrawTasks = new List<(ImGuiMapMarker Marker, Vector2 CenterPos)>();

        // 绘制所有 Marker
        var             mousePos      = ImGui.GetMousePos();
        ImGuiMapMarker? hoveredMarker = null;

        foreach (var marker in markers)
        {
            var screenPos = WorldToScreen(marker.Position);

            // 如果 Marker 投影在裁剪视口之外，跳过不画
            if (screenPos.X < viewportMin.X - 50f ||
                screenPos.X > viewportMax.X + 50f ||
                screenPos.Y < viewportMin.Y - 50f ||
                screenPos.Y > viewportMax.Y + 50f) continue;

            var markerHalfSize = marker.Size / 2f;
            var markerMin      = screenPos - markerHalfSize;
            var markerMax      = screenPos + markerHalfSize;

            // 检测悬浮状态
            var isMouseOver = isHovered                 &&
                              mousePos.X >= markerMin.X &&
                              mousePos.X <= markerMax.X &&
                              mousePos.Y >= markerMin.Y &&
                              mousePos.Y <= markerMax.Y;

            if (isMouseOver)
            {
                hoveredMarker = marker;
                marker.OnHover?.Invoke(marker);
            }

            // 脉冲环特效渲染
            if (marker.PulseEffect)
            {
                var time        = (float)ImGui.GetTime();
                var pulse       = ((float)Math.Sin(time * 8f) * 0.15f) + 1.0f;
                var pulseRadius = marker.Size.X * 0.7f * pulse;

                var animTime   = time % 1.2f;
                var waveAlpha  = 1.0f - (animTime / 1.2f);
                var waveRadius = (marker.Size.X   * 0.5f) + (animTime * 25f);

                if (waveAlpha > 0f)
                {
                    var waveColor = marker.PulseColor;
                    var alpha     = (uint)(waveAlpha * ((waveColor >> 24) & 0xFF));
                    waveColor = (waveColor & 0x00FFFFFF) | (alpha << 24);
                    drawList.AddCircle(screenPos, waveRadius, waveColor, 32, 2f);
                }

                drawList.AddCircleFilled(screenPos, pulseRadius, (marker.PulseColor & 0x00FFFFFF) | 0x40000000);
            }

            // 获取渲染纹理
            ImTextureID textureHandle = default;

            if (marker.Texture != null)
            {
                var markerWrap                                    = marker.Texture.GetWrapOrEmpty();
                if (markerWrap.Handle != nint.Zero) textureHandle = markerWrap.Handle;
            }
            else if (marker.IconID.HasValue)
            {
                if (!iconTextureCache.TryGetValue(marker.IconID.Value, out var iconTex))
                {
                    iconTex                               = DService.Instance().Texture.GetFromGameIcon(new(marker.IconID.Value));
                    iconTextureCache[marker.IconID.Value] = iconTex;
                }

                var iconWrap                                    = iconTex.GetWrapOrEmpty();
                if (iconWrap.Handle != nint.Zero) textureHandle = iconWrap.Handle;
            }

            // 基础微动动效 (Hover 状态下略微放大和亮色特效)
            var drawMin = markerMin;
            var drawMax = markerMax;

            if (isMouseOver)
            {
                drawMin -= new Vector2(2f, 2f);
                drawMax += new Vector2(2f, 2f);
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            // 自定义或默认图标渲染
            if (marker.OnCustomDraw != null) marker.OnCustomDraw(marker, screenPos, drawList);
            else if (textureHandle  != nint.Zero) drawList.AddImage(textureHandle, drawMin, drawMax, Vector2.Zero, Vector2.One, marker.Color);
            else
            {
                // 没有纹理时默认绘制小圆形
                drawList.AddCircleFilled(screenPos, Math.Min(marker.Size.X, marker.Size.Y) / 2f, marker.Color);
                drawList.AddCircle(screenPos, Math.Min(marker.Size.X,       marker.Size.Y) / 2f, 0xFFFFFFFF, 16, 1.5f);
            }

            // 缓存要绘制的常驻 Label 任务
            if (marker.ShowLabel && !string.IsNullOrEmpty(marker.Label)) labelDrawTasks.Add((marker, screenPos));
        }

        // 集中批量渲染常驻 Label 文本，避免被图标遮挡
        foreach (var (marker, centerPos) in labelDrawTasks)
        {
            var labelText = marker.Label;
            var textSize  = ImGui.CalcTextSize(labelText);
            var padding   = new Vector2(3f, 2f);

            // 标签定位在图标正下方
            var labelPos = centerPos - new Vector2(textSize.X / 2f, (marker.Size.Y / 2f) + textSize.Y + 4f);

            // 智能 Clamp 限制在底图视口边缘内部，防裁剪丢失
            var minLimit = viewportMin + padding;
            var maxLimit = viewportMax - padding - textSize;

            if (minLimit.X < maxLimit.X) labelPos.X = Math.Clamp(labelPos.X, minLimit.X, maxLimit.X);
            if (minLimit.Y < maxLimit.Y) labelPos.Y = Math.Clamp(labelPos.Y, minLimit.Y, maxLimit.Y);

            var rectMin = labelPos - padding;
            var rectMax = labelPos + textSize + padding;

            // 绘制气泡文本框
            drawList.AddRectFilled(rectMin, rectMax, 0xCC000000, 3f);
            drawList.AddRect(rectMin, rectMax, 0x40FFFFFF, 3f, ImDrawFlags.None, 1f);
            drawList.AddText(labelPos, marker.LabelColor, labelText);

            // 渲染次级小 Hint 胶囊
            if (!string.IsNullOrEmpty(marker.Hint))
            {
                var hintText    = marker.Hint;
                var hintSize    = ImGui.CalcTextSize(hintText);
                var hintPadding = new Vector2(3f,             1f);
                var hintPos     = new Vector2(rectMax.X + 2f, rectMin.Y + ((rectMax.Y - rectMin.Y - hintSize.Y) / 2f));

                // 也需要边缘裁剪检查
                if (hintPos.X + hintSize.X + hintPadding.X < viewportMax.X)
                {
                    var hintRectMin = hintPos - hintPadding;
                    var hintRectMax = hintPos + hintSize + hintPadding;
                    drawList.AddRectFilled(hintRectMin, hintRectMax, 0xE6262626, 3f);
                    drawList.AddRect(hintRectMin, hintRectMax, 0x30FFFFFF, 3f, ImDrawFlags.None, 1f);
                    drawList.AddText(hintPos, marker.HintColor, hintText);
                }
            }
        }

        // 渲染悬浮框气泡 (Tooltip)
        if (hoveredMarker is { ShowTooltip: true })
        {
            var tooltip = string.IsNullOrEmpty(hoveredMarker.TooltipText) ? hoveredMarker.Name : hoveredMarker.TooltipText;

            if (!string.IsNullOrEmpty(tooltip))
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(tooltip);
                if (!string.IsNullOrEmpty(hoveredMarker.Description)) ImGui.TextDisabled(hoveredMarker.Description);
                ImGui.EndTooltip();
            }
        }

        // 允许调用方在最上层绘制悬浮内容
        OnCustomForegroundDraw?.Invoke(this, drawList);

        // 视口四角边框线
        drawList.AddRect(viewportMin, viewportMax, ImGui.GetColorU32(ImGuiCol.Border), 4f, ImDrawFlags.None, 2f);

        // 地图视口与空白处的点击/悬停事件
        if (isHovered && !isDragging)
        {
            var worldPos   = ScreenToWorld(mousePos, viewportMin, viewportSize, baseSize, baseScale);
            var texturePos = (mousePos - actualMin) / (baseScale * currentZoom);

            OnMapHovered?.Invoke(this, worldPos, texturePos);
        }

        if (isMouseDown && ImGui.IsMouseReleased(mouseDownButton))
        {
            if (!isDragging)
            {
                var releasePos = ImGui.GetMousePos();
                var worldPos   = ScreenToWorld(releasePos, viewportMin, viewportSize, baseSize, baseScale);
                var texturePos = (releasePos - actualMin) / (baseScale * currentZoom);

                var handled = false;

                if (hoveredMarker != null)
                {
                    if (mouseDownButton == ImGuiMouseButton.Left && hoveredMarker.OnClick != null)
                    {
                        hoveredMarker.OnClick.Invoke(hoveredMarker);
                        handled = true;
                    }
                    else if (mouseDownButton == ImGuiMouseButton.Right && hoveredMarker.OnRightClick != null)
                    {
                        hoveredMarker.OnRightClick.Invoke(hoveredMarker);
                        handled = true;
                    }
                }

                if (!handled) OnMapClicked?.Invoke(this, worldPos, texturePos, mouseDownButton);
            }

            isMouseDown = false;
            isDragging  = false;
        }

        // 视口边缘拉伸 Resize Grip 渲染与事件处理
        if (EnableResizeGrip)
        {
            var gripSize = 14f * ImGuiHelpers.GlobalScale;
            var gripMin  = viewportMax - new Vector2(gripSize, gripSize);

            drawList.AddTriangleFilled(viewportMax, viewportMax - new Vector2(gripSize, 0f), viewportMax - new Vector2(0f, gripSize), 0xAAFFFFFF);

            // 检查拖拽手柄更改视口尺寸
            ImGui.SetCursorScreenPos(gripMin);
            ImGui.InvisibleButton("##MapRendererResizeGrip", new Vector2(gripSize, gripSize));

            if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                var delta       = ImGui.GetIO().MouseDelta;
                var currentSize = CustomViewportSize.X > 50f ? CustomViewportSize : viewportSize;
                CustomViewportSize = new Vector2
                (
                    Math.Max(100f, currentSize.X + delta.X),
                    Math.Max(100f, currentSize.Y + delta.Y)
                );
                OnViewportResized?.Invoke(CustomViewportSize);
            }
        }

        drawList.PopClipRect();
    }
}
