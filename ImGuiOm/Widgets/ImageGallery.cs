using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Utility.Numerics;
using OmenTools.OmenService.KeyboardShortcut;
using static OmenTools.ImGuiOm.ImGuiOm;

namespace OmenTools.ImGuiOm.Widgets;

/// <summary>
///     支持缩放、平移、快捷键导航与缩略图切换的图片画廊弹窗控件。
/// </summary>
/// <param name="popupID">弹窗唯一标识；为空或空白时使用默认标识。</param>
public sealed class ImageGallery
(
    string? popupID = null
)
{
    private readonly string popupID = string.IsNullOrWhiteSpace(popupID) ?
                                          DEFAULT_POPUP_ID :
                                          popupID;

    private readonly List<KeyboardShortcutEvent> pendingShortcuts = [];

    private int     shortcutScopeID;
    private double  galleryTransitionStart;
    private double  snapFeedbackStart = -10;
    private double  snapHoldUntil;
    private float   snapDetentAccum;
    private int     snapHoldDirection;
    private float   arrowVisibility;
    private float   currentZoom = 1f;
    private float   targetZoom  = 1f;
    private float   lastFitZoom;
    private float   thumbnailScrollX;
    private float   targetThumbnailScrollX;
    private int     lastThumbnailScrollIndex = -1;
    private int     previewImageIndex;
    private string? previousImageURL;
    private string? activeImageURL;
    private Vector2 currentPan;
    private Vector2 targetPan;
    private Vector2 previousImagePan;
    private float   previousImageZoom = 1f;
    private bool    zoomInitialized;
    private bool    isFitLocked = true;
    private bool    isDragging;
    private bool    isSnappedState;

    /// <summary>
    ///     绘制图片画廊弹窗，支持滚轮缩放、拖拽平移、键盘导航与缩略图切换。
    /// </summary>
    /// <param name="urls">待展示的图片地址列表。</param>
    /// <param name="popupCenter">弹窗期望居中显示的屏幕坐标。</param>
    /// <param name="open">是否在当前帧打开画廊弹窗。</param>
    /// <param name="initialIndex">打开弹窗时默认选中的图片索引。</param>
    /// <param name="loadingText">图片加载中显示的提示文本；为空时仅绘制加载动画。</param>
    public void Draw
    (
        IReadOnlyList<string> urls,
        Vector2               popupCenter,
        bool                  open,
        int                   initialIndex = 0,
        string?               loadingText  = null
    )
    {
        if (open)
        {
            previewImageIndex = urls.Count > 0 ?
                                    Math.Clamp(initialIndex, 0, urls.Count - 1) :
                                    0;
            galleryTransitionStart   = ImGui.GetTime();
            previousImageURL         = null;
            activeImageURL           = null;
            zoomInitialized          = false;
            isFitLocked              = true;
            isDragging               = false;
            isSnappedState           = false;
            snapFeedbackStart        = -10;
            snapHoldUntil            = 0;
            snapDetentAccum          = 0;
            snapHoldDirection        = 0;
            arrowVisibility          = 0;
            thumbnailScrollX         = 0;
            targetThumbnailScrollX   = 0;
            lastThumbnailScrollIndex = -1;
            currentPan               = Vector2.Zero;
            targetPan                = Vector2.Zero;
            ImGui.OpenPopup(popupID);
        }

        var uiScale        = GlobalUIScale;
        var spacing        = GALLERY_SPACING         * uiScale;
        var windowRounding = GALLERY_WINDOW_ROUNDING * uiScale;
        var displaySize    = ImGui.GetIO().DisplaySize;
        var viewportMargin = GALLERY_VIEWPORT_MARGIN                            * uiScale;
        var minGallerySize = new Vector2(GALLERY_MIN_WIDTH, GALLERY_MIN_HEIGHT) * uiScale;
        var maxGallerySize = Vector2.Max
        (
            minGallerySize,
            displaySize - new Vector2((viewportMargin + spacing) * 2f)
        );
        var gallerySize = Vector2.Clamp
        (
            displaySize * GALLERY_VIEWPORT_RATIO,
            minGallerySize,
            maxGallerySize
        );
        var windowOuterSize = gallerySize            + new Vector2(spacing * 2f);
        var minCenter       = (windowOuterSize / 2f) + new Vector2(viewportMargin);
        var maxCenter       = Vector2.Max(minCenter, displaySize - (windowOuterSize / 2f) - new Vector2(viewportMargin));
        var clampedCenter = Vector2.Clamp
        (
            popupCenter,
            minCenter,
            maxCenter
        );

        using var windowPadding  = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding,   new Vector2(spacing));
        using var roundingStyle  = ImRaii.PushStyle(ImGuiStyleVar.WindowRounding,  windowRounding);
        using var borderStyle    = ImRaii.PushStyle(ImGuiStyleVar.PopupBorderSize, 1f);
        using var popupBgColor   = ImRaii.PushColor(ImGuiCol.PopupBg, ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg] with { W = 0.96f });
        using var popupBorderCol = ImRaii.PushColor(ImGuiCol.Border,  ImGui.GetStyle().Colors[(int)ImGuiCol.Border] with { W = 0.75f });

        if (ImGui.IsPopupOpen(popupID))
            ImGui.SetNextWindowPos
            (
                clampedCenter,
                ImGuiCond.Always,
                new Vector2(0.5f)
            );

        using var popup = ImRaii.Popup(popupID, POPUP_WINDOW_FLAGS);

        if (!popup)
        {
            isDragging = false;
            KeyboardShortcutManager.Instance().Release(ref shortcutScopeID);
            return;
        }

        if (urls.Count == 0)
        {
            KeyboardShortcutManager.Instance().Release(ref shortcutScopeID);
            return;
        }

        pendingShortcuts.Clear();
        KeyboardShortcutManager.Instance().Drain(shortcutScopeID, pendingShortcuts);

        previewImageIndex = Math.Clamp
        (
            previewImageIndex,
            0,
            urls.Count - 1
        );

        var hasMultiple       = urls.Count > 1;
        var thumbnailItemSize = GALLERY_THUMBNAIL_SIZE    * uiScale;
        var thumbnailPadding  = GALLERY_THUMBNAIL_OUTLINE * uiScale * 2f;
        var thumbnailBarH     = thumbnailItemSize + (thumbnailPadding * 2f);
        var canvasSize = hasMultiple ?
                             gallerySize with { Y = MathF.Max(120f * uiScale, gallerySize.Y - thumbnailBarH - spacing) } :
                             gallerySize;
        var canvasMin      = ImGui.GetCursorScreenPos();
        var canvasMax      = canvasMin + canvasSize;
        var canvasCenter   = canvasMin + (canvasSize / 2f);
        var canvasRounding = GALLERY_CANVAS_ROUNDING * uiScale;
        var drawList       = ImGui.GetWindowDrawList();
        var now            = ImGui.GetTime();
        var deltaTime      = MathF.Min(ImGui.GetIO().DeltaTime, 0.05f);

        ImGui.SetCursorScreenPos(canvasMin);
        ImGui.Dummy(canvasSize);

        var isCanvasHovered = ImGui.IsMouseHoveringRect(canvasMin, canvasMax) &&
                              ImGui.IsWindowHovered();

        var arrowRadius = GALLERY_ARROW_RADIUS * uiScale;
        var arrowOffset = spacing + arrowRadius;
        var leftArrow   = canvasCenter with { X = canvasMin.X + arrowOffset };
        var rightArrow  = canvasCenter with { X = canvasMax.X - arrowOffset };
        var closeRadius = GALLERY_CLOSE_RADIUS * uiScale;
        var closeCenter = new Vector2(canvasMax.X - spacing - closeRadius, canvasMin.Y + spacing + closeRadius);

        var goPrevious        = false;
        var goNext            = false;
        var isPreviousHovered = false;
        var isNextHovered     = false;

        if (hasMultiple)
        {
            ImGui.SetCursorScreenPos(leftArrow - new Vector2(arrowRadius));
            using (ImRaii.Disabled(previewImageIndex == 0))
                goPrevious = ImGui.InvisibleButton("##GalleryPrevious", new Vector2(arrowRadius * 2f));
            ImGuiP.SetItemUsingMouseWheel();
            isPreviousHovered = ImGui.IsItemHovered();

            ImGui.SetCursorScreenPos(rightArrow - new Vector2(arrowRadius));
            using (ImRaii.Disabled(previewImageIndex == urls.Count - 1))
                goNext = ImGui.InvisibleButton("##GalleryNext", new Vector2(arrowRadius * 2f));
            ImGuiP.SetItemUsingMouseWheel();
            isNextHovered = ImGui.IsItemHovered();
        }

        ImGui.SetCursorScreenPos(closeCenter - new Vector2(closeRadius));
        var closeClicked = ImGui.InvisibleButton("##GalleryClose", new Vector2(closeRadius * 2f));
        ImGuiP.SetItemUsingMouseWheel();
        var isCloseHovered = ImGui.IsItemHovered();

        var shouldClose = closeClicked ||
                          HasPendingShortcut(ImGuiKey.Escape);

        if (shouldClose)
        {
            isDragging = false;
            KeyboardShortcutManager.Instance().Release(ref shortcutScopeID);
            ImGui.CloseCurrentPopup();
        }
        else
            KeyboardShortcutManager.Instance().KeepAlive
            (
                ref shortcutScopeID,
                hasMultiple ?
                    MATCHES_MULTI_IMAGE_SHORTCUT :
                    MATCHES_SINGLE_IMAGE_SHORTCUT
            );

        var requestedIndex = previewImageIndex;

        if (hasMultiple)
        {
            if (goPrevious ||
                HasPendingShortcut(ImGuiKey.LeftArrow))
                requestedIndex--;

            if (goNext ||
                HasPendingShortcut(ImGuiKey.RightArrow))
                requestedIndex++;

            if (HasPendingShortcut(ImGuiKey.Home))
                requestedIndex = 0;

            if (HasPendingShortcut(ImGuiKey.End))
                requestedIndex = urls.Count - 1;

            requestedIndex = Math.Clamp
            (
                requestedIndex,
                0,
                urls.Count - 1
            );

            var clickedThumbnail = DrawGalleryThumbnails
            (
                urls,
                canvasMin with { Y = canvasMax.Y + spacing },
                gallerySize.X,
                thumbnailBarH,
                thumbnailItemSize,
                spacing,
                requestedIndex,
                deltaTime
            );

            if (clickedThumbnail >= 0)
                requestedIndex = clickedThumbnail;

            requestedIndex = Math.Clamp
            (
                requestedIndex,
                0,
                urls.Count - 1
            );
        }

        if (requestedIndex != previewImageIndex)
        {
            previousImageURL       = urls[previewImageIndex];
            previousImageZoom      = currentZoom;
            previousImagePan       = currentPan;
            previewImageIndex      = requestedIndex;
            galleryTransitionStart = now;
            zoomInitialized        = false;
            isFitLocked            = true;
            isDragging             = false;
            isSnappedState         = false;
            snapHoldUntil          = 0;
            snapDetentAccum        = 0;
            snapHoldDirection      = 0;
        }

        var currentURL = urls[previewImageIndex];
        var hasImage = ImageHelper.Instance().TryGetImage(currentURL, out var image) &&
                       image.Size is { X: > 0f, Y: > 0f };

        var fitZoom     = 1f;
        var defaultZoom = 1f;
        var minZoom     = ZOOM_MIN_ABSOLUTE;
        var maxZoom     = ZOOM_MAX_BASE;

        if (hasImage)
        {
            var innerSize = Vector2.Max(Vector2.One, canvasSize - new Vector2(spacing * 2f));
            fitZoom     = MathF.Min(innerSize.X / image.Size.X, innerSize.Y / image.Size.Y);
            defaultZoom = MathF.Min(fitZoom,                    1f);
            minZoom     = Math.Clamp(MathF.Min(fitZoom                   * 0.5f, 0.25f), ZOOM_MIN_ABSOLUTE, defaultZoom);
            maxZoom     = MathF.Max(ZOOM_MAX_BASE, MathF.Ceiling(fitZoom * 4f));

            if (!zoomInitialized ||
                !string.Equals(activeImageURL, currentURL, StringComparison.Ordinal))
            {
                activeImageURL  = currentURL;
                currentZoom     = defaultZoom;
                targetZoom      = defaultZoom;
                currentPan      = Vector2.Zero;
                targetPan       = Vector2.Zero;
                lastFitZoom     = fitZoom;
                zoomInitialized = true;
                isFitLocked     = true;
                isSnappedState  = false;
            }
            else if (MathF.Abs(lastFitZoom - fitZoom) > 0.001f)
            {
                if (isFitLocked)
                {
                    currentZoom = defaultZoom;
                    targetZoom  = defaultZoom;
                    currentPan  = Vector2.Zero;
                    targetPan   = Vector2.Zero;
                }
                else
                {
                    targetZoom  = Math.Clamp(targetZoom,  minZoom, maxZoom);
                    currentZoom = Math.Clamp(currentZoom, minZoom, maxZoom);
                    targetPan   = ClampPan(targetPan,  image.Size, targetZoom,  canvasSize);
                    currentPan  = ClampPan(currentPan, image.Size, currentZoom, canvasSize);
                }

                lastFitZoom = fitZoom;
            }
        }

        var zoomBadgeClicked = false;
        var isZoomHovered    = false;
        var zoomBadgeMin     = Vector2.Zero;
        var zoomBadgeMax     = Vector2.Zero;
        var zoomText         = string.Empty;
        var zoomIconText     = string.Empty;
        var zoomIconMetrics  = default(TextMetrics);
        var zoomTextMetrics  = default(TextMetrics);
        var pillPadding      = new Vector2(spacing * 0.8f, spacing * 0.35f);
        var pillIconSpacing  = 5f * uiScale;

        if (hasImage)
        {
            using var pillFont = FontManager.Instance().UIFont80.Push();

            var isAtDefault = MathF.Abs(targetZoom - defaultZoom) <= ZOOM_EPSILON;
            var percent     = MathF.Round(currentZoom * 100f);
            zoomText = $"{percent:0}%";
            zoomIconText = (isAtDefault ?
                                FontAwesomeIcon.Expand :
                                FontAwesomeIcon.Compress).ToIconString();
            zoomIconMetrics = TextMetrics.Measure(zoomIconText);
            zoomTextMetrics = TextMetrics.Measure(zoomText);

            var badgeWidth  = zoomIconMetrics.Size.X                                    + pillIconSpacing + zoomTextMetrics.Size.X + (pillPadding.X * 2f);
            var badgeHeight = MathF.Max(zoomIconMetrics.Size.Y, zoomTextMetrics.Size.Y) + (pillPadding.Y                                            * 2f);
            zoomBadgeMin = new Vector2(canvasMin.X + spacing, canvasMax.Y - spacing - badgeHeight);
            zoomBadgeMax = zoomBadgeMin + new Vector2(badgeWidth, badgeHeight);

            ImGui.SetCursorScreenPos(zoomBadgeMin);
            zoomBadgeClicked = ImGui.InvisibleButton("##GalleryZoomBadge", zoomBadgeMax - zoomBadgeMin);
            ImGuiP.SetItemUsingMouseWheel();
            isZoomHovered = ImGui.IsItemHovered();
        }

        var isOverOverlayControl = isPreviousHovered ||
                                   isNextHovered     ||
                                   isCloseHovered    ||
                                   isZoomHovered;

        if (hasImage)
        {
            var canPan = image.Size.X * targetZoom > canvasSize.X + 1f ||
                         image.Size.Y * targetZoom > canvasSize.Y + 1f;

            if (isCanvasHovered       &&
                !isOverOverlayControl &&
                ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                ToggleQuickZoom
                (
                    ImGui.GetMousePos(),
                    canvasCenter,
                    canvasSize,
                    image.Size,
                    fitZoom,
                    defaultZoom,
                    minZoom,
                    maxZoom,
                    now
                );
            else if (zoomBadgeClicked)
                ToggleQuickZoom
                (
                    canvasCenter,
                    canvasCenter,
                    canvasSize,
                    image.Size,
                    fitZoom,
                    defaultZoom,
                    minZoom,
                    maxZoom,
                    now
                );

            var wheel = isCanvasHovered ?
                            ImGui.GetIO().MouseWheel :
                            0f;
            var keyZoomDelta = 0f;
            if (HasPendingShortcut(ImGuiKey.Equal) ||
                HasPendingShortcut(ImGuiKey.KeypadAdd))
                keyZoomDelta = 1f;
            else if (HasPendingShortcut(ImGuiKey.Minus) ||
                     HasPendingShortcut(ImGuiKey.KeypadSubtract))
                keyZoomDelta = -1f;

            if (wheel != 0f)
                ApplyZoomStep
                (
                    wheel,
                    ImGui.GetMousePos(),
                    canvasCenter,
                    canvasSize,
                    image.Size,
                    fitZoom,
                    defaultZoom,
                    minZoom,
                    maxZoom,
                    now
                );
            else if (keyZoomDelta != 0f)
                ApplyZoomStep
                (
                    keyZoomDelta,
                    canvasCenter,
                    canvasCenter,
                    canvasSize,
                    image.Size,
                    fitZoom,
                    defaultZoom,
                    minZoom,
                    maxZoom,
                    now
                );

            if (HasPendingShortcut(ImGuiKey.Key0)    ||
                HasPendingShortcut(ImGuiKey.Keypad0) ||
                HasPendingShortcut(ImGuiKey.R))
                SetZoomTarget
                (
                    defaultZoom,
                    canvasCenter,
                    canvasCenter,
                    canvasSize,
                    image.Size,
                    defaultZoom,
                    true,
                    now
                );
            else if (HasPendingShortcut(ImGuiKey.Key1) ||
                     HasPendingShortcut(ImGuiKey.Keypad1))
                SetZoomTarget
                (
                    Math.Clamp(1f, minZoom, maxZoom),
                    canvasCenter,
                    canvasCenter,
                    canvasSize,
                    image.Size,
                    defaultZoom,
                    true,
                    now
                );

            if (canPan                &&
                isCanvasHovered       &&
                !isOverOverlayControl &&
                ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                isDragging = true;

            if (isDragging)
            {
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    var dragDelta = ImGui.GetIO().MouseDelta;

                    if (dragDelta != Vector2.Zero)
                    {
                        targetPan  = ClampPan(targetPan + dragDelta, image.Size, targetZoom, canvasSize);
                        currentPan = targetPan;
                    }
                }
                else
                    isDragging = false;
            }

            var canClickPrevious = isPreviousHovered &&
                                   previewImageIndex > 0;
            var canClickNext = isNextHovered &&
                               previewImageIndex < urls.Count - 1;
            var canDragCanvas = canPan          &&
                                isCanvasHovered &&
                                !isOverOverlayControl;

            if (isDragging)
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
            else if (canClickPrevious ||
                     canClickNext     ||
                     isCloseHovered   ||
                     isZoomHovered    ||
                     canDragCanvas)
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            var smoothFactor = 1f - MathF.Exp(-ZOOM_SMOOTH_SPEED * deltaTime);
            currentZoom = float.Lerp(currentZoom, targetZoom, smoothFactor);
            if (MathF.Abs(currentZoom - targetZoom) < 0.0005f)
                currentZoom = targetZoom;

            if (!isDragging)
            {
                currentPan = Vector2.Lerp(currentPan, targetPan, smoothFactor);
                if (Vector2.DistanceSquared(currentPan, targetPan) < 0.01f)
                    currentPan = targetPan;
            }
        }
        else
        {
            var canClickPrevious = isPreviousHovered &&
                                   previewImageIndex > 0;
            var canClickNext = isNextHovered &&
                               previewImageIndex < urls.Count - 1;

            if (canClickPrevious ||
                canClickNext     ||
                isCloseHovered)
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        var progress = Math.Clamp
        (
            (float)((now - galleryTransitionStart) / GALLERY_TRANSITION_DURATION),
            0f,
            1f
        );
        var alpha = 1f - MathF.Pow(1f - progress, 3f);

        var canvasBgColor = Vector4.Lerp
        (
            ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg],
            new Vector4(0.04f, 0.04f, 0.05f, 1f),
            0.45f
        );
        drawList.AddRectFilled
        (
            canvasMin,
            canvasMax,
            ImGui.GetColorU32(canvasBgColor with { W = 0.85f }),
            canvasRounding
        );

        if (progress < 1f                                                               &&
            !string.IsNullOrEmpty(previousImageURL)                                     &&
            ImageHelper.Instance().TryGetImage(previousImageURL, out var previousImage) &&
            previousImage.Size is { X: > 0f, Y: > 0f })
            DrawClippedImage
            (
                drawList,
                previousImage.Handle,
                previousImage.Size,
                canvasMin,
                canvasMax,
                canvasCenter,
                previousImageZoom,
                previousImagePan,
                canvasRounding,
                1f - alpha
            );

        if (hasImage)
            DrawClippedImage
            (
                drawList,
                image.Handle,
                image.Size,
                canvasMin,
                canvasMax,
                canvasCenter,
                currentZoom,
                currentPan,
                canvasRounding,
                alpha
            );
        else
            DrawLoadingIndicator
            (
                drawList,
                canvasMin,
                canvasSize,
                canvasCenter,
                uiScale,
                now,
                loadingText
            );

        drawList.AddRect
        (
            canvasMin,
            canvasMax,
            ImGui.GetColorU32(ImGuiCol.Border, 0.45f),
            canvasRounding,
            ImDrawFlags.RoundCornersAll,
            1f
        );

        var targetArrowVisibility = isCanvasHovered   ||
                                    isPreviousHovered ||
                                    isNextHovered ?
                                        1f :
                                        0.35f;
        arrowVisibility = float.Lerp(arrowVisibility, targetArrowVisibility, 1f - MathF.Exp(-14f * deltaTime));

        if (hasMultiple)
        {
            DrawCircleButton
            (
                leftArrow,
                arrowRadius,
                FontAwesomeIcon.AngleLeft,
                previewImageIndex == 0,
                isPreviousHovered,
                arrowVisibility
            );
            DrawCircleButton
            (
                rightArrow,
                arrowRadius,
                FontAwesomeIcon.AngleRight,
                previewImageIndex == urls.Count - 1,
                isNextHovered,
                arrowVisibility
            );
        }

        DrawCircleButton
        (
            closeCenter,
            closeRadius,
            FontAwesomeIcon.Times,
            false,
            isCloseHovered,
            0.6f,
            0.72f
        );

        if (hasImage)
        {
            using var badgeFont = FontManager.Instance().UIFont80.Push();

            var snapElapsed = (float)(now - snapFeedbackStart);
            var snapPulse = snapElapsed is >= 0f and < SNAP_FEEDBACK_DURATION ?
                                1f - (snapElapsed / SNAP_FEEDBACK_DURATION) :
                                0f;
            snapPulse *= snapPulse;

            var badgeBgAlpha = isZoomHovered ?
                                   0.92f :
                                   0.78f;
            var badgeBg = ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg] with { W = badgeBgAlpha };
            var baseBorderAlpha = isZoomHovered ?
                                      0.8f :
                                      0.45f;
            var hoverSnapBoost = isSnappedState &&
                                 isZoomHovered ?
                                     0.4f :
                                     0f;
            var badgeBorder = Vector4.Lerp
            (
                ImGui.GetStyle().Colors[(int)ImGuiCol.Border] with { W = baseBorderAlpha },
                ImGuiColors.DalamudOrange with { W = 0.95f },
                MathF.Max(snapPulse, hoverSnapBoost)
            );
            var badgeTextCol = Vector4.Lerp
            (
                ImGui.GetStyle().Colors[(int)ImGuiCol.Text],
                ImGuiColors.DalamudOrange,
                snapPulse * 0.85f
            );

            var pillRounding = (zoomBadgeMax.Y - zoomBadgeMin.Y) / 2f;

            if (snapPulse > 0.01f)
                AddGlowRect
                (
                    drawList,
                    zoomBadgeMin,
                    zoomBadgeMax,
                    ImGui.GetColorU32(ImGuiColors.DalamudOrange with { W = 0.35f * snapPulse }),
                    pillRounding,
                    6f * uiScale * snapPulse,
                    6
                );

            drawList.AddRectFilled
            (
                zoomBadgeMin,
                zoomBadgeMax,
                ImGui.GetColorU32(badgeBg),
                pillRounding
            );
            drawList.AddRect
            (
                zoomBadgeMin,
                zoomBadgeMax,
                ImGui.GetColorU32(badgeBorder),
                pillRounding,
                ImDrawFlags.RoundCornersAll,
                1f
            );

            var badgeHeight = zoomBadgeMax.Y - zoomBadgeMin.Y;
            var iconOrigin  = zoomBadgeMin with { X = zoomBadgeMin.X + pillPadding.X };
            drawList.AddText
            (
                zoomIconMetrics.CenterIn(iconOrigin, zoomIconMetrics.Size with { Y = badgeHeight }),
                ImGui.GetColorU32(badgeTextCol with { W = 0.85f }),
                zoomIconText
            );

            var textOrigin = zoomBadgeMin with { X = iconOrigin.X + zoomIconMetrics.Size.X + pillIconSpacing };
            drawList.AddText
            (
                zoomTextMetrics.CenterIn(textOrigin, zoomTextMetrics.Size with { Y = badgeHeight }),
                ImGui.GetColorU32(badgeTextCol),
                zoomText
            );
        }

        if (hasMultiple)
        {
            var       pageText = $"{previewImageIndex + 1} / {urls.Count}";
            using var pageFont = FontManager.Instance().UIFont80.Push();

            var pageMetrics  = TextMetrics.Measure(pageText);
            var pageMax      = new Vector2(canvasMax.X - spacing, canvasMax.Y - spacing);
            var pageMin      = pageMax - pageMetrics.Size - (pillPadding * 2f);
            var pageRounding = (pageMax.Y - pageMin.Y) / 2f;

            drawList.AddRectFilled
            (
                pageMin,
                pageMax,
                ImGui.GetColorU32(ImGuiCol.WindowBg, 0.78f),
                pageRounding
            );
            drawList.AddRect
            (
                pageMin,
                pageMax,
                ImGui.GetColorU32(ImGuiCol.Border, 0.45f),
                pageRounding,
                ImDrawFlags.RoundCornersAll,
                1f
            );
            drawList.AddText
            (
                pageMetrics.CenterIn(pageMin, pageMax - pageMin),
                ImGui.GetColorU32(ImGuiCol.Text),
                pageText
            );
        }

        ImGui.SetCursorScreenPos(canvasMin);
        ImGui.InvisibleButton("##GalleryWheelArea", gallerySize);
        ImGuiP.SetItemUsingMouseWheel();

        return;

        void DrawCircleButton
        (
            Vector2         center,
            float           radius,
            FontAwesomeIcon icon,
            bool            isDisabled,
            bool            isHovered,
            float           visibility,
            float           iconScale = 1f
        )
        {
            var effectiveAlpha = (isDisabled, isHovered) switch
            {
                (true, _) => 0.28f * visibility,
                (_, true) => 0.96f,
                _         => 0.76f * visibility
            };
            var isActiveHover = isHovered &&
                                !isDisabled;
            var bgCol = isActiveHover ?
                            ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered] with { W = effectiveAlpha } :
                            ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg] with { W = effectiveAlpha };
            var borderCol = isActiveHover ?
                                ImGuiColors.DalamudOrange with { W = 0.85f } :
                                ImGui.GetStyle().Colors[(int)ImGuiCol.Border] with { W = 0.55f * visibility };
            var textAlpha = isActiveHover ?
                                1f :
                                Math.Clamp(visibility + 0.25f, 0f, 1f);
            var textCol = isDisabled ?
                              ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled] with { W = 0.55f * visibility } :
                              ImGui.GetStyle().Colors[(int)ImGuiCol.Text] with { W = textAlpha };

            drawList.AddCircleFilled
            (
                center,
                radius,
                ImGui.GetColorU32(bgCol),
                48
            );
            drawList.AddCircle
            (
                center,
                radius,
                ImGui.GetColorU32(borderCol),
                48,
                1f
            );

            using var font = FontManager.Instance().UIFont90.Push();

            var iconText    = icon.ToIconString();
            var iconMetrics = TextMetrics.Measure(iconText, iconScale);

            drawList.AddText
            (
                ImGui.GetFont(),
                ImGui.GetFontSize() * iconScale,
                iconMetrics.CenterIn(center - new Vector2(radius), new Vector2(radius * 2f)),
                ImGui.GetColorU32(textCol),
                iconText
            );
        }
    }

    private void ToggleQuickZoom
    (
        Vector2 zoomOrigin,
        Vector2 canvasCenter,
        Vector2 canvasSize,
        Vector2 imageSize,
        float   fitZoom,
        float   defaultZoom,
        float   minZoom,
        float   maxZoom,
        double  now
    )
    {
        float nextZoom;

        if (MathF.Abs(targetZoom - defaultZoom) <= ZOOM_EPSILON)
            nextZoom = defaultZoom < 0.95f ?
                           Math.Clamp(1f, minZoom, maxZoom) :
                           Math.Clamp(2f, minZoom, maxZoom);
        else
            nextZoom = defaultZoom;

        if (MathF.Abs(nextZoom - fitZoom)     <= ZOOM_EPSILON &&
            MathF.Abs(fitZoom  - defaultZoom) <= ZOOM_EPSILON)
            nextZoom = defaultZoom;

        SetZoomTarget
        (
            nextZoom,
            zoomOrigin,
            canvasCenter,
            canvasSize,
            imageSize,
            defaultZoom,
            true,
            now
        );
    }

    private void ApplyZoomStep
    (
        float   wheelDelta,
        Vector2 zoomOrigin,
        Vector2 canvasCenter,
        Vector2 canvasSize,
        Vector2 imageSize,
        float   fitZoom,
        float   defaultZoom,
        float   minZoom,
        float   maxZoom,
        double  now
    )
    {
        var direction = wheelDelta > 0f ?
                            1 :
                            -1;

        if (now < snapHoldUntil &&
            isSnappedState      &&
            direction == snapHoldDirection)
        {
            snapDetentAccum += MathF.Abs(wheelDelta);
            if (snapDetentAccum < SNAP_DETENT_ESCAPE)
                return;
        }

        snapDetentAccum   = 0f;
        snapHoldDirection = 0;

        var nextZoom = ComputeNextZoom
        (
            targetZoom,
            wheelDelta,
            fitZoom,
            defaultZoom,
            minZoom,
            maxZoom,
            out var snapped,
            out var strongSnap
        );

        SetZoomTarget
        (
            nextZoom,
            zoomOrigin,
            canvasCenter,
            canvasSize,
            imageSize,
            defaultZoom,
            snapped,
            now
        );

        if (strongSnap)
        {
            snapHoldUntil     = now + SNAP_HOLD_DURATION;
            snapHoldDirection = direction;
        }
    }

    private void SetZoomTarget
    (
        float   nextZoom,
        Vector2 zoomOrigin,
        Vector2 canvasCenter,
        Vector2 canvasSize,
        Vector2 imageSize,
        float   defaultZoom,
        bool    snapped,
        double  now
    )
    {
        var anchorOffset = zoomOrigin - canvasCenter;
        var imagePoint   = (anchorOffset - targetPan) / MathF.Max(targetZoom, ZOOM_MIN_ABSOLUTE);

        targetZoom     = nextZoom;
        isFitLocked    = MathF.Abs(targetZoom - defaultZoom) <= ZOOM_EPSILON;
        isSnappedState = snapped;

        targetPan = isFitLocked ?
                        Vector2.Zero :
                        ClampPan(anchorOffset - (imagePoint * targetZoom), imageSize, targetZoom, canvasSize);

        if (snapped)
            snapFeedbackStart = now;
    }

    private static float ComputeNextZoom
    (
        float    currentTarget,
        float    wheelDelta,
        float    fitZoom,
        float    defaultZoom,
        float    minZoom,
        float    maxZoom,
        out bool snapped,
        out bool strongSnap
    )
    {
        snapped    = false;
        strongSnap = false;

        var rawTarget = Math.Clamp(currentTarget * MathF.Pow(ZOOM_STEP_FACTOR, wheelDelta), minZoom, maxZoom);

        Span<float> snapBuffer = stackalloc float[ZOOM_SNAP_LEVELS.Length + 2];
        var         snapCount  = 0;

        foreach (var level in ZOOM_SNAP_LEVELS)
        {
            if (level < minZoom - ZOOM_EPSILON ||
                level > maxZoom + ZOOM_EPSILON)
                continue;

            if (MathF.Abs(level - 1f)                                              > ZOOM_EPSILON &&
                MathF.Abs(level - fitZoom) / MathF.Max(fitZoom, ZOOM_MIN_ABSOLUTE) < SNAP_MERGE_RATIO)
                continue;

            snapBuffer[snapCount++] = level;
        }

        if (fitZoom                 >= minZoom - ZOOM_EPSILON &&
            fitZoom                 <= maxZoom + ZOOM_EPSILON &&
            MathF.Abs(fitZoom - 1f) > 0.04f)
            snapBuffer[snapCount++] = fitZoom;

        if (MathF.Abs(defaultZoom - fitZoom) > ZOOM_EPSILON            &&
            MathF.Abs(defaultZoom - 1f)      > ZOOM_EPSILON            &&
            defaultZoom                      >= minZoom - ZOOM_EPSILON &&
            defaultZoom                      <= maxZoom + ZOOM_EPSILON)
            snapBuffer[snapCount++] = defaultZoom;

        var candidates = snapBuffer[..snapCount];
        candidates.Sort();

        if (wheelDelta > 0f)
        {
            foreach (var candidate in candidates)
            {
                if (candidate <= currentTarget + ZOOM_EPSILON)
                    continue;

                var crossed = candidate <= rawTarget + ZOOM_EPSILON;
                var magnetic = candidate                           > rawTarget &&
                               (candidate - rawTarget) / candidate <= SNAP_MAGNET_RATIO;
                if (!crossed &&
                    !magnetic)
                    continue;

                snapped = true;
                strongSnap = MathF.Abs(candidate - 1f)          <= ZOOM_EPSILON ||
                             MathF.Abs(candidate - fitZoom)     <= ZOOM_EPSILON ||
                             MathF.Abs(candidate - defaultZoom) <= ZOOM_EPSILON;
                return candidate;
            }
        }
        else
        {
            for (var i = candidates.Length - 1; i >= 0; i--)
            {
                var candidate = candidates[i];
                if (candidate >= currentTarget - ZOOM_EPSILON)
                    continue;

                var crossed = candidate >= rawTarget - ZOOM_EPSILON;
                var magnetic = candidate                           < rawTarget &&
                               (rawTarget - candidate) / candidate <= SNAP_MAGNET_RATIO;
                if (!crossed &&
                    !magnetic)
                    continue;

                snapped = true;
                strongSnap = MathF.Abs(candidate - 1f)          <= ZOOM_EPSILON ||
                             MathF.Abs(candidate - fitZoom)     <= ZOOM_EPSILON ||
                             MathF.Abs(candidate - defaultZoom) <= ZOOM_EPSILON;
                return candidate;
            }
        }

        return rawTarget;
    }

    private static Vector2 ClampPan
    (
        Vector2 pan,
        Vector2 imageSize,
        float   zoom,
        Vector2 canvasSize
    )
    {
        var scaledSize = imageSize * zoom;
        var maxOffsetX = MathF.Max(0f, (scaledSize.X - canvasSize.X) / 2f);
        var maxOffsetY = MathF.Max(0f, (scaledSize.Y - canvasSize.Y) / 2f);

        return new Vector2
        (
            Math.Clamp(pan.X, -maxOffsetX, maxOffsetX),
            Math.Clamp(pan.Y, -maxOffsetY, maxOffsetY)
        );
    }

    private static void DrawClippedImage
    (
        ImDrawListPtr drawList,
        ImTextureID   textureHandle,
        Vector2       imageSize,
        Vector2       canvasMin,
        Vector2       canvasMax,
        Vector2       canvasCenter,
        float         zoom,
        Vector2       pan,
        float         rounding,
        float         alpha
    )
    {
        if (alpha <= 0.001f)
            return;

        var scaledSize = imageSize * zoom;
        var imageMin   = canvasCenter + pan - (scaledSize / 2f);
        var imageMax   = imageMin           + scaledSize;

        var drawMin = Vector2.Max(imageMin, canvasMin);
        var drawMax = Vector2.Min(imageMax, canvasMax);

        if (drawMin.X >= drawMax.X ||
            drawMin.Y >= drawMax.Y)
            return;

        var uvMin = (drawMin - imageMin) / scaledSize;
        var uvMax = (drawMax - imageMin) / scaledSize;

        var isInsideCanvas = imageMin.X >= canvasMin.X + 1f &&
                             imageMin.Y >= canvasMin.Y + 1f &&
                             imageMax.X <= canvasMax.X - 1f &&
                             imageMax.Y <= canvasMax.Y - 1f;

        var imageRounding = isInsideCanvas ?
                                MathF.Min(rounding * 0.75f, MathF.Min(scaledSize.X, scaledSize.Y) * 0.1f) :
                                rounding;

        if (isInsideCanvas)
            drawList.AddRect
            (
                drawMin - Vector2.One,
                drawMax + Vector2.One,
                ImGui.GetColorU32(ImGuiCol.Border, 0.35f * alpha),
                imageRounding + 1f,
                ImDrawFlags.RoundCornersAll,
                1f
            );

        drawList.AddImageRounded
        (
            textureHandle,
            drawMin,
            drawMax,
            uvMin,
            uvMax,
            Vector4.One.WithW(alpha).ToUInt(),
            imageRounding
        );
    }

    private static void DrawLoadingIndicator
    (
        ImDrawListPtr drawList,
        Vector2       canvasMin,
        Vector2       canvasSize,
        Vector2       canvasCenter,
        float         uiScale,
        double        now,
        string?       loadingText
    )
    {
        var hasText          = !string.IsNullOrEmpty(loadingText);
        var spinnerRadius    = 14f  * uiScale;
        var spinnerThickness = 2.5f * uiScale;
        var spinnerCenter = hasText ?
                                canvasCenter - new Vector2(0f, 12f * uiScale) :
                                canvasCenter;
        var startAngle = (float)(now * 5.2);

        drawList.AddCircle
        (
            spinnerCenter,
            spinnerRadius,
            ImGui.GetColorU32(ImGuiCol.Border, 0.35f),
            32,
            spinnerThickness
        );

        drawList.PathClear();
        drawList.PathArcTo
        (
            spinnerCenter,
            spinnerRadius,
            startAngle,
            startAngle + (MathF.PI * 1.25f),
            24
        );
        drawList.PathStroke
        (
            ImGui.GetColorU32(ImGuiColors.DalamudOrange),
            ImDrawFlags.None,
            spinnerThickness
        );

        if (!hasText)
            return;

        using var font           = FontManager.Instance().UIFont80.Push();
        var       loadingMetrics = TextMetrics.Measure(loadingText);
        var       textOrigin     = canvasMin with { Y = spinnerCenter.Y + spinnerRadius + (10f * uiScale) };

        drawList.AddText
        (
            loadingMetrics.CenterIn(textOrigin, new Vector2(canvasSize.X, loadingMetrics.Size.Y)),
            ImGui.GetColorU32(ImGuiCol.TextDisabled),
            loadingText
        );
    }

    private int DrawGalleryThumbnails
    (
        IReadOnlyList<string> urls,
        Vector2               origin,
        float                 width,
        float                 barHeight,
        float                 thumbnailSize,
        float                 spacing,
        int                   currentIndex,
        float                 deltaTime
    )
    {
        var clickedIndex = -1;
        var uiScale      = GlobalUIScale;
        var outline      = GALLERY_THUMBNAIL_OUTLINE  * uiScale;
        var rounding     = GALLERY_THUMBNAIL_ROUNDING * uiScale;
        var itemGap      = spacing                    * 0.7f;
        var totalWidth   = (urls.Count * thumbnailSize) + ((urls.Count - 1) * itemGap);
        var maxScroll    = MathF.Max(0f, totalWidth                    - width + (outline * 4f));
        var barMax       = origin + new Vector2(width, barHeight);
        var drawList     = ImGui.GetWindowDrawList();

        var isBarHovered = ImGui.IsMouseHoveringRect(origin, barMax) &&
                           ImGui.IsWindowHovered();

        if (maxScroll > 0f)
        {
            if (lastThumbnailScrollIndex != currentIndex)
            {
                lastThumbnailScrollIndex = currentIndex;

                var selectedStart = (outline * 2f) + (currentIndex * (thumbnailSize + itemGap));
                var selectedEnd   = selectedStart  + thumbnailSize;

                if (selectedStart < targetThumbnailScrollX)
                    targetThumbnailScrollX = Math.Clamp(selectedStart - itemGap, 0f, maxScroll);
                else if (selectedEnd > targetThumbnailScrollX + width)
                    targetThumbnailScrollX = Math.Clamp(selectedEnd - width + itemGap, 0f, maxScroll);
            }

            if (isBarHovered)
            {
                var wheel = ImGui.GetIO().MouseWheelH + ImGui.GetIO().MouseWheel;
                if (wheel != 0f)
                    targetThumbnailScrollX = Math.Clamp
                    (
                        targetThumbnailScrollX - (wheel * (thumbnailSize + itemGap)),
                        0f,
                        maxScroll
                    );
            }

            thumbnailScrollX = float.Lerp(thumbnailScrollX, targetThumbnailScrollX, 1f - MathF.Exp(-14f * deltaTime));
            if (MathF.Abs(thumbnailScrollX - targetThumbnailScrollX) < 0.25f)
                thumbnailScrollX = targetThumbnailScrollX;
        }
        else
        {
            thumbnailScrollX         = 0f;
            targetThumbnailScrollX   = 0f;
            lastThumbnailScrollIndex = currentIndex;
        }

        var startX = maxScroll > 0f ?
                         origin.X + (outline * 2f) - thumbnailScrollX :
                         origin.X                  + ((width - totalWidth) / 2f);
        var topY = origin.Y + ((barHeight - thumbnailSize) / 2f);

        drawList.PushClipRect(origin, barMax, true);

        for (var i = 0; i < urls.Count; i++)
        {
            var thumbnailMin = new Vector2(startX + (i * (thumbnailSize + itemGap)), topY);
            var thumbnailMax = thumbnailMin + new Vector2(thumbnailSize);

            if (thumbnailMax.X < origin.X ||
                thumbnailMin.X > barMax.X)
                continue;

            var hitMin = Vector2.Max(thumbnailMin, origin);
            var hitMax = Vector2.Min(thumbnailMax, barMax);

            ImGui.SetCursorScreenPos(hitMin);
            ImGui.InvisibleButton($"##GalleryThumbnail{i}", Vector2.Max(Vector2.One, hitMax - hitMin));
            ImGuiP.SetItemUsingMouseWheel();

            var isHovered  = ImGui.IsItemHovered();
            var isSelected = i == currentIndex;

            if (isHovered)
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            if (ImGui.IsItemClicked())
                clickedIndex = i;

            if (isSelected)
                AddGlowRect
                (
                    drawList,
                    thumbnailMin,
                    thumbnailMax,
                    ImGui.GetColorU32(ImGuiColors.DalamudOrange with { W = 0.28f }),
                    rounding,
                    4f * uiScale,
                    5
                );

            if (ImageHelper.Instance().TryGetImage(urls[i], out var thumbnail) &&
                thumbnail.Size is { X: > 0f, Y: > 0f })
            {
                var (uvMin, uvMax) = GetCenterCroppedUV(thumbnail.Size);
                var alpha = (isSelected, isHovered) switch
                {
                    (true, _) => 1f,
                    (_, true) => 0.85f,
                    _         => 0.48f
                };

                drawList.AddImageRounded
                (
                    thumbnail.Handle,
                    thumbnailMin,
                    thumbnailMax,
                    uvMin,
                    uvMax,
                    Vector4.One.WithW(alpha).ToUInt(),
                    rounding
                );
            }
            else
                drawList.AddRectFilled
                (
                    thumbnailMin,
                    thumbnailMax,
                    ImGui.GetColorU32(ImGuiCol.FrameBg, 0.5f),
                    rounding
                );

            if (isSelected)
                drawList.AddRect
                (
                    thumbnailMin - new Vector2(outline * 0.5f),
                    thumbnailMax + new Vector2(outline * 0.5f),
                    ImGui.GetColorU32(ImGuiColors.DalamudOrange),
                    rounding,
                    ImDrawFlags.RoundCornersAll,
                    outline
                );
            else if (isHovered)
                drawList.AddRect
                (
                    thumbnailMin,
                    thumbnailMax,
                    ImGui.GetColorU32(ImGuiCol.Border, 0.85f),
                    rounding,
                    ImDrawFlags.RoundCornersAll,
                    1f
                );
        }

        if (maxScroll > 0f)
        {
            var fadeWidth = 24f * uiScale;
            var edgeColor = ImGui.GetColorU32(ImGuiCol.WindowBg, 0.92f);
            var clearEdge = ImGui.GetColorU32(ImGuiCol.WindowBg, 0f);

            if (thumbnailScrollX > 1f)
                drawList.AddRectFilledMultiColor
                (
                    origin,
                    barMax with { X = origin.X + fadeWidth },
                    edgeColor,
                    clearEdge,
                    clearEdge,
                    edgeColor
                );

            if (thumbnailScrollX < maxScroll - 1f)
                drawList.AddRectFilledMultiColor
                (
                    origin with { X = barMax.X - fadeWidth },
                    barMax,
                    clearEdge,
                    edgeColor,
                    edgeColor,
                    clearEdge
                );
        }

        drawList.PopClipRect();

        return clickedIndex;
    }

    private static (Vector2 Min, Vector2 Max) GetCenterCroppedUV(Vector2 size)
    {
        var aspect = size.X / size.Y;

        if (aspect > 1f)
        {
            var width = 1f / aspect;

            return (new Vector2((1f - width) / 2f, 0f), new Vector2((1f + width) / 2f, 1f));
        }

        return (new Vector2(0f, (1f - aspect) / 2f), new Vector2(1f, (1f + aspect) / 2f));
    }

    private bool HasPendingShortcut(ImGuiKey key)
    {
        foreach (var shortcut in pendingShortcuts)
        {
            if (shortcut.Key == key)
                return true;
        }

        return false;
    }

    private static bool MatchesGalleryShortcut
    (
        KeyboardShortcutEvent shortcut,
        bool                  allowNavigation
    ) =>
        shortcut.Key switch
        {
            ImGuiKey.Equal => shortcut.Modifiers is KeyboardShortcutModifiers.None or KeyboardShortcutModifiers.Shift,
            ImGuiKey.KeypadAdd or
                ImGuiKey.Minus or
                ImGuiKey.KeypadSubtract => shortcut.Modifiers == KeyboardShortcutModifiers.None,
            ImGuiKey.Escape or
                ImGuiKey.Key0 or
                ImGuiKey.Keypad0 or
                ImGuiKey.R or
                ImGuiKey.Key1 or
                ImGuiKey.Keypad1 => shortcut is
                {
                    IsRepeat: false,
                    Modifiers: KeyboardShortcutModifiers.None
                },
            ImGuiKey.LeftArrow or
                ImGuiKey.RightArrow => allowNavigation &&
                                       shortcut.Modifiers == KeyboardShortcutModifiers.None,
            ImGuiKey.Home or
                ImGuiKey.End => allowNavigation &&
                                shortcut is
                                {
                                    IsRepeat: false,
                                    Modifiers: KeyboardShortcutModifiers.None
                                },
            _ => false
        };

    #region 常量

    private static readonly Func<KeyboardShortcutEvent, bool> MATCHES_SINGLE_IMAGE_SHORTCUT =
        static shortcut => MatchesGalleryShortcut(shortcut, false);

    private static readonly Func<KeyboardShortcutEvent, bool> MATCHES_MULTI_IMAGE_SHORTCUT =
        static shortcut => MatchesGalleryShortcut(shortcut, true);

    private const string DEFAULT_POPUP_ID = "OmenImageGalleryPopup";

    private const ImGuiWindowFlags POPUP_WINDOW_FLAGS = ImGuiWindowFlags.NoMove            |
                                                        ImGuiWindowFlags.NoResize          |
                                                        ImGuiWindowFlags.NoScrollbar       |
                                                        ImGuiWindowFlags.NoScrollWithMouse |
                                                        ImGuiWindowFlags.AlwaysAutoResize;

    private const float  GALLERY_ARROW_RADIUS        = 17f;
    private const float  GALLERY_CLOSE_RADIUS        = 10f;
    private const float  GALLERY_CANVAS_ROUNDING     = 8f;
    private const float  GALLERY_WINDOW_ROUNDING     = 10f;
    private const float  GALLERY_MIN_WIDTH           = 420f;
    private const float  GALLERY_MIN_HEIGHT          = 300f;
    private const float  GALLERY_SPACING             = 10f;
    private const float  GALLERY_THUMBNAIL_OUTLINE   = 2f;
    private const float  GALLERY_THUMBNAIL_ROUNDING  = 6f;
    private const float  GALLERY_THUMBNAIL_SIZE      = 48f;
    private const double GALLERY_TRANSITION_DURATION = 0.2;
    private const float  GALLERY_VIEWPORT_MARGIN     = 24f;
    private const float  GALLERY_VIEWPORT_RATIO      = 0.58f;

    private const float  ZOOM_EPSILON           = 0.002f;
    private const float  ZOOM_MAX_BASE          = 8f;
    private const float  ZOOM_MIN_ABSOLUTE      = 0.1f;
    private const float  ZOOM_SMOOTH_SPEED      = 18f;
    private const float  ZOOM_STEP_FACTOR       = 1.2f;
    private const float  SNAP_DETENT_ESCAPE     = 1.5f;
    private const float  SNAP_FEEDBACK_DURATION = 0.45f;
    private const double SNAP_HOLD_DURATION     = 0.14;
    private const float  SNAP_MAGNET_RATIO      = 0.07f;
    private const float  SNAP_MERGE_RATIO       = 0.06f;

    private static readonly float[] ZOOM_SNAP_LEVELS =
    [
        0.25f,
        0.5f,
        0.75f,
        1f,
        1.5f,
        2f,
        3f,
        4f,
        6f,
        8f
    ];

    #endregion
}
