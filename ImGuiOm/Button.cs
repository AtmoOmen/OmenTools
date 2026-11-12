using System.Numerics;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Utility;

namespace OmenTools.ImGuiOm;

/// <summary>提供基于当前 ImGui 字体和样式的布局、绘制及交互控件。</summary>
public static partial class ImGuiOm
{
    private static readonly Dictionary<uint, (float Charge, float FlashTime)> HoldButtonStates = [];

    /// <summary>绘制开关按钮，并在点击时切换状态。</summary>
    /// <param name="id">控件唯一标识。</param>
    /// <param name="value">当前状态，点击后更新。</param>
    /// <param name="height">按钮高度；不大于零时使用当前文本行高。</param>
    /// <param name="bgActiveColor">开启时的打包背景颜色。</param>
    /// <param name="bgColor">关闭时的打包背景颜色。</param>
    /// <returns>状态发生变化时返回真。</returns>
    public static bool ToggleButton
    (
        string   id,
        ref bool value,
        float    height        = 0f,
        uint?    bgActiveColor = null,
        uint?    bgColor       = null
    )
    {
        var resolvedHeight = height > 0f ?
                                 height :
                                 ImGui.GetTextLineHeightWithSpacing();
        var size = new Vector2(resolvedHeight * 1.875f, resolvedHeight);

        var position = ImGui.GetCursorScreenPos();
        var changed  = ImGui.InvisibleButton(id, size);

        if (changed)
            value = !value;

        var colors = ImGui.GetStyle().Colors;
        var backgroundColor = value ?
                                  bgActiveColor ?? ImGui.GetColorU32(ImGuiCol.ButtonActive) :
                                  bgColor       ?? ImGui.GetColorU32(ImGuiCol.FrameBg);

        if (ImGui.IsItemActive())
            backgroundColor = Vector4.Lerp(backgroundColor.ToVector4(), colors[(int)ImGuiCol.Text], 0.24f).ToUInt();
        else if (ImGui.IsItemHovered())
            backgroundColor = Vector4.Lerp(backgroundColor.ToVector4(), colors[(int)ImGuiCol.Text], 0.14f).ToUInt();

        var radius     = size.Y * 0.5f;
        var knobRadius = MathF.Max(radius - (2f * ImGuiHelpers.GlobalScale), 1f);
        var knobCenter = new Vector2
        (
            value ?
                position.X + size.X - radius :
                position.X          + radius,
            position.Y + radius
        );
        var drawList = ImGui.GetWindowDrawList();

        drawList.AddRectFilled(position, position + size, backgroundColor, radius);
        drawList.AddCircleFilled(knobCenter, knobRadius, ImGui.GetColorU32(ImGuiCol.Text));

        return changed;
    }

    /// <summary>使用向量颜色绘制开关按钮，并在点击时切换状态。</summary>
    /// <param name="id">控件唯一标识。</param>
    /// <param name="value">当前状态，点击后更新。</param>
    /// <param name="bgActiveColor">开启时的背景颜色。</param>
    /// <param name="bgColor">关闭时的背景颜色。</param>
    /// <param name="height">按钮高度；不大于零时使用当前文本行高。</param>
    /// <returns>状态发生变化时返回真。</returns>
    public static bool ToggleButton
    (
        string   id,
        ref bool value,
        Vector4? bgActiveColor,
        Vector4? bgColor = null,
        float    height  = 0f
    ) =>
        ToggleButton
        (
            id,
            ref value,
            height,
            bgActiveColor?.ToUInt(),
            bgColor?.ToUInt()
        );

    /// <summary>绘制图片按钮，将图片等比缩放到按钮内容区域内。</summary>
    /// <param name="textureID">图片纹理标识。</param>
    /// <param name="buttonSize">按钮尺寸。</param>
    /// <param name="imageSize">图片原始尺寸。</param>
    /// <param name="uv0">纹理左上角坐标。</param>
    /// <param name="uv1">纹理右下角坐标；默认值表示完整纹理。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonImage
    (
        ImTextureID textureID,
        Vector2     buttonSize,
        Vector2     imageSize,
        Vector2     uv0 = default,
        Vector2     uv1 = default
    )
    {
        if (uv0 == default)
            uv0 = new Vector2(0, 0);
        if (uv1 == default)
            uv1 = new Vector2(1, 1);

        using var id = ImRaii.PushId($"{textureID}");

        var pressed = ImGui.InvisibleButton("##imgbtn", buttonSize);
        var hovered = ImGui.IsItemHovered();
        var held    = ImGui.IsItemActive();

        var min  = ImGui.GetItemRectMin();
        var max  = ImGui.GetItemRectMax();
        var draw = ImGui.GetWindowDrawList();

        var background = ImGuiCol.Button;
        if (held)
            background = ImGuiCol.ButtonActive;
        else if (hovered)
            background = ImGuiCol.ButtonHovered;

        var rounding = ImGui.GetStyle().FrameRounding;
        draw.AddRectFilled(min, max, ImGui.GetColorU32(background), rounding);

        var borderCol  = ImGui.GetColorU32(ImGuiCol.Border);
        var borderSize = ImGui.GetStyle().FrameBorderSize;
        if (borderSize > 0.0f)
            draw.AddRect(min, max, borderCol, rounding);

        var pad        = ImGui.GetStyle().FramePadding;
        var contentMin = min + pad;
        var contentMax = max - pad;
        if (contentMax.X < contentMin.X)
            contentMax.X = contentMin.X;
        if (contentMax.Y < contentMin.Y)
            contentMax.Y = contentMin.Y;

        var contentSize = contentMax - contentMin;

        var horizontalScale = contentSize.X / MathF.Max(1e-6f, imageSize.X);
        var verticalScale   = contentSize.Y / MathF.Max(1e-6f, imageSize.Y);
        var scale           = MathF.Min(horizontalScale, verticalScale);

        var drawSize = imageSize                * scale;
        var offset   = (contentSize - drawSize) * 0.5f;

        var imageMin = contentMin + offset;
        var imageMax = imageMin   + drawSize;

        draw.AddImage(textureID, imageMin, imageMax, uv0, uv1, ImGui.GetColorU32(Vector4.One));

        return pressed;
    }

    /// <summary>绘制正方形图标按钮，按可见字形边界居中图标。</summary>
    /// <param name="id">控件唯一标识。</param>
    /// <param name="icon">按钮图标。</param>
    /// <param name="tooltip">悬停提示；为空时不显示。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonIcon
    (
        string          id,
        FontAwesomeIcon icon,
        string          tooltip = ""
    )
    {
        using var idPush = ImRaii.PushId($"{id}_{icon}");

        var iconText = icon.ToIconString();
        return TextMetrics.Measure(iconText).DrawButton(string.Empty, iconText, tooltip);
    }

    /// <summary>绘制指定尺寸的图标按钮，按可见字形边界居中图标。</summary>
    /// <param name="id">控件唯一标识。</param>
    /// <param name="icon">按钮图标。</param>
    /// <param name="buttonSize">按钮尺寸。</param>
    /// <param name="tooltip">悬停提示；为空时不显示。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonIcon
    (
        string          id,
        FontAwesomeIcon icon,
        Vector2         buttonSize,
        string          tooltip = ""
    )
    {
        using var idPush = ImRaii.PushId($"{id}_{icon}");

        var iconText = icon.ToIconString();
        return TextMetrics.Measure(iconText).DrawButton(string.Empty, iconText, tooltip, buttonSize);
    }

    /// <summary>绘制图标在上、文字在下的按钮，尺寸按内容和样式计算。</summary>
    /// <param name="icon">按钮图标。</param>
    /// <param name="text">按钮标签；双井号后的部分仅参与控件标识。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonIconWithTextVertical
    (
        FontAwesomeIcon icon,
        string          text
    )
        => ButtonWithIcon(icon, text, null, true);

    /// <summary>在指定尺寸的按钮中垂直排列并居中图标和文字。</summary>
    /// <param name="icon">按钮图标。</param>
    /// <param name="text">按钮标签；双井号后的部分仅参与控件标识。</param>
    /// <param name="buttonSize">按钮尺寸。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonIconWithTextVertical
    (
        FontAwesomeIcon icon,
        string          text,
        Vector2         buttonSize
    )
        => ButtonWithIcon(icon, text, buttonSize, true);

    /// <summary>绘制图标在左、文字在右的按钮，尺寸按内容和样式计算。</summary>
    /// <param name="icon">按钮图标。</param>
    /// <param name="text">按钮标签；双井号后的部分仅参与控件标识。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonIconWithText
    (
        FontAwesomeIcon icon,
        string          text
    )
        => ButtonWithIcon(icon, text, null, false);

    /// <summary>在指定尺寸的按钮中水平排列并居中图标和文字。</summary>
    /// <param name="icon">按钮图标。</param>
    /// <param name="text">按钮标签；双井号后的部分仅参与控件标识。</param>
    /// <param name="buttonSize">按钮尺寸。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonIconWithText
    (
        FontAwesomeIcon icon,
        string          text,
        Vector2         buttonSize
    )
        => ButtonWithIcon(icon, text, buttonSize, false);

    private static bool ButtonWithIcon(FontAwesomeIcon icon, string text, Vector2? requestedSize, bool vertical)
    {
        var       iconText = icon.ToIconString();
        using var idPush   = ImRaii.PushId($"{text}_{iconText}");

        var displaySpan = GetDisplaySpan(text);
        var iconMetrics = TextMetrics.Measure(iconText);
        var textMetrics = TextMetrics.Measure(displaySpan);
        var style       = ImGui.GetStyle();
        var spacing = vertical ?
                          3f * ImGuiHelpers.GlobalScale :
                          style.ItemSpacing.X;
        var contentSize = vertical ?
                              new Vector2(MathF.Max(iconMetrics.Size.X, textMetrics.Size.X), iconMetrics.Size.Y + spacing + textMetrics.Size.Y) :
                              new Vector2(iconMetrics.Size.X + spacing + textMetrics.Size.X, MathF.Max(iconMetrics.Size.Y, textMetrics.Size.Y));
        var minimumHeight = vertical ?
                                GetDoubleLineHeight() :
                                GetSingleLineHeight();
        var buttonSize = requestedSize ??
                         new Vector2(contentSize.X + (2f * style.FramePadding.X), MathF.Max(minimumHeight, contentSize.Y + (2f * style.FramePadding.Y)));
        var result = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, availableSize) = GetButtonContentRect();
        var (iconPosition, textPosition) = vertical ?
                                               GetVerticalLayout(contentMin, availableSize, iconMetrics.Size, textMetrics.Size, spacing) :
                                               GetHorizontalLayout(contentMin, availableSize, iconMetrics.Size, textMetrics.Size, spacing);
        var drawList = ImGui.GetWindowDrawList();
        var color    = ImGui.GetColorU32(ImGuiCol.Text);
        drawList.AddText(iconPosition - new Vector2(0, iconMetrics.OffsetY), color, iconText);
        drawList.AddText(textPosition - new Vector2(0, textMetrics.OffsetY), color, displaySpan);

        return result;
    }

    /// <summary>绘制图片在上、文字在下的按钮，图片保持原始尺寸。</summary>
    /// <param name="icon">图片纹理。</param>
    /// <param name="text">按钮标签；双井号后的部分仅参与控件标识。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonImageWithTextVertical
    (
        IDalamudTextureWrap icon,
        string              text
    )
    {
        using var idPush = ImRaii.PushId($"{text}_{icon.Handle}");

        var iconSize      = icon.Size;
        var displaySpan   = GetDisplaySpan(text);
        var textMetrics   = TextMetrics.Measure(displaySpan);
        var textSize      = textMetrics.Size;
        var padding       = ImGui.GetStyle().FramePadding.X;
        var spacing       = 3f * ImGuiHelpers.GlobalScale;
        var contentHeight = iconSize.Y + spacing + MathF.Max(textSize.Y, textMetrics.LineAdvance);
        var buttonSize = new Vector2
            (MathF.Max(iconSize.X, textSize.X) + (padding * 2), MathF.Max(GetDoubleLineHeight(), contentHeight + (2f * ImGui.GetStyle().FramePadding.Y)));
        var result = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetVerticalLayout(contentMin, contentSize, iconSize, textSize, spacing);
        var windowDrawList = ImGui.GetWindowDrawList();

        windowDrawList.AddImage(icon.Handle, iconPos, iconPos + iconSize);
        windowDrawList.AddText(textPos                        - new Vector2(0, textMetrics.OffsetY), ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

    /// <summary>在指定尺寸的按钮中垂直排列并居中图片和文字。</summary>
    /// <param name="icon">图片纹理，保持原始尺寸。</param>
    /// <param name="text">按钮标签；双井号后的部分仅参与控件标识。</param>
    /// <param name="buttonSize">按钮尺寸。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonImageWithTextVertical
    (
        IDalamudTextureWrap icon,
        string              text,
        Vector2             buttonSize
    )
    {
        using var idPush = ImRaii.PushId($"{text}_{icon.Handle}");

        var iconSize    = icon.Size;
        var displaySpan = GetDisplaySpan(text);
        var textMetrics = TextMetrics.Measure(displaySpan);
        var (textSize, _) = textMetrics;
        var spacing = 3f * ImGuiHelpers.GlobalScale;
        var result  = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetVerticalLayout(contentMin, contentSize, iconSize, textSize, spacing);
        var windowDrawList = ImGui.GetWindowDrawList();

        windowDrawList.AddImage(icon.Handle, iconPos, iconPos + iconSize);
        windowDrawList.AddText(textPos                        - new Vector2(0, textMetrics.OffsetY), ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

    /// <summary>绘制占满可用宽度、使用选择项背景样式的图标按钮。</summary>
    /// <param name="id">控件唯一标识。</param>
    /// <param name="icon">按钮图标。</param>
    /// <param name="tooltip">悬停提示；为空时不显示。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonIconSelectable
    (
        string          id,
        FontAwesomeIcon icon,
        string          tooltip = ""
    )
    {
        using var idPush = ImRaii.PushId(id);

        var colors   = ImGui.GetStyle().Colors;
        var iconText = icon.ToIconString();
        var size     = new Vector2(ImGui.GetContentRegionAvail().X, GetSingleLineHeight());

        using var colorPush = ImRaii.PushColor(ImGuiCol.ButtonActive, colors[(int)ImGuiCol.HeaderActive].ToUInt())
                                    .Push(ImGuiCol.ButtonHovered, colors[(int)ImGuiCol.HeaderHovered].ToUInt())
                                    .Push(ImGuiCol.Button,        0);

        var result = TextMetrics.Measure(iconText).DrawButton($"{iconText}-{id}", iconText, availableSize: size);

        if (!tooltip.IsNullOrEmpty())
            TooltipHover(tooltip);

        return result;
    }

    /// <summary>绘制占满可用宽度、使用选择项背景样式的文字按钮。</summary>
    /// <param name="text">按钮标签。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonSelectable
    (
        string text
    )
    {
        var style    = ImGui.GetStyle();
        var padding  = style.FramePadding;
        var colors   = style.Colors;
        var textSize = TextMetrics.Measure(GetDisplaySpan(text)).Size;

        var size = new Vector2(MathF.Max(ImGui.GetContentRegionAvail().X, textSize.X + (2 * padding.X)), GetSingleLineHeight());

        using var colorPush = ImRaii.PushColor(ImGuiCol.ButtonActive, colors[(int)ImGuiCol.HeaderActive].ToUInt())
                                    .Push(ImGuiCol.ButtonHovered, colors[(int)ImGuiCol.HeaderHovered].ToUInt())
                                    .Push(ImGuiCol.Button,        0);

        var result = ImGui.Button(text, size);

        return result;
    }

    /// <summary>绘制占满当前可用宽度的按钮。</summary>
    /// <param name="text">按钮标签。</param>
    /// <returns>按钮被点击时返回真。</returns>
    public static bool ButtonStretch
    (
        string text
    )
    {
        var size   = new Vector2(ImGui.GetContentRegionAvail().X, GetSingleLineHeight());
        var result = ImGui.Button(text, size);

        return result;
    }

    /// <summary>绘制持续按住后触发的按钮，并使用标签作为状态标识。</summary>
    /// <param name="text">按钮标签。</param>
    /// <param name="autoReset">触发后是否清空蓄力。</param>
    /// <returns>蓄力完成且闪烁结束时返回真。</returns>
    public static bool HoldButton
    (
        string text,
        bool   autoReset = true
    ) =>
        HoldButton(text, text, autoReset);

    /// <summary>绘制指定尺寸和蓄力时长的长按按钮。</summary>
    /// <param name="text">按钮标签及状态标识。</param>
    /// <param name="autoReset">触发后是否清空蓄力。</param>
    /// <param name="size">按钮尺寸；非正分量由内容和样式计算。</param>
    /// <param name="duration">完成蓄力所需秒数。</param>
    /// <returns>蓄力完成且闪烁结束时返回真。</returns>
    public static bool HoldButton
    (
        string  text,
        bool    autoReset,
        Vector2 size,
        float   duration = 1f
    ) =>
        HoldButton(text, text, autoReset, size, duration);

    /// <summary>使用向量颜色绘制具有独立标识的长按按钮。</summary>
    /// <param name="id">控件唯一标识。</param>
    /// <param name="text">按钮标签。</param>
    /// <param name="chargeColor">蓄力边框颜色。</param>
    /// <param name="autoReset">触发后是否清空蓄力。</param>
    /// <param name="size">按钮尺寸；非正分量由内容和样式计算。</param>
    /// <param name="duration">完成蓄力所需秒数。</param>
    /// <returns>蓄力完成且闪烁结束时返回真。</returns>
    public static bool HoldButton
    (
        string   id,
        string   text,
        Vector4? chargeColor,
        bool     autoReset = true,
        Vector2  size      = default,
        float    duration  = 1f
    ) =>
        HoldButton(id, text, autoReset, size, duration, chargeColor?.ToUInt());

    /// <summary>绘制具有蓄力边框和完成闪烁效果的长按按钮。</summary>
    /// <param name="id">控件唯一标识。</param>
    /// <param name="text">按钮标签；双井号后的部分不显示。</param>
    /// <param name="autoReset">触发后是否清空蓄力。</param>
    /// <param name="size">按钮尺寸；非正分量由内容和样式计算。</param>
    /// <param name="duration">完成蓄力所需秒数；非正值按一秒处理。</param>
    /// <param name="chargeColor">蓄力边框的打包颜色；未指定时使用当前勾选颜色。</param>
    /// <returns>蓄力完成且闪烁结束时返回真。</returns>
    public static bool HoldButton
    (
        string  id,
        string  text,
        bool    autoReset   = true,
        Vector2 size        = default,
        float   duration    = 1f,
        uint?   chargeColor = null
    )
    {
        using var idPush  = ImRaii.PushId(id);
        var       stateID = ImGui.GetID("##hold");
        var (charge, flashTime) = HoldButtonStates.GetValueOrDefault(stateID);

        var displaySpan = GetDisplaySpan(text);
        var textMetrics = TextMetrics.Measure(displaySpan);
        var textSize    = textMetrics.Size;
        var padding     = ImGui.GetStyle().FramePadding;
        var buttonSize = new Vector2
        (
            size.X > 0f ?
                size.X :
                textSize.X + (padding.X * 2f),
            size.Y > 0f ?
                size.Y :
                GetSingleLineHeight()
        );

        ImGui.InvisibleButton("##hold", buttonSize);
        var held    = ImGui.IsItemActive();
        var hovered = ImGui.IsItemHovered();

        var effectiveDuration = duration > 0f ?
                                    duration :
                                    1f;
        var chargeRate = 1f         / effectiveDuration;
        var decayRate  = chargeRate * 1.5f;

        var completed = false;

        if (flashTime > 0f)
        {
            flashTime -= ImGui.GetIO().DeltaTime;
            charge    =  1f;

            if (flashTime <= 0f)
            {
                flashTime = 0f;
                completed = true;
                if (autoReset)
                    charge = 0f;
            }
        }
        else
        {
            var previousCharge = charge;
            charge = held switch
            {
                true when autoReset || charge < 1f => MathF.Min(charge + (chargeRate * ImGui.GetIO().DeltaTime), 1f),
                false                              => MathF.Max(charge - (decayRate  * ImGui.GetIO().DeltaTime), 0f),
                _                                  => charge
            };
            if (held && charge >= 1f && (autoReset || previousCharge < 1f))
                flashTime = HOLD_BUTTON_FLASH_DURATION;
        }

        if (charge > 0f || flashTime > 0f)
            HoldButtonStates[stateID] = (charge, flashTime);
        else
            HoldButtonStates.Remove(stateID);

        var min            = ImGui.GetItemRectMin();
        var max            = ImGui.GetItemRectMax();
        var buttonRectSize = max - min;
        var draw           = ImGui.GetWindowDrawList();
        var style          = ImGui.GetStyle();
        var colors         = style.Colors;
        var rounding       = style.FrameRounding;

        var isFlashing = flashTime > 0f;
        var flashRatio = isFlashing ?
                             flashTime / HOLD_BUTTON_FLASH_DURATION :
                             0f;

        var bgCol = ImGui.GetColorU32(ImGuiCol.Button);
        if (held || isFlashing)
            bgCol = ImGui.GetColorU32(Vector4.Lerp(colors[(int)ImGuiCol.ButtonHovered], colors[(int)ImGuiCol.ButtonActive], 0.5f + (0.5f * charge)));
        else if (hovered)
            bgCol = ImGui.GetColorU32(ImGuiCol.ButtonHovered);

        draw.AddRectFilled(min, max, bgCol, rounding);

        var borderThickness = style.FrameBorderSize > 0f ?
                                  style.FrameBorderSize :
                                  1f;
        if (style.FrameBorderSize > 0f || held || hovered || isFlashing)
            draw.AddRect(min, max, colors[(int)ImGuiCol.Border].ToUInt(), rounding, ImDrawFlags.None, borderThickness);

        if (charge > 0f)
        {
            var activeCol = chargeColor ?? ImGui.GetColorU32(ImGuiCol.CheckMark);

            if (isFlashing)
            {
                var highlightFactor = MathF.Pow(flashRatio, 0.5f);
                var activeVec       = activeCol.ToVector4();
                var highlightVec    = Vector4.Lerp(activeVec, new Vector4(1f, 1f, 1f, 1f), 0.6f * highlightFactor);
                activeCol = highlightVec.ToUInt();
                AddGlowRect(draw, min, max, activeCol, rounding, 5f * ImGuiHelpers.GlobalScale * highlightFactor, 4);
            }

            var chargeThickness = MathF.Max(borderThickness * 1.5f, 2f * ImGuiHelpers.GlobalScale);
            if (isFlashing)
                chargeThickness += 1.5f * ImGuiHelpers.GlobalScale * MathF.Pow(flashRatio, 0.5f);

            DrawHoldProgressBorder(draw, min, max, rounding, charge, activeCol, chargeThickness);
        }

        var textPos = textMetrics.CenterIn(min, buttonRectSize);
        draw.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return completed;
    }

    private static void DrawHoldProgressBorder
    (
        ImDrawListPtr draw,
        Vector2       min,
        Vector2       max,
        float         rounding,
        float         charge,
        uint          chargeColor,
        float         thickness
    )
    {
        switch (charge)
        {
            case <= 0f:
                return;
            case >= 1f:
                draw.AddRect(min, max, chargeColor, rounding, ImDrawFlags.None, thickness);
                return;
        }

        var size = max - min;
        var r    = MathF.Max(0f, MathF.Min(rounding, MathF.Min(size.X * 0.5f, size.Y * 0.5f)));

        var straightW    = MathF.Max(0f, size.X - (2f * r));
        var straightH    = MathF.Max(0f, size.Y - (2f * r));
        var verticalHalf = straightH * 0.5f;
        var arcLen = r > 0f ?
                         r * (MathF.PI * 0.5f) :
                         0f;
        var branchTotalLength = verticalHalf + arcLen + straightW + arcLen + verticalHalf;

        if (branchTotalLength <= 0f) return;

        var progress = charge * branchTotalLength;
        var startPos = min with { Y = min.Y + (size.Y * 0.5f) };

        var remTop = progress;
        draw.PathClear();
        draw.PathLineTo(startPos);

        if (remTop <= verticalHalf)
        {
            draw.PathLineTo(min with { Y = startPos.Y - remTop });
            draw.PathStroke(chargeColor, ImDrawFlags.None, thickness);
        }
        else
        {
            remTop -= verticalHalf;

            if (r > 0f)
            {
                var centerTL = new Vector2(min.X + r, min.Y + r);

                if (remTop <= arcLen)
                {
                    var angle = MathF.PI + (remTop / r);
                    var segs  = Math.Clamp((int)(16 * (remTop / arcLen)), 4, 16);
                    draw.PathArcTo(centerTL, r, MathF.PI, angle, segs);
                    draw.PathStroke(chargeColor, ImDrawFlags.None, thickness);
                    remTop = 0f;
                }
                else
                {
                    draw.PathArcTo(centerTL, r, MathF.PI, MathF.PI * 1.5f, 16);
                    remTop -= arcLen;
                }
            }

            if (remTop > 0f)
            {
                if (remTop <= straightW)
                {
                    draw.PathLineTo(min with { X = min.X + r + remTop });
                    draw.PathStroke(chargeColor, ImDrawFlags.None, thickness);
                }
                else
                {
                    remTop -= straightW;

                    if (r > 0f)
                    {
                        var centerTR = new Vector2(max.X - r, min.Y + r);

                        if (remTop <= arcLen)
                        {
                            var angle = (MathF.PI * 1.5f) + (remTop / r);
                            var segs  = Math.Clamp((int)(16 * (remTop / arcLen)), 4, 16);
                            draw.PathArcTo(centerTR, r, MathF.PI * 1.5f, angle, segs);
                            draw.PathStroke(chargeColor, ImDrawFlags.None, thickness);
                            remTop = 0f;
                        }
                        else
                        {
                            draw.PathArcTo(centerTR, r, MathF.PI * 1.5f, MathF.PI * 2f, 16);
                            remTop -= arcLen;
                        }
                    }

                    if (remTop > 0f)
                    {
                        draw.PathLineTo(max with { Y = min.Y + r + MathF.Min(remTop, verticalHalf) });
                        draw.PathStroke(chargeColor, ImDrawFlags.None, thickness);
                    }
                }
            }
        }

        var remBottom = progress;
        draw.PathClear();
        draw.PathLineTo(startPos);

        if (remBottom <= verticalHalf)
        {
            draw.PathLineTo(min with { Y = startPos.Y + remBottom });
            draw.PathStroke(chargeColor, ImDrawFlags.None, thickness);
        }
        else
        {
            remBottom -= verticalHalf;

            if (r > 0f)
            {
                var centerBL = new Vector2(min.X + r, max.Y - r);

                if (remBottom <= arcLen)
                {
                    var angle = MathF.PI - (remBottom / r);
                    var segs  = Math.Clamp((int)(16 * (remBottom / arcLen)), 4, 16);
                    draw.PathArcTo(centerBL, r, MathF.PI, angle, segs);
                    draw.PathStroke(chargeColor, ImDrawFlags.None, thickness);
                    remBottom = 0f;
                }
                else
                {
                    draw.PathArcTo(centerBL, r, MathF.PI, MathF.PI * 0.5f, 16);
                    remBottom -= arcLen;
                }
            }

            if (remBottom > 0f)
            {
                if (remBottom <= straightW)
                {
                    draw.PathLineTo(max with { X = min.X + r + remBottom });
                    draw.PathStroke(chargeColor, ImDrawFlags.None, thickness);
                }
                else
                {
                    remBottom -= straightW;

                    if (r > 0f)
                    {
                        var centerBR = new Vector2(max.X - r, max.Y - r);

                        if (remBottom <= arcLen)
                        {
                            var angle = (MathF.PI * 0.5f) - (remBottom / r);
                            var segs  = Math.Clamp((int)(16 * (remBottom / arcLen)), 4, 16);
                            draw.PathArcTo(centerBR, r, MathF.PI * 0.5f, angle, segs);
                            draw.PathStroke(chargeColor, ImDrawFlags.None, thickness);
                            remBottom = 0f;
                        }
                        else
                        {
                            draw.PathArcTo(centerBR, r, MathF.PI * 0.5f, 0f, 16);
                            remBottom -= arcLen;
                        }
                    }

                    if (remBottom > 0f)
                    {
                        draw.PathLineTo(max with { Y = max.Y - r - MathF.Min(remBottom, verticalHalf) });
                        draw.PathStroke(chargeColor, ImDrawFlags.None, thickness);
                    }
                }
            }
        }
    }

    #region 常量

    private const float HOLD_BUTTON_FLASH_DURATION = 0.5f;

    #endregion
}
