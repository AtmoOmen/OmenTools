using System.Numerics;
using Dalamud.Interface;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static bool Selectable(string text, bool selected = false,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.Y);
        var result = ImGui.Selectable(text, selected, flags);
        return result;
    }

    public static bool Selectable(string text, ref bool selected, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.Y);
        var result = ImGui.Selectable(text, ref selected, flags);
        return result;
    }
    
    public static bool SelectableFillCell(
        string               text,
        bool                 selected = false,
        ImGuiSelectableFlags flags    = ImGuiSelectableFlags.None)
    {
        var textSize         = ImGui.CalcTextSize(text);
        var windowDrawList   = ImGui.GetWindowDrawList();
        var cursorPos        = ImGui.GetCursorScreenPos();
        var padding          = ImGui.GetStyle().FramePadding.X;
        var selectableHeight = textSize.Y + (2 * padding);

        var result = ImGui.Selectable(string.Empty, selected, flags, new(0, selectableHeight));

        var textPos = cursorPos with { Y = cursorPos.Y + padding };
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        return result;
    }

    public static bool SelectableFillCell(
        string               text,
        ref bool             selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        var textSize         = ImGui.CalcTextSize(text);
        var windowDrawList   = ImGui.GetWindowDrawList();
        var cursorPos        = ImGui.GetCursorScreenPos();
        var padding          = ImGui.GetStyle().FramePadding.X;
        var selectableHeight = textSize.Y + (2 * padding);

        var result = ImGui.Selectable("", ref selected, flags, new Vector2(0, selectableHeight));

        var textPos = cursorPos with { Y = cursorPos.Y + padding };
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        return result;
    }

    public static bool SelectableCentered(
        string               text,
        bool                 selected = false,
        ImGuiSelectableFlags flags    = ImGuiSelectableFlags.None,
        Vector2              size     = default)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) - (ImGui.CalcTextSize(text).X / 2));
        return ImGui.Selectable(text, selected, flags, size);
    }

    public static bool SelectableCentered(
        string               text,
        ref bool             selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None,
        Vector2              size  = default)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) - (ImGui.CalcTextSize(text).X / 2));
        var result = ImGui.Selectable(text, ref selected, flags, size);
        return result;
    }

    public static bool SelectableImageWithText(
        nint                 imageHandle,
        Vector2              imageSize,
        string               text,
        bool                 selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None) =>
        SelectableImageWithText(new ImTextureID(imageHandle), imageSize, text, selected, flags);

    public static bool SelectableImageWithText(
        ImTextureID          imageHandle,
        Vector2              imageSize,
        string               text,
        bool                 selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        using var id = ImRaii.PushId($"{imageHandle}_{text}");

        var style = ImGui.GetStyle();
        
        var verticalPadding       = style.FramePadding.Y;
        var windowDrawList        = ImGui.GetWindowDrawList();
        var selectableTopLeft     = ImGui.GetCursorScreenPos();
        var textSize              = ImGui.CalcTextSize(text);
        var contentHeight         = Math.Max(imageSize.Y, textSize.Y);
        var totalSelectableHeight = contentHeight + (verticalPadding * 2f);
        var selectableSize        = new Vector2(0f, totalSelectableHeight);

        var result = ImGui.Selectable(string.Empty, selected, flags, selectableSize);
        
        var imageX   = selectableTopLeft.X + style.FramePadding.X;
        var imageY   = selectableTopLeft.Y + verticalPadding + ((contentHeight - imageSize.Y) / 2f);
        var imagePos = new Vector2(imageX, imageY);
        var textX    = imagePos.X          + imageSize.X     + style.ItemSpacing.X;
        var textY    = selectableTopLeft.Y + verticalPadding + ((contentHeight - textSize.Y) / 2f);
        var textPos  = new Vector2(textX, textY);
        
        windowDrawList.AddImage(
            imageHandle,
            imagePos,
            new Vector2(imagePos.X + imageSize.X, imagePos.Y + imageSize.Y),
            Vector2.Zero,
            Vector2.One,
            ImGui.GetColorU32(Vector4.One));
        
        windowDrawList.AddText(
            textPos,
            ImGui.GetColorU32(ImGuiCol.Text),
            text);

        return result;
    }

    public static bool SelectableImageWithText(
        ImTextureID          imageHandle,
        Vector2              imageSize,
        string               text,
        ref bool             selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        using var id = ImRaii.PushId($"{imageHandle}_{text}");

        var style = ImGui.GetStyle();

        var verticalPadding       = style.FramePadding.Y;
        var windowDrawList        = ImGui.GetWindowDrawList();
        var selectableTopLeft     = ImGui.GetCursorScreenPos();
        var textSize              = ImGui.CalcTextSize(text);
        var contentHeight         = Math.Max(imageSize.Y, textSize.Y);
        var totalSelectableHeight = contentHeight + (verticalPadding * 2f);
        var selectableSize        = new Vector2(0f, totalSelectableHeight);

        var result = ImGui.Selectable(string.Empty, ref selected, flags, selectableSize);

        var imageX   = selectableTopLeft.X + style.FramePadding.X;
        var imageY   = selectableTopLeft.Y + verticalPadding + ((contentHeight - imageSize.Y) / 2f);
        var imagePos = new Vector2(imageX, imageY);
        var textX    = imagePos.X          + imageSize.X     + style.ItemSpacing.X;
        var textY    = selectableTopLeft.Y + verticalPadding + ((contentHeight - textSize.Y) / 2f);
        var textPos  = new Vector2(textX, textY);

        windowDrawList.AddImage(
            imageHandle,
            imagePos,
            new Vector2(imagePos.X + imageSize.X, imagePos.Y + imageSize.Y),
            Vector2.Zero,
            Vector2.One,
            ImGui.GetColorU32(Vector4.One));

        windowDrawList.AddText(
            textPos,
            ImGui.GetColorU32(ImGuiCol.Text),
            text);

        return result;
    }

    public static bool SelectableTextCentered(
        string               text,
        bool                 selected = false,
        ImGuiSelectableFlags flags    = ImGuiSelectableFlags.None)
    {
        ImGui.PushID($"{text}_{flags}");
        var textSize         = ImGui.CalcTextSize(text);
        var windowDrawList   = ImGui.GetWindowDrawList();
        var cursorPos        = ImGui.GetCursorScreenPos();
        var padding          = ImGui.GetStyle().FramePadding.X;
        var selectableHeight = textSize.Y + (2 * padding);

        var result = ImGui.Selectable(string.Empty, selected, flags, new Vector2(0f, selectableHeight));

        var textPos = new Vector2(cursorPos.X + ((ImGui.GetItemRectSize().X - textSize.X) / 2), cursorPos.Y + padding);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);
        ImGui.PopID();

        return result;
    }

    public static bool SelectableTextCentered(
        string               text,
        ref bool             selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        using var idPush = ImRaii.PushId($"{text}_{flags}");
        
        var textSize         = ImGui.CalcTextSize(text);
        var windowDrawList   = ImGui.GetWindowDrawList();
        var cursorPos        = ImGui.GetCursorScreenPos();
        var padding          = ImGui.GetStyle().FramePadding.X;
        var selectableHeight = textSize.Y + (2 * padding);

        var result = ImGui.Selectable(string.Empty, ref selected, flags, new Vector2(0, selectableHeight));

        var textPos = new Vector2(cursorPos.X + ((ImGui.GetItemRectSize().X - textSize.X) / 2), cursorPos.Y + padding);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        return result;
    }

    public static bool SelectableIconCentered(
        string               id,
        FontAwesomeIcon      icon,
        bool                 selected      = false,
        ImGuiSelectableFlags flags         = ImGuiSelectableFlags.None,
        bool                 useStaticFont = false)
    {
        using var idPush = ImRaii.PushId($"{icon}_{id}");
        
        var textSize = Vector2.Zero;
        using (ImRaii.PushFont(UiBuilder.IconFont, useStaticFont))
            textSize = ImGui.CalcTextSize(icon.ToIconString());
        
        var windowDrawList   = ImGui.GetWindowDrawList();
        var cursorPos        = ImGui.GetCursorScreenPos();
        var padding          = ImGui.GetStyle().FramePadding.X;
        var selectableHeight = textSize.Y + (2 * padding);

        var result = ImGui.Selectable(string.Empty, selected, flags, new Vector2(0, selectableHeight));

        var textPos = new Vector2(cursorPos.X + ((ImGui.GetItemRectSize().X - textSize.X + padding) / 2), cursorPos.Y + padding);

        using (ImRaii.PushFont(UiBuilder.IconFont, useStaticFont))
            windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());

        return result;
    }

    public static bool SelectableIconCentered(
        string               id,
        FontAwesomeIcon      icon,
        ref bool             selected,
        ImGuiSelectableFlags flags         = ImGuiSelectableFlags.None,
        bool                 useStaticFont = false)
    {
        using var idPush = ImRaii.PushId($"{icon}_{id}");

        var textSize = Vector2.Zero;
        using (ImRaii.PushFont(UiBuilder.IconFont, useStaticFont))
            textSize = ImGui.CalcTextSize(icon.ToIconString());

        var windowDrawList   = ImGui.GetWindowDrawList();
        var cursorPos        = ImGui.GetCursorScreenPos();
        var padding          = ImGui.GetStyle().FramePadding.X;
        var selectableHeight = textSize.Y + (2 * padding);

        var result = ImGui.Selectable("", ref selected, flags, new Vector2(0, selectableHeight));

        var textPos = new Vector2(cursorPos.X + ((ImGui.GetItemRectSize().X - textSize.X + padding) / 2), cursorPos.Y + padding);

        using (ImRaii.PushFont(UiBuilder.IconFont, useStaticFont))
            windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());

        return result;
    }
}
