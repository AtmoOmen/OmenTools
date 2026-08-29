using System.Numerics;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Utility;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    private const float HOLD_BUTTON_FLASH_DURATION = 0.5f;

    private static readonly Dictionary<string, (float Charge, float FlashTime)> HoldButtonStates = [];

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

        var bgCol = held ? ImGui.GetColorU32(ImGuiCol.ButtonActive) : hovered ? ImGui.GetColorU32(ImGuiCol.ButtonHovered) : ImGui.GetColorU32(ImGuiCol.Button);

        var rounding = ImGui.GetStyle().FrameRounding;
        draw.AddRectFilled(min, max, bgCol, rounding);

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

        var imgDrawMin = contentMin;
        var imgDrawMax = contentMax;

        var sx = contentSize.X / MathF.Max(1e-6f, imageSize.X);
        var sy = contentSize.Y / MathF.Max(1e-6f, imageSize.Y);
        var s  = MathF.Min(sx, sy);

        var drawSize = imageSize                * s;
        var offset   = (contentSize - drawSize) * 0.5f;

        imgDrawMin = contentMin + offset;
        imgDrawMax = imgDrawMin + drawSize;

        draw.AddImage(textureID, imgDrawMin, imgDrawMax, uv0, uv1, 0xFFFFFFFF);

        return pressed;
    }

    public static bool ButtonIcon
    (
        string          id,
        FontAwesomeIcon icon,
        string          tooltip       = "",
        bool            useStaticFont = false
    )
    {
        using var idPush = ImRaii.PushId($"{id}_{icon}");

        var iconText   = icon.ToIconString();
        var iconSize   = CalcIconSize(iconText, useStaticFont);
        var buttonSize = new Vector2(GetSingleLineHeight());
        var result     = ImGui.Button(string.Empty, buttonSize);
        var (min, size) = GetItemRect();
        var iconPos = GetCenteredPosition(min, size, iconSize);

        DrawIconText(iconPos, iconText, useStaticFont);

        if (!tooltip.IsNullOrEmpty())
            TooltipHover(tooltip);

        return result;
    }

    public static bool ButtonIcon
    (
        string          id,
        FontAwesomeIcon icon,
        Vector2         buttonSize,
        string          tooltip       = "",
        bool            useStaticFont = false
    )
    {
        using var idPush = ImRaii.PushId($"{id}_{icon}");

        var iconText = icon.ToIconString();
        var iconSize = CalcIconSize(iconText, useStaticFont);
        var result   = ImGui.Button(string.Empty, buttonSize);
        var (min, size) = GetItemRect();
        var iconPos = GetCenteredPosition(min, size, iconSize);

        DrawIconText(iconPos, iconText, useStaticFont);

        if (!tooltip.IsNullOrEmpty())
            TooltipHover(tooltip);

        return result;
    }

    public static bool ButtonIconWithTextVertical
    (
        FontAwesomeIcon icon,
        string          text,
        bool            useStaticFont = false
    )
    {
        using var idPush = ImRaii.PushId($"{text}_{icon.ToIconString()}");

        var iconText    = icon.ToIconString();
        var iconSize    = CalcIconSize(iconText, useStaticFont);
        var displaySpan = GetDisplaySpan(text);
        var textSize    = ImGui.CalcTextSize(displaySpan);
        var padding     = ImGui.GetStyle().FramePadding.X;
        var spacing     = 3f * ImGuiHelpers.GlobalScale;
        var buttonSize  = new Vector2(MathF.Max(iconSize.X, textSize.X) + (padding * 2), GetDoubleLineHeight());
        var result      = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetVerticalLayout(contentMin, contentSize, iconSize, textSize, spacing);

        DrawIconText(iconPos, iconText, useStaticFont);
        ImGui.GetWindowDrawList().AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

    public static bool ButtonIconWithTextVertical
    (
        FontAwesomeIcon icon,
        string          text,
        Vector2         buttonSize,
        bool            useStaticFont = false
    )
    {
        using var idPush = ImRaii.PushId($"{text}_{icon.ToIconString()}");

        var iconText    = icon.ToIconString();
        var iconSize    = CalcIconSize(iconText, useStaticFont);
        var displaySpan = GetDisplaySpan(text);
        var textSize    = ImGui.CalcTextSize(displaySpan);
        var spacing     = 3f * ImGuiHelpers.GlobalScale;
        var result      = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetVerticalLayout(contentMin, contentSize, iconSize, textSize, spacing);

        DrawIconText(iconPos, iconText, useStaticFont);
        ImGui.GetWindowDrawList().AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

    public static bool ButtonIconWithText
    (
        FontAwesomeIcon icon,
        string          text,
        bool            useStaticFont = false
    )
    {
        using var idPush = ImRaii.PushId($"{text}_{icon.ToIconString()}");

        var iconText    = icon.ToIconString();
        var iconSize    = CalcIconSize(iconText, useStaticFont);
        var displaySpan = GetDisplaySpan(text);
        var textSize    = ImGui.CalcTextSize(displaySpan);
        var padding     = ImGui.GetStyle().FramePadding;
        var spacing     = ImGui.GetStyle().ItemSpacing.X;
        var buttonSize  = new Vector2(iconSize.X + textSize.X + (padding.X * 2) + spacing, GetSingleLineHeight());
        var result      = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetHorizontalLayout(contentMin, contentSize, iconSize, textSize, spacing);

        DrawIconText(iconPos, iconText, useStaticFont);
        ImGui.GetWindowDrawList().AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

    public static bool ButtonIconWithText
    (
        FontAwesomeIcon icon,
        string          text,
        Vector2         buttonSize,
        bool            useStaticFont = false
    )
    {
        using var idPush = ImRaii.PushId($"{text}_{icon.ToIconString()}");

        var iconText    = icon.ToIconString();
        var iconSize    = CalcIconSize(iconText, useStaticFont);
        var displaySpan = GetDisplaySpan(text);
        var textSize    = ImGui.CalcTextSize(displaySpan);
        var spacing     = ImGui.GetStyle().ItemSpacing.X;
        var result      = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetHorizontalLayout(contentMin, contentSize, iconSize, textSize, spacing);

        DrawIconText(iconPos, iconText, useStaticFont);
        ImGui.GetWindowDrawList().AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

    public static bool ButtonImageWithTextVertical
    (
        IDalamudTextureWrap icon,
        string              text
    )
    {
        using var idPush = ImRaii.PushId($"{text}_{icon.Handle}");

        var iconSize    = icon.Size;
        var displaySpan = GetDisplaySpan(text);
        var textSize    = ImGui.CalcTextSize(displaySpan);
        var padding     = ImGui.GetStyle().FramePadding.X;
        var spacing     = 3f * ImGuiHelpers.GlobalScale;
        var buttonSize  = new Vector2(MathF.Max(iconSize.X, textSize.X) + (padding * 2), GetDoubleLineHeight());
        var result      = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetVerticalLayout(contentMin, contentSize, iconSize, textSize, spacing);
        var windowDrawList = ImGui.GetWindowDrawList();

        windowDrawList.AddImage(icon.Handle, iconPos, iconPos + iconSize);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

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
        var textSize    = ImGui.CalcTextSize(displaySpan);
        var spacing     = 3f * ImGuiHelpers.GlobalScale;
        var result      = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetVerticalLayout(contentMin, contentSize, iconSize, textSize, spacing);
        var windowDrawList = ImGui.GetWindowDrawList();

        windowDrawList.AddImage(icon.Handle, iconPos, iconPos + iconSize);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

    public static bool ButtonIconSelectable
    (
        string          id,
        FontAwesomeIcon icon,
        string          tooltip       = "",
        bool            useStaticFont = false
    )
    {
        using var idPush = ImRaii.PushId(id);

        var colors   = ImGui.GetStyle().Colors;
        var iconText = icon.ToIconString();
        var size     = new Vector2(ImGui.GetContentRegionAvail().X, GetSingleLineHeight());

        using var colorPush = ImRaii.PushColor(ImGuiCol.ButtonActive, colors[(int)ImGuiCol.HeaderActive].ToUInt())
                                    .Push(ImGuiCol.ButtonHovered, colors[(int)ImGuiCol.HeaderHovered].ToUInt())
                                    .Push(ImGuiCol.Button,        0);

        bool result;
        using (ImRaii.PushFont(UiBuilder.IconFont, useStaticFont))
            result = ImGui.Button($"{iconText}##{iconText}-{id}", size);

        if (!tooltip.IsNullOrEmpty())
            TooltipHover(tooltip);

        return result;
    }

    public static bool ButtonSelectable
    (
        string text
    )
    {
        var style    = ImGui.GetStyle();
        var padding  = style.FramePadding;
        var colors   = style.Colors;
        var textSize = ImGui.CalcTextSize(text);

        var size = new Vector2(MathF.Max(ImGui.GetContentRegionAvail().X, textSize.X + (2 * padding.X)), GetSingleLineHeight());

        using var colorPush = ImRaii.PushColor(ImGuiCol.ButtonActive, colors[(int)ImGuiCol.HeaderActive].ToUInt())
                                    .Push(ImGuiCol.ButtonHovered, colors[(int)ImGuiCol.HeaderHovered].ToUInt())
                                    .Push(ImGuiCol.Button,        0);

        var result = ImGui.Button(text, size);

        return result;
    }

    public static bool ButtonStretch
    (
        string text
    )
    {
        var size   = new Vector2(ImGui.GetContentRegionAvail().X, GetSingleLineHeight());
        var result = ImGui.Button(text, size);

        return result;
    }

    public static bool HoldButton
    (
        string text,
        bool   autoReset = true
    ) =>
        HoldButton(text, text, autoReset);

    public static bool HoldButton
    (
        string  text,
        bool    autoReset,
        Vector2 size,
        float   duration = 1f
    ) =>
        HoldButton(text, text, autoReset, size, duration);

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
        var (charge, flashTime) = HoldButtonStates.GetValueOrDefault(id);

        var displaySpan = GetDisplaySpan(text);
        var textSize    = ImGui.CalcTextSize(displaySpan);
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

        using var idPush = ImRaii.PushId(id);
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
        else if (held)
        {
            charge = MathF.Min(charge + (chargeRate * ImGui.GetIO().DeltaTime), 1f);
            if (charge >= 1f)
                flashTime = HOLD_BUTTON_FLASH_DURATION;
        }
        else
            charge = MathF.Max(charge - (decayRate * ImGui.GetIO().DeltaTime), 0f);

        HoldButtonStates[id] = (charge, flashTime);

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

        var bgCol = held || isFlashing ?
                        Vector4.Lerp(colors[(int)ImGuiCol.ButtonHovered], colors[(int)ImGuiCol.ButtonActive], 0.5f + (0.5f * charge)).ToUInt() :
                        hovered ?
                            ImGui.GetColorU32(ImGuiCol.ButtonHovered) :
                            ImGui.GetColorU32(ImGuiCol.Button);

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

        var textPos = GetCenteredPosition(min, buttonRectSize, textSize);
        draw.AddText(textPos, colors[(int)ImGuiCol.Text].ToUInt(), displaySpan);

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
        if (charge <= 0f) return;

        if (charge >= 1f)
        {
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
        var startPos = new Vector2(min.X, min.Y + (size.Y * 0.5f));

        var remTop = progress;
        draw.PathClear();
        draw.PathLineTo(startPos);

        if (remTop <= verticalHalf)
        {
            draw.PathLineTo(new Vector2(min.X, startPos.Y - remTop));
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
                    draw.PathLineTo(new Vector2(min.X + r + remTop, min.Y));
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
                        draw.PathLineTo(new Vector2(max.X, min.Y + r + MathF.Min(remTop, verticalHalf)));
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
            draw.PathLineTo(new Vector2(min.X, startPos.Y + remBottom));
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
                    draw.PathLineTo(new Vector2(min.X + r + remBottom, max.Y));
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
                        draw.PathLineTo(new Vector2(max.X, max.Y - r - MathF.Min(remBottom, verticalHalf)));
                        draw.PathStroke(chargeColor, ImDrawFlags.None, thickness);
                    }
                }
            }
        }
    }
}
