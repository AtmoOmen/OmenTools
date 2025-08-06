using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    private static readonly Dictionary<(string text, float width), string> wrappedTextCache = [];
    
    /// <summary>
    /// 可选中的文本，支持链接识别和点击
    /// </summary>
    /// <param name="text">要显示的文本</param>
    /// <param name="lineLength">单行长度</param>
    /// <param name="links">要识别的链接类型</param>
    /// <param name="colorBG">背景颜色</param>
    /// <param name="id">控件ID</param>
    public static void TextSelectable(string text, 
                                      float lineLength, 
                                      List<TextSelectableLinkTypeInfo>? links = null, 
                                      uint? colorBG = null, 
                                      string? id = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var wrappedText = WrapMixedText(text, lineLength).Trim();
        var uniqueId    = $"###{(string.IsNullOrWhiteSpace(id) ? "TextSelectable" + text.GetHashCode() : id)}";

        var lines      = wrappedText.Split('\n');
        var lineHeight = ImGui.GetTextLineHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y;
        var finalSize  = new Vector2(lineLength + (2 * ImGui.GetStyle().FramePadding.X), lines.Length * lineHeight);

        var textTemp   = wrappedText;
        var textLength = wrappedText.Length + 1;

        var startPos       = ImGui.GetCursorPos();
        var screenStartPos = ImGui.GetCursorScreenPos();

        using (ImRaii.PushColor(ImGuiCol.FrameBg, colorBG ?? ImGui.GetColorU32(ImGuiCol.WindowBg)))
        {
            ImGui.SetCursorPos(startPos - ImGui.GetStyle().FramePadding);
            ImGui.InputTextMultiline(uniqueId, ref textTemp, textLength, finalSize,
                                     ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.NoHorizontalScroll);
        }

        // 不禁用会崩
        if (ImGui.IsItemActivated())
            DisableIME();
        if (ImGui.IsItemDeactivated())
            EnableIME();

        if (links is { Count: > 0 })
            ProcessLinks(links, wrappedText, screenStartPos);
    }

    private static void ProcessLinks(List<TextSelectableLinkTypeInfo> links, string wrappedText, Vector2 screenStartPos)
    {
        var drawList = ImGui.GetWindowDrawList();

        foreach (var linkType in links)
        {
            var matches = linkType.Pattern.Matches(wrappedText);

            foreach (Match match in matches)
            {
                var linkPositions = FindLinkPositionsInWrappedText(wrappedText, match.Index, match.Length);

                foreach (var (lineIndex, startX, endX) in linkPositions)
                {
                    var linkStart = new Vector2(
                        screenStartPos.X + startX,
                        screenStartPos.Y + (lineIndex * ImGui.GetTextLineHeightWithSpacing()));

                    var linkEnd = new Vector2(
                        screenStartPos.X + endX,
                        linkStart.Y      + ImGui.GetTextLineHeightWithSpacing());

                    var basicYOffset = (lineIndex + 1) * ImGui.GetStyle().ItemSpacing.Y;

                    // 下划线
                    drawList.AddLine(
                        new(linkStart.X, linkStart.Y + ImGui.GetTextLineHeightWithSpacing() - basicYOffset),
                        new(linkEnd.X,   linkEnd.Y                                          - basicYOffset),
                        linkType.UnderlineColor,
                        1.0f);

                    if (ImGui.IsMouseHoveringRect(linkStart, linkEnd))
                    {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                            linkType.ClickCallback(match);
                    }
                }
            }
        }
    }

    private static List<(int lineIndex, float startX, float endX)> FindLinkPositionsInWrappedText(
        string wrappedText, int linkStartIndex, int linkLength)
    {
        var result = new List<(int lineIndex, float startX, float endX)>();
        var lines  = wrappedText.Split('\n');

        var currentIndex = 0;
        for (var i = 0; i < lines.Length; i++)
        {
            var lineLength   = lines[i].Length;
            var lineEndIndex = currentIndex + lineLength;

            if (linkStartIndex < lineEndIndex && linkStartIndex + linkLength > currentIndex)
            {
                var startInLine = Math.Max(0, linkStartIndex                       - currentIndex);
                var endInLine   = Math.Min(lineLength, linkStartIndex + linkLength - currentIndex);

                var startX = ImGui.CalcTextSize(lines[i].Substring(0, startInLine)).X;
                var endX   = ImGui.CalcTextSize(lines[i].Substring(0, endInLine)).X;

                result.Add((i, startX, endX));
            }

            currentIndex = lineEndIndex + 1; // +1 for newline char
        }

        return result;
    }
    
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

public class TextSelectableLinkTypeInfo
{
    public Regex               Pattern        { get; }
    public Action<Match>       ClickCallback  { get; }
    public uint                UnderlineColor { get; }

    public TextSelectableLinkTypeInfo(string pattern, Action<Match> clickCallback, uint underlineColor)
    {
        Pattern        = new Regex(pattern, RegexOptions.Compiled);
        ClickCallback  = clickCallback;
        UnderlineColor = underlineColor;
    }
}
