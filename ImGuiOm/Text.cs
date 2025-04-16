using System.Numerics;
using System.Text;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    public static void Text(string text)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.X);
        ImGui.Text(text);
    }

    public static void TextImage(string text, nint imageHandle, Vector2 imageSize)
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().FramePadding.Y);
        ImGui.Image(imageHandle, imageSize);

        ImGui.SameLine();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetStyle().FramePadding.Y);
        Text(text);
    }

    public static void TextCentered(string text)
    {
        CenterAlignFor(ImGui.CalcTextSize(text).X);
        Text(text);
    }

    public static void TextDisabledWrapped(string text)
    {
        ImGui.BeginDisabled();
        ImGui.TextWrapped(text);
        ImGui.EndDisabled();
    }

    public static void TextDisabledWrapped(string text, float warpPos)
    {
        ImGui.BeginDisabled();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * warpPos);
        Text(text);
        ImGui.PopTextWrapPos();
        ImGui.EndDisabled();
    }

    public static bool TextIcon(FontAwesomeIcon icon, string text, bool useStaticFont = false)
    {
        if (useStaticFont) ImGui.PushFont(UiBuilder.IconFont);
        var iconSize = ImGui.CalcTextSize(icon.ToIconString());
        if (useStaticFont) ImGui.PopFont();

        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorPos      = ImGui.GetCursorScreenPos();
        var padding        = ImGui.GetStyle().FramePadding;

        var textSize     = ImGui.CalcTextSize(text);
        var buttonHeight = Math.Max(iconSize.Y, textSize.Y);

        ImGui.BeginDisabled();
        ImGui.PushStyleColor(ImGuiCol.Button, 0);
        var result = ImGui.Button("", new Vector2(iconSize.X + textSize.X + 3 * padding.X, buttonHeight + 2 * padding.Y));
        ImGui.PopStyleColor();
        ImGui.EndDisabled();

        var iconPos = new Vector2(cursorPos.X + padding.X, cursorPos.Y + padding.Y);
        if (useStaticFont) ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        if (useStaticFont) ImGui.PopFont();

        var textPos = new Vector2(iconPos.X + iconSize.X + 2 * padding.X, cursorPos.Y + padding.Y);
        windowDrawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), text);

        return result;
    }

    public static void TextOutlined(Vector4 textColor, string text, Vector4 outlineColor = default, float outlineThickness = 1.5f)
    {
        if (outlineColor == default)
            outlineColor = new Vector4(0, 0, 0, 1);

        var originalPos = ImGui.GetCursorPos();
        using (ImRaii.Group())
        {
            // 8 方向阴影
            for (var x = -outlineThickness; x <= outlineThickness; x += 0.5f)
            {
                for (var y = -outlineThickness; y <= outlineThickness; y += 0.5f)
                {
                    if (x == 0 && y == 0) continue;

                    ImGui.SetCursorPos(originalPos + new Vector2(x, y));
                    ImGui.TextColored(outlineColor, text);
                }
            }

            // 原始文字
            ImGui.SetCursorPos(originalPos);
            ImGui.TextColored(textColor, text);
        }
    }

    /// <summary>
    /// 可选中的文本 (使用 <see cref="ImGui.InputText(string, ref string, uint, ImGuiInputTextFlags)"/> 伪装而成)
    /// </summary>
    /// <param name="text">要显示的文本</param>
    /// <param name="lineLength">单行长度</param>
    /// <param name="colorBG">背景颜色, 如若不指定则为 <see cref="ImGuiCol.WindowBg"/> 的颜色, 可以使用 <see cref="ImGui.GetColorU32(ImGuiCol)"/> 来获取</param>
    /// <param name="id">不指定则使用 <paramref name="text"/> 来作为输入框 ID</param>
    public static void TextSelectable(string text, float lineLength, uint? colorBG = null, string? id = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var wrappedText = WrapMixedText(text, lineLength).Trim();

        var lines      = wrappedText.Split('\n');
        var lineHeight = ImGui.GetTextLineHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y;
        var finalSize  = new Vector2(lineLength + (2 * ImGui.GetStyle().FramePadding.X), lines.Length * lineHeight);

        var textTemp   = wrappedText;
        var textLength = (uint)(wrappedText.Length + 1);
        using (ImRaii.PushColor(ImGuiCol.FrameBg, colorBG == null ? ImGui.GetColorU32(ImGuiCol.WindowBg) : colorBG.Value))
        {
            ImGui.SetCursorPos(ImGui.GetCursorPos() - ImGui.GetStyle().FramePadding);
            ImGui.InputTextMultiline($"###{(string.IsNullOrWhiteSpace(id) ? text : id)}", ref textTemp, textLength, finalSize,
                                     ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.NoHorizontalScroll);
        }

        // 不禁用会崩
        if (ImGui.IsItemActivated())
            DisableIME();
        if (ImGui.IsItemDeactivated())
            EnableIME();
    }

    private static readonly Dictionary<(string text, float width), string> wrappedTextCache = [];
    
    private static string WrapMixedText(string text, float maxWidth)
    {
        if (string.IsNullOrEmpty(text)) 
            return string.Empty;

        var cacheKey = (text, maxWidth);
        if (wrappedTextCache.TryGetValue(cacheKey, out var cachedResult))
            return cachedResult;

        var result           = new StringBuilder(text.Length + 20); // 预分配
        var currentLineWidth = 0f;

        var endPunctuations = new[]
            { '，', '。', '、', '：', '；', '！', '？', '）', '》', '"', '\'', '」', '』', '】', '）', '…', ',', '.', ':', ';', '!', '?', ')', '>', '"', '\'', ']', '}' };

        var i        = 0;
        var textSpan = text.AsSpan();
        while (i < textSpan.Length)
        {
            // 英文单词
            if (IsLatinChar(textSpan[i]))
            {
                var wordStart = i;
                while (i < textSpan.Length && IsLatinChar(textSpan[i])) 
                    i++;

                var wordSpan  = textSpan.Slice(wordStart, i - wordStart);
                var wordWidth = GetWordWidth(wordSpan);

                if (wordWidth > maxWidth && currentLineWidth > 0)
                {
                    result.Append('\n');
                    currentLineWidth = 0;
                }

                if (currentLineWidth + wordWidth > maxWidth)
                {
                    result.Append('\n');
                    currentLineWidth = 0;
                }

                result.Append(wordSpan);
                currentLineWidth += wordWidth;

                // 单词后的空格
                if (i < textSpan.Length && textSpan[i] == ' ')
                {
                    var spaceWidth = GetCharWidth(' ');
                    if (currentLineWidth + spaceWidth <= maxWidth)
                    {
                        result.Append(' ');
                        currentLineWidth += spaceWidth;
                    }

                    i++;
                }

                continue;
            }

            var currentChar      = textSpan[i];
            var isEndPunctuation = Array.IndexOf(endPunctuations, currentChar) >= 0;
            var charWidth        = GetCharWidth(currentChar);

            // 标点符号
            if (isEndPunctuation)
            {
                if (currentLineWidth + charWidth > maxWidth * 0.95f && currentLineWidth > 0)
                {
                    result.Append('\n');
                    currentLineWidth = 0;
                }

                result.Append(currentChar);
                currentLineWidth += charWidth;
                i++;
                continue;
            }

            // 检查下一个字符是否是标点
            if (i + 1 < textSpan.Length && Array.IndexOf(endPunctuations, textSpan[i + 1]) >= 0)
            {
                var nextCharWidth = GetCharWidth(textSpan[i + 1]);

                if (currentLineWidth + charWidth + nextCharWidth > maxWidth)
                {
                    result.Append('\n');
                    currentLineWidth = 0;
                }
            }

            // 普通字符
            if (currentLineWidth + charWidth > maxWidth)
            {
                result.Append('\n');
                currentLineWidth = 0;
            }

            result.Append(currentChar);
            currentLineWidth += charWidth;
            i++;
        }

        var finalResult = result.ToString();

        if (wrappedTextCache.Count > 100)
            wrappedTextCache.Clear();
        wrappedTextCache[cacheKey] = finalResult;

        return finalResult;

        bool IsLatinChar(char c) =>
            (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || c == '-' || c == '@' || c == '&';

        float GetCharWidth(char c) => ImGui.CalcTextSize(c.ToString()).X;

        float GetWordWidth(ReadOnlySpan<char> word) =>
            ImGui.CalcTextSize(word.ToString()).X;
    }
}
