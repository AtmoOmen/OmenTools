using System.Numerics;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Lumina.Excel.Sheets;
using OmenTools.Interop.Game.Helpers;
using OmenTools.Interop.Game.Lumina;

namespace OmenTools.ImGuiOm.Widgets.MapRenderer;

public class ImGuiMapRenderer
{
    public bool  Zoomable  { get; set; } = true;
    public bool  Pannable  { get; set; } = true;
    public float MinZoom   { get; set; } = 0.2f;
    public float MaxZoom   { get; set; } = 5.0f;
    public float LerpSpeed { get; set; } = 15.0f;

    public bool EnableResizeGrip { get; set; } = true;

    public uint MapID      { get; private set; }
    public Map? CurrentMap { get; private set; }

    private ISharedImmediateTexture? mapTexture;

    private Vector2 currentPan  = Vector2.Zero;
    private Vector2 targetPan   = Vector2.Zero;
    private float   currentZoom = 1.0f;
    private float   targetZoom  = 1.0f;

    private bool             isDragging;
    private bool             isMouseDown;
    private Vector2          mouseDownPos    = Vector2.Zero;
    private ImGuiMouseButton mouseDownButton = ImGuiMouseButton.Left;

    public Vector2 CustomViewportSize { get; set; } = Vector2.Zero;

    private readonly List<ImGuiMapMarker> markers = [];

    private readonly Dictionary<uint, ISharedImmediateTexture> iconTextureCache = [];

    public Action<ImGuiMapRenderer, Vector3, Vector2, ImGuiMouseButton>? OnMapClicked { get; set; }

    public Action<ImGuiMapRenderer, Vector3, Vector2>? OnMapHovered { get; set; }

    public Action<Vector2>? OnViewportResized { get; set; }

    public Action<ImGuiMapRenderer, ImDrawListPtr>? OnCustomMapDraw { get; set; }

    public Action<ImGuiMapRenderer, ImDrawListPtr>? OnCustomForegroundDraw { get; set; }

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

    public Vector2 WorldToScreen(Vector3 worldPos, Vector2 viewportMin, Vector2 viewportSize, Vector2 baseSize, float baseScale)
    {
        if (CurrentMap == null) return Vector2.Zero;


        var texturePos = PositionHelper.WorldToTexture(worldPos, CurrentMap.Value);


        var actualMin = viewportMin + ((viewportSize - (baseSize * currentZoom)) / 2f) + currentPan;
        return actualMin + (texturePos * baseScale * currentZoom);
    }

    public Vector2 WorldToScreen(Vector3 worldPos) =>
        WorldToScreen(worldPos, currentViewportMin, currentViewportSize, currentBaseSize, currentBaseScale);

    public Vector3 ScreenToWorld(Vector2 screenPos, Vector2 viewportMin, Vector2 viewportSize, Vector2 baseSize, float baseScale)
    {
        if (CurrentMap == null) return Vector3.Zero;

        var actualMin  = viewportMin + ((viewportSize - (baseSize * currentZoom)) / 2f) + currentPan;
        var texturePos = (screenPos - actualMin) / (baseScale                     * currentZoom);

        return PositionHelper.TextureToWorld(texturePos, CurrentMap.Value).ToVector3(0);
    }

    public Vector3 ScreenToWorld(Vector2 screenPos) =>
        ScreenToWorld(screenPos, currentViewportMin, currentViewportSize, currentBaseSize, currentBaseScale);

    public void Draw(Vector2 drawSize)
    {
        if (CurrentMap == null || mapTexture == null) return;

        var warp = mapTexture.GetWrapOrEmpty();
        if (warp.Handle == nint.Zero || warp.Width < 64 || warp.Height < 64) return;

        var drawList    = ImGui.GetWindowDrawList();
        var viewportMin = ImGui.GetCursorScreenPos();

        var viewportSize                         = drawSize;
        if (viewportSize.X <= 0f) viewportSize.X = ImGui.GetContentRegionAvail().X;
        if (viewportSize.Y <= 0f) viewportSize.Y = ImGui.GetContentRegionAvail().Y;

        if (CustomViewportSize is { X: > 50f, Y: > 50f }) viewportSize = CustomViewportSize;

        var gripSize   = EnableResizeGrip ? 14f * ImGuiHelpers.GlobalScale : 0f;
        var canvasSize = viewportSize - new Vector2(gripSize, gripSize);

        ImGui.InvisibleButton("##MapCanvas", canvasSize);

        var viewportMax = viewportMin + viewportSize;
        var isHovered   = ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);

        var isMouseInGrip = false;

        if (EnableResizeGrip)
        {
            var gripMin = viewportMax - new Vector2(gripSize, gripSize);
            var m       = ImGui.GetMousePos();
            isMouseInGrip = m.X >= gripMin.X && m.X <= viewportMax.X && m.Y >= gripMin.Y && m.Y <= viewportMax.Y;
        }

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

        var baseScale = Math.Min(viewportSize.X / warp.Width, viewportSize.Y / warp.Height);
        var baseSize  = new Vector2(warp.Width  * baseScale, warp.Height     * baseScale);

        currentViewportMin  = viewportMin;
        currentViewportSize = viewportSize;
        currentBaseSize     = baseSize;
        currentBaseScale    = baseScale;

        var dt    = ImGui.GetIO().DeltaTime;
        var lerpT = Math.Min(dt * LerpSpeed, 1.0f);

        var lastZoom = currentZoom;
        currentZoom += (targetZoom - currentZoom) * lerpT;

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

        currentPan += (targetPan - currentPan) * lerpT;

        if (Pannable)
        {
            switch (isMouseDown)
            {
                case true when ImGui.IsMouseDown(mouseDownButton):
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

                    break;
                }
                case true when !ImGui.IsMouseDown(mouseDownButton) && !ImGui.IsMouseReleased(mouseDownButton):
                    isMouseDown = false;
                    break;
            }

            if (isHovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                ResetView();
                isMouseDown = false;
                isDragging  = false;
            }
        }

        drawList.PushClipRect(viewportMin, viewportMax, true);

        var actualMin = viewportMin + ((viewportSize - (baseSize * currentZoom)) / 2f) + currentPan;
        var actualMax = actualMin   + (baseSize                                  * currentZoom);

        drawList.AddRectFilled(viewportMin, viewportMax, 0x1A000000, 4f);
        drawList.AddImage(warp.Handle, actualMin, actualMax);

        OnCustomMapDraw?.Invoke(this, drawList);

        var labelDrawTasks = new List<(ImGuiMapMarker Marker, Vector2 CenterPos)>();

        var             mousePos      = ImGui.GetMousePos();
        ImGuiMapMarker? hoveredMarker = null;

        foreach (var marker in markers)
        {
            var screenPos = WorldToScreen(marker.Position);


            if (screenPos.X < viewportMin.X - 50f ||
                screenPos.X > viewportMax.X + 50f ||
                screenPos.Y < viewportMin.Y - 50f ||
                screenPos.Y > viewportMax.Y + 50f) continue;

            var markerHalfSize = marker.Size / 2f;
            var markerMin      = screenPos - markerHalfSize;
            var markerMax      = screenPos + markerHalfSize;

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

            ImTextureID textureHandle = default;

            if (marker.Texture != null)
            {
                var markerWrap = marker.Texture.GetWrapOrEmpty();
                if (markerWrap.Handle != nint.Zero)
                    textureHandle = markerWrap.Handle;
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

            var drawMin = markerMin;
            var drawMax = markerMax;

            if (isMouseOver)
            {
                drawMin -= new Vector2(2f, 2f);
                drawMax += new Vector2(2f, 2f);
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            if (marker.OnCustomDraw != null)
                marker.OnCustomDraw(marker, screenPos, drawList);
            else if (textureHandle != nint.Zero)
                drawList.AddImage(textureHandle, drawMin, drawMax, Vector2.Zero, Vector2.One, marker.Color);
            else
            {

                drawList.AddCircleFilled(screenPos, Math.Min(marker.Size.X, marker.Size.Y) / 2f, marker.Color);
                drawList.AddCircle(screenPos, Math.Min(marker.Size.X,       marker.Size.Y) / 2f, 0xFFFFFFFF, 16, 1.5f);
            }

            if (marker.ShowLabel && !string.IsNullOrEmpty(marker.Label))
                labelDrawTasks.Add((marker, screenPos));
        }


        foreach (var (marker, centerPos) in labelDrawTasks)
        {
            var labelText = marker.Label;
            var textSize  = ImGui.CalcTextSize(labelText);
            var padding   = new Vector2(3f, 2f);

            var labelPos = centerPos - new Vector2(textSize.X / 2f, (marker.Size.Y / 2f) + textSize.Y + 4f);

            var minLimit = viewportMin + padding;
            var maxLimit = viewportMax - padding - textSize;

            if (minLimit.X < maxLimit.X) labelPos.X = Math.Clamp(labelPos.X, minLimit.X, maxLimit.X);
            if (minLimit.Y < maxLimit.Y) labelPos.Y = Math.Clamp(labelPos.Y, minLimit.Y, maxLimit.Y);

            var rectMin = labelPos - padding;
            var rectMax = labelPos + textSize + padding;

            drawList.AddRectFilled(rectMin, rectMax, 0xCC000000, 3f);
            drawList.AddRect(rectMin, rectMax, 0x40FFFFFF, 3f, ImDrawFlags.None, 1f);
            drawList.AddText(labelPos, marker.LabelColor, labelText);

            if (!string.IsNullOrEmpty(marker.Hint))
            {
                var hintText    = marker.Hint;
                var hintSize    = ImGui.CalcTextSize(hintText);
                var hintPadding = new Vector2(3f,             1f);
                var hintPos     = new Vector2(rectMax.X + 2f, rectMin.Y + ((rectMax.Y - rectMin.Y - hintSize.Y) / 2f));

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

        OnCustomForegroundDraw?.Invoke(this, drawList);

        drawList.AddRect(viewportMin, viewportMax, ImGui.GetColorU32(ImGuiCol.Border), 4f, ImDrawFlags.None, 2f);

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

        if (EnableResizeGrip)
        {
            var gripMin = viewportMax - new Vector2(gripSize, gripSize);

            drawList.AddTriangleFilled(viewportMax, viewportMax - new Vector2(gripSize, 0f), viewportMax - new Vector2(0f, gripSize), 0xAAFFFFFF);

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
