using System.Numerics;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Utility;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
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

    public static bool ButtonIcon(string id, FontAwesomeIcon icon, string tooltip = "", bool useStaticFont = false)
    {
        using var idPush = ImRaii.PushId($"{id}_{icon}");

        var iconText   = icon.ToIconString();
        var iconSize   = CalcIconSize(iconText, useStaticFont);
        var buttonSize = new Vector2(GetSingleLineHeight());
        var result     = ImGui.Button(string.Empty, buttonSize);
        var (min, size) = GetItemRect();
        var iconPos     = GetCenteredPosition(min, size, iconSize);

        DrawIconText(iconPos, iconText, useStaticFont);

        if (!tooltip.IsNullOrEmpty())
            TooltipHover(tooltip);

        return result;
    }

    public static bool ButtonIcon(string id, FontAwesomeIcon icon, Vector2 buttonSize, string tooltip = "", bool useStaticFont = false)
    {
        using var idPush = ImRaii.PushId($"{id}_{icon}");

        var iconText = icon.ToIconString();
        var iconSize = CalcIconSize(iconText, useStaticFont);
        var result      = ImGui.Button(string.Empty, buttonSize);
        var (min, size) = GetItemRect();
        var iconPos     = GetCenteredPosition(min, size, iconSize);

        DrawIconText(iconPos, iconText, useStaticFont);

        if (!tooltip.IsNullOrEmpty())
            TooltipHover(tooltip);

        return result;
    }
    
    public static bool ButtonIconWithTextVertical(FontAwesomeIcon icon, string text, bool useStaticFont = false)
    {
        using var idPush = ImRaii.PushId($"{text}_{icon.ToIconString()}");

        var iconText    = icon.ToIconString();
        var iconSize    = CalcIconSize(iconText, useStaticFont);
        var displaySpan = GetDisplaySpan(text);
        var textSize    = ImGui.CalcTextSize(displaySpan);
        var padding     = ImGui.GetStyle().FramePadding.X;
        var spacing     = 3f * ImGuiHelpers.GlobalScale;
        var buttonSize  = new Vector2(MathF.Max(iconSize.X, textSize.X) + padding * 2, GetDoubleLineHeight());
        var result      = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetVerticalLayout(contentMin, contentSize, iconSize, textSize, spacing);

        DrawIconText(iconPos, iconText, useStaticFont);
        ImGui.GetWindowDrawList().AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

    public static bool ButtonIconWithTextVertical(FontAwesomeIcon icon, string text, Vector2 buttonSize, bool useStaticFont = false)
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

    public static bool ButtonIconWithText(FontAwesomeIcon icon, string text, bool useStaticFont = false)
    {
        using var idPush = ImRaii.PushId($"{text}_{icon.ToIconString()}");

        var iconText    = icon.ToIconString();
        var iconSize    = CalcIconSize(iconText, useStaticFont);
        var displaySpan = GetDisplaySpan(text);
        var textSize    = ImGui.CalcTextSize(displaySpan);
        var padding     = ImGui.GetStyle().FramePadding;
        var spacing     = ImGui.GetStyle().ItemSpacing.X;
        var buttonSize  = new Vector2(iconSize.X + textSize.X + padding.X * 2 + spacing, GetSingleLineHeight());
        var result      = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetHorizontalLayout(contentMin, contentSize, iconSize, textSize, spacing);

        DrawIconText(iconPos, iconText, useStaticFont);
        ImGui.GetWindowDrawList().AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

    public static bool ButtonIconWithText(FontAwesomeIcon icon, string text, Vector2 buttonSize, bool useStaticFont = false)
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

    public static bool ButtonImageWithTextVertical(IDalamudTextureWrap icon, string text)
    {
        using var idPush = ImRaii.PushId($"{text}_{icon.Handle}");

        var iconSize    = icon.Size;
        var displaySpan = GetDisplaySpan(text);
        var textSize    = ImGui.CalcTextSize(displaySpan);
        var padding     = ImGui.GetStyle().FramePadding.X;
        var spacing     = 3f * ImGuiHelpers.GlobalScale;
        var buttonSize  = new Vector2(MathF.Max(iconSize.X, textSize.X) + padding * 2, GetDoubleLineHeight());
        var result      = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetVerticalLayout(contentMin, contentSize, iconSize, textSize, spacing);
        var windowDrawList            = ImGui.GetWindowDrawList();

        windowDrawList.AddImage(icon.Handle, iconPos, iconPos + iconSize);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

    public static bool ButtonImageWithTextVertical(IDalamudTextureWrap icon, string text, Vector2 buttonSize)
    {
        using var idPush = ImRaii.PushId($"{text}_{icon.Handle}");

        var iconSize    = icon.Size;
        var displaySpan = GetDisplaySpan(text);
        var textSize    = ImGui.CalcTextSize(displaySpan);
        var spacing     = 3f * ImGuiHelpers.GlobalScale;
        var result      = ImGui.Button(string.Empty, buttonSize);
        var (contentMin, contentSize) = GetButtonContentRect();
        var (iconPos, textPos)        = GetVerticalLayout(contentMin, contentSize, iconSize, textSize, spacing);
        var windowDrawList            = ImGui.GetWindowDrawList();

        windowDrawList.AddImage(icon.Handle, iconPos, iconPos + iconSize);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), displaySpan);

        return result;
    }

    public static bool ButtonIconSelectable(string id, FontAwesomeIcon icon, string tooltip = "", bool useStaticFont = false)
    {
        using var idPush = ImRaii.PushId(id);

        var colors   = ImGui.GetStyle().Colors;
        var iconText = icon.ToIconString();
        var size     = new Vector2(ImGui.GetContentRegionAvail().X, GetSingleLineHeight());

        using var colorPush = ImRaii.PushColor(ImGuiCol.ButtonActive, colors[(int)ImGuiCol.HeaderActive])
                                    .Push(ImGuiCol.ButtonHovered, colors[(int)ImGuiCol.HeaderHovered])
                                    .Push(ImGuiCol.Button,        0);

        bool result;
        using (ImRaii.PushFont(UiBuilder.IconFont, useStaticFont))
            result = ImGui.Button($"{iconText}##{iconText}-{id}", size);

        if (!tooltip.IsNullOrEmpty())
            TooltipHover(tooltip);

        return result;
    }

    public static bool ButtonSelectable(string text)
    {
        var style    = ImGui.GetStyle();
        var padding  = style.FramePadding;
        var colors   = style.Colors;
        var textSize = ImGui.CalcTextSize(text);

        var size = new Vector2(MathF.Max(ImGui.GetContentRegionAvail().X, textSize.X + 2 * padding.X), GetSingleLineHeight());

        using var colorPush = ImRaii.PushColor(ImGuiCol.ButtonActive, colors[(int)ImGuiCol.HeaderActive])
                                    .Push(ImGuiCol.ButtonHovered, colors[(int)ImGuiCol.HeaderHovered])
                                    .Push(ImGuiCol.Button,        0);

        var result = ImGui.Button(text, size);

        return result;
    }

    public static bool ButtonStretch(string text)
    {
        var size   = new Vector2(ImGui.GetContentRegionAvail().X, GetSingleLineHeight());
        var result = ImGui.Button(text, size);

        return result;
    }
}
