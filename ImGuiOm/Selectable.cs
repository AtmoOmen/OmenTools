using System.Numerics;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    /// <summary>绘制按控件内边距下移的文本选择项。</summary>
    /// <param name="text">选择项标签。</param>
    /// <param name="selected">当前选中状态。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool Selectable
    (
        string               text,
        bool                 selected = false,
        ImGuiSelectableFlags flags    = ImGuiSelectableFlags.None
    )
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.Y);
        var result = ImGui.Selectable(text, selected, flags);
        return result;
    }

    /// <summary>绘制文本选择项，并在触发时更新选中状态。</summary>
    /// <param name="text">选择项标签。</param>
    /// <param name="selected">当前选中状态，触发后更新。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool Selectable(string text, ref bool selected, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.Y);
        var result = ImGui.Selectable(text, ref selected, flags);
        return result;
    }

    /// <summary>绘制填满当前单元格宽度、文本垂直居中的选择项。</summary>
    /// <param name="text">选择项标签；双井号后的部分仅参与标识。</param>
    /// <param name="selected">当前选中状态。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool SelectableFillCell
    (
        string               text,
        bool                 selected = false,
        ImGuiSelectableFlags flags    = ImGuiSelectableFlags.None
    ) => SelectableFillCell(text, ref selected, flags);

    /// <summary>绘制填满当前单元格宽度的选择项，并更新选中状态。</summary>
    /// <param name="text">选择项标签；双井号后的部分仅参与标识。</param>
    /// <param name="selected">当前选中状态，触发后更新。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool SelectableFillCell
    (
        string               text,
        ref bool             selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None
    )
    {
        using var id = ImRaii.PushId(text);
        return DrawSelectableText(GetDisplaySpan(text), ref selected, flags, false);
    }

    /// <summary>将整个文本选择项在当前可用宽度内水平居中。</summary>
    /// <param name="text">选择项标签。</param>
    /// <param name="selected">当前选中状态。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <param name="size">选择项尺寸；零宽度按可见标签度量。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool SelectableCentered
    (
        string               text,
        bool                 selected = false,
        ImGuiSelectableFlags flags    = ImGuiSelectableFlags.None,
        Vector2              size     = default
    )
    {
        CenterAlignFor
        (
            size.X > 0f ?
                size.X :
                TextMetrics.Measure(GetDisplaySpan(text)).Size.X
        );
        return ImGui.Selectable(text, selected, flags, size);
    }

    /// <summary>水平居中绘制整个文本选择项，并更新选中状态。</summary>
    /// <param name="text">选择项标签。</param>
    /// <param name="selected">当前选中状态，触发后更新。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <param name="size">选择项尺寸；零宽度按可见标签度量。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool SelectableCentered
    (
        string               text,
        ref bool             selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None,
        Vector2              size  = default
    )
    {
        CenterAlignFor
        (
            size.X > 0f ?
                size.X :
                TextMetrics.Measure(GetDisplaySpan(text)).Size.X
        );
        var result = ImGui.Selectable(text, ref selected, flags, size);
        return result;
    }

    /// <summary>绘制图片在左、文字在右且垂直居中的选择项。</summary>
    /// <param name="imageHandle">图片纹理句柄。</param>
    /// <param name="imageSize">图片绘制尺寸。</param>
    /// <param name="text">选择项标签。</param>
    /// <param name="selected">当前选中状态。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool SelectableImageWithText
    (
        nint                 imageHandle,
        Vector2              imageSize,
        string               text,
        bool                 selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None
    ) =>
        SelectableImageWithText(new ImTextureID(imageHandle), imageSize, text, selected, flags);

    /// <summary>使用纹理标识绘制图文选择项。</summary>
    /// <param name="imageHandle">图片纹理标识。</param>
    /// <param name="imageSize">图片绘制尺寸。</param>
    /// <param name="text">选择项标签；双井号后的部分仅参与标识。</param>
    /// <param name="selected">当前选中状态。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool SelectableImageWithText
    (
        ImTextureID          imageHandle,
        Vector2              imageSize,
        string               text,
        bool                 selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None
    ) => SelectableImageWithText(imageHandle, imageSize, text, ref selected, flags);

    /// <summary>绘制图文选择项，并在触发时更新选中状态。</summary>
    /// <param name="imageHandle">图片纹理标识。</param>
    /// <param name="imageSize">图片绘制尺寸。</param>
    /// <param name="text">选择项标签；双井号后的部分仅参与标识。</param>
    /// <param name="selected">当前选中状态，触发后更新。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool SelectableImageWithText
    (
        ImTextureID          imageHandle,
        Vector2              imageSize,
        string               text,
        ref bool             selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None
    )
    {
        using var id = ImRaii.PushId($"{imageHandle}_{text}");

        var style = ImGui.GetStyle();

        var verticalPadding       = style.FramePadding.Y;
        var windowDrawList        = ImGui.GetWindowDrawList();
        var selectableTopLeft     = ImGui.GetCursorScreenPos();
        var displaySpan           = GetDisplaySpan(text);
        var metrics               = TextMetrics.Measure(displaySpan);
        var contentHeight         = MathF.Max(imageSize.Y, MathF.Max(metrics.Size.Y, metrics.LineAdvance));
        var totalSelectableHeight = contentHeight + (verticalPadding * 2f);
        var selectableSize        = new Vector2(0f, totalSelectableHeight);

        var result = ImGui.Selectable(string.Empty, ref selected, flags, selectableSize);

        var imageX   = selectableTopLeft.X + style.FramePadding.X;
        var imageY   = selectableTopLeft.Y + verticalPadding + ((contentHeight - imageSize.Y) / 2f);
        var imagePos = new Vector2(imageX, imageY);
        var textX    = imagePos.X + imageSize.X + style.ItemSpacing.X;
        var textPos  = metrics.CenterIn(new Vector2(textX, selectableTopLeft.Y + verticalPadding), metrics.Size with { Y = contentHeight });

        windowDrawList.AddImage
        (
            imageHandle,
            imagePos,
            new Vector2(imagePos.X + imageSize.X, imagePos.Y + imageSize.Y),
            Vector2.Zero,
            Vector2.One,
            ImGui.GetColorU32(Vector4.One)
        );

        windowDrawList.AddText
        (
            textPos,
            ImGui.GetColorU32(ImGuiCol.Text),
            displaySpan
        );

        return result;
    }

    /// <summary>绘制填满可用宽度、文字在区域内居中的选择项。</summary>
    /// <param name="text">选择项标签；双井号后的部分仅参与标识。</param>
    /// <param name="selected">当前选中状态。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool SelectableTextCentered
    (
        string               text,
        bool                 selected = false,
        ImGuiSelectableFlags flags    = ImGuiSelectableFlags.None
    ) => SelectableTextCentered(text, ref selected, flags);

    /// <summary>绘制文字居中的选择项，并在触发时更新选中状态。</summary>
    /// <param name="text">选择项标签；双井号后的部分仅参与标识。</param>
    /// <param name="selected">当前选中状态，触发后更新。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool SelectableTextCentered
    (
        string               text,
        ref bool             selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None
    )
    {
        using var idPush = ImRaii.PushId($"{text}_{flags}");

        return DrawSelectableText(GetDisplaySpan(text), ref selected, flags, true);
    }

    /// <summary>绘制填满可用宽度、按可见字形边界居中图标的选择项。</summary>
    /// <param name="id">控件唯一标识。</param>
    /// <param name="icon">选择项图标。</param>
    /// <param name="selected">当前选中状态。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool SelectableIconCentered
    (
        string               id,
        FontAwesomeIcon      icon,
        bool                 selected = false,
        ImGuiSelectableFlags flags    = ImGuiSelectableFlags.None
    ) => SelectableIconCentered(id, icon, ref selected, flags);

    /// <summary>绘制图标居中的选择项，并在触发时更新选中状态。</summary>
    /// <param name="id">控件唯一标识。</param>
    /// <param name="icon">选择项图标。</param>
    /// <param name="selected">当前选中状态，触发后更新。</param>
    /// <param name="flags">原生选择项标志。</param>
    /// <returns>选择项被触发时返回真。</returns>
    public static bool SelectableIconCentered
    (
        string               id,
        FontAwesomeIcon      icon,
        ref bool             selected,
        ImGuiSelectableFlags flags = ImGuiSelectableFlags.None
    )
    {
        using var idPush = ImRaii.PushId($"{icon}_{id}");

        return DrawSelectableText(icon.ToIconString(), ref selected, flags, true);
    }

    private static bool DrawSelectableText(ReadOnlySpan<char> text, ref bool selected, ImGuiSelectableFlags flags, bool centered)
    {
        var metrics = TextMetrics.Measure(text);
        var padding = ImGui.GetStyle().FramePadding;
        var height  = MathF.Max(metrics.Size.Y, metrics.LineAdvance) + (2f * padding.Y);
        var origin  = ImGui.GetCursorScreenPos();
        var result  = ImGui.Selectable(string.Empty, ref selected, flags, new Vector2(0, height));
        var width = centered ?
                        ImGui.GetItemRectSize().X :
                        metrics.Size.X;
        var position = metrics.CenterIn(origin, new Vector2(width, height));
        ImGui.GetWindowDrawList().AddText(position, ImGui.GetColorU32(ImGuiCol.Text), text);

        return result;
    }
}
