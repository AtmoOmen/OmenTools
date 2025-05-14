using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.String;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    /// <summary>
    /// Markdown 格式文本转纯文本
    /// </summary>
    public static string MarkdownToPlainText(string markdown)
    {
        markdown = Regex.Replace(markdown, @"^\#{1,6}\s*", "", RegexOptions.Multiline);

        markdown = Regex.Replace(markdown, @"(\*{1,2})(.*?)(\*{1,2})", "$2");
        markdown = Regex.Replace(markdown, @"_{1,2}(.*?)_{1,2}",       "$1");

        markdown = Regex.Replace(markdown, @"(`{1,3})(.*?)(`{1,3})", "$2");

        markdown = Regex.Replace(markdown, @"\[(.*?)\]\((.*?)\)",   "$1");
        markdown = Regex.Replace(markdown, @"\!\[(.*?)\]\((.*?)\)", "$1");

        markdown = Regex.Replace(markdown, @"^> ?(.*)$",         "$1", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^-\s+(.*)$",        "$1", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^\d+\.\s+(.*)$",    "$1", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^---\s*$",          "",   RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^\|[\s\S]+?\|\s*$", "",   RegexOptions.Multiline);

        markdown = Regex.Replace(markdown, @"<[^>]*>",      "");
        markdown = Regex.Replace(markdown, @"\[\^(.*?)\]:", "");

        markdown = Regex.Replace(markdown, @"`{1,3}(.*?)`{1,3}", "$1");
        markdown = Regex.Replace(markdown, @"~~(.*?)~~",         "$1");

        markdown = Regex.Replace(markdown, @"==([^=]+)==",     "$1");
        markdown = Regex.Replace(markdown, @"\+\+([^+]+)\+\+", "$1");

        markdown = Regex.Replace(markdown, @"::: collapse [^\n]*\n", "");
        markdown = Regex.Replace(markdown, @"::: segment blue\n",    "");
        markdown = Regex.Replace(markdown, @"\n:::",                 "");

        markdown = Regex.Replace(markdown, @"[ ]+", " ");
        markdown = Regex.Replace(markdown, @"\n+",  "\n");

        return markdown.Trim();
    }

    /// <summary>
    /// 给定字符串是否均为汉字
    /// </summary>
    public static bool IsChineseString(string text) => 
        text.All(IsChineseCharacter);

    /// <summary>
    /// 给定字符是否为汉字
    /// </summary>
    public static bool IsChineseCharacter(char c) => 
        (c >= 0x4E00 && c <= 0x9FA5) || (c >= 0x3400 && c <= 0x4DB5);

    /// <summary>
    /// 将给定数字格式化为带中文单位的 Utf8String 字符串指针, 最多支持至 兆
    /// </summary>
    /// <param name="num">数字</param>
    /// <param name="lang">语言: ChineseSimplified, ChineseTraditional, Japanese</param>
    /// <param name="minusColor"><see cref="UIColor"/> Row ID</param>
    /// <param name="unitColor"><see cref="UIColor"/> Row ID</param>
    public static Utf8String* FormatUtf8NumberByChineseNotation(long num, string lang = "ChineseSimplified", 
                                                                int minusColor = -1, int unitColor = -1)
    {
        if (num == 0) return Utf8String.FromString("0");

        var 兆 = "兆";
        var 亿 = "亿";
        var 万 = "万";
        var 零 = "零";
        switch (lang)
        {
            case "ChineseTraditional":
                亿 = "億";
                万 = "萬";
                break;
            case "Japanese":
                亿 = "億";
                零 = "0";
                break;
        }

        var isNegative = num < 0;
        num = Math.Abs(num);

        var zhao               = num                / 1000000000000;
        var remainingAfterZhao = num                % 1000000000000;
        var yi                 = remainingAfterZhao / 100000000;
        var remainingAfterYi   = remainingAfterZhao % 100000000;
        var wan                = remainingAfterYi   / 10000;
        var ge                 = remainingAfterYi   % 10000;

        var builder = new SeStringBuilder();

        if (isNegative)
        {
            if (minusColor != -1) builder.AddUiForeground((ushort)minusColor);
            builder.AddText("-");
            if (minusColor != -1) builder.AddUiForegroundOff();
        }

        var hasZhao = zhao > 0;
        var hasYi   = yi   > 0;
        var hasWan  = wan  > 0;
        var hasGe   = ge   > 0;

        // 兆
        if (hasZhao)
        {
            builder.Append(zhao.ToString());
            if (unitColor != -1) builder.AddUiForeground((ushort)unitColor);
            builder.AddText(兆);
            if (unitColor != -1) builder.AddUiForegroundOff();
        }

        // 亿
        if (hasYi)
        {
            builder.Append(yi.ToString());
            if (unitColor != -1) builder.AddUiForeground((ushort)unitColor);
            builder.AddText(亿);
            if (unitColor != -1) builder.AddUiForegroundOff();
        }
        else if (hasZhao && (hasWan || hasGe)) // 兆存在但亿为0时补零
            builder.Append(零);

        // 万
        if (hasWan)
        {
            builder.Append(wan.ToString());
            if (unitColor != -1) builder.AddUiForeground((ushort)unitColor);
            builder.AddText(万);
            if (unitColor != -1) builder.AddUiForegroundOff();
        }
        else if ((hasZhao || hasYi) && hasGe) // 兆或亿存在但万为0时补零
            builder.Append(零);

        // 个
        if (hasGe)
        {
            // 当高位存在且个不足四位时补零 (1兆零1000)
            var needsZero = (hasZhao || hasYi || hasWan) && ge < 1000;
            if (needsZero) builder.Append(零);
            builder.Append(ge.ToString());
        }

        // 合并多余零
        var isPreviousZero = false;
        var payloads       = builder.Build().Payloads;
        for (var i = 0; i < payloads.Count; i++)
        {
            var payload = payloads[i];
            if (payload is not TextPayload textPayload) continue;
            if (i == payloads.Count - 1 && textPayload.Text == 零)
                payloads.RemoveAt(i);
            if (isPreviousZero && textPayload.Text == 零)
            {
                payloads.RemoveAt(i);
                isPreviousZero = false;
                i--;
            }

            if (!isPreviousZero && textPayload.Text == 零)
                isPreviousZero = true;
        }

        return payloads.Count == 0
                   ? Utf8String.FromString("0")
                   : Utf8String.FromSequence(new SeString().Append(payloads).Encode());
    }

    /// <summary>
    /// 将给定数字格式化为带中文单位的字符串, 最多支持至 兆
    /// </summary>
    /// <param name="num">数字</param>
    /// <param name="lang">语言: ChineseSimplified, ChineseTraditional, Japanese</param>
    public static string FormatNumberByChineseNotation(long num, string lang = "ChineseSimplified")
    {
        if (num == 0) return "0";

        var 兆 = "兆";
        var 亿 = "亿";
        var 万 = "万";
        var 零 = "零";
        switch (lang)
        {
            case "ChineseTraditional":
                兆 = "兆";
                亿 = "億";
                万 = "萬";
                break;
            case "Japanese":
                兆 = "兆";
                亿 = "億";
                零 = "0";
                break;
        }

        var isNegative = num < 0;
        num = Math.Abs(num);

        // 新增兆级单位（1兆 = 1万亿）
        var zhao               = num                / 1000000000000;
        var remainingAfterZhao = num                % 1000000000000;
        var yi                 = remainingAfterZhao / 100000000;
        var remainingAfterYi   = remainingAfterZhao % 100000000;
        var wan                = remainingAfterYi   / 10000;
        var ge                 = remainingAfterYi   % 10000;

        var builder = new StringBuilder();

        if (isNegative) builder.Append("-");

        var hasZhao = zhao > 0;
        var hasYi   = yi   > 0;
        var hasWan  = wan  > 0;
        var hasGe   = ge   > 0;

        // 兆
        if (hasZhao)
        {
            builder.Append(zhao.ToString());
            builder.Append(兆);
        }

        // 亿
        if (hasYi)
        {
            builder.Append(yi.ToString());
            builder.Append(亿);
        }
        else if (hasZhao && (hasWan || hasGe)) // 兆存在但亿为0时补零
            builder.Append(零);

        // 万
        if (hasWan)
        {
            builder.Append(wan.ToString());
            builder.Append(万);
        }
        else if ((hasZhao || hasYi) && hasGe) // 兆或亿存在但万为0时补零
            builder.Append(零);

        // 个
        if (hasGe)
        {
            var needsZero = (hasZhao || hasYi || hasWan) && ge < 1000;
            if (needsZero) builder.Append(零);
            builder.Append(ge.ToString());
        }

        // 合并多余零
        var isPreviousZero = false;
        var result         = builder.ToString();
        var chars          = result.ToCharArray();
        var filteredChars  = new List<char>();

        for (var i = 0; i < chars.Length; i++)
        {
            var currentChar = chars[i];
            if (i == chars.Length - 1 && currentChar.ToString() == 零)
                continue;

            if (isPreviousZero && currentChar.ToString() == 零)
            {
                isPreviousZero = false;
                continue;
            }

            if (!isPreviousZero && currentChar.ToString() == 零)
            {
                isPreviousZero = true;
                filteredChars.Add(currentChar);
            }
            else { filteredChars.Add(currentChar); }
        }

        return filteredChars.Count == 0 ? "0" : new string(filteredChars.ToArray());
    }
    
    /// <summary>
    /// 将给定数字格式化为万分位分隔的 Utf8String 字符串指针
    /// </summary>
    public static Utf8String* FormatUtf8NumberByTenThousand(long number)
    {
        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSizes     = [4];
        numberFormat.NumberGroupSeparator = ",";
        
        return Utf8String.FromString(number.ToString("N0", numberFormat));
    }
    
    /// <summary>
    /// 将给定数字格式化为万分位分隔的字符串
    /// </summary>
    public static string FormatNumberByTenThousandAsString(long number)
    {
        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSizes     = [4];
        numberFormat.NumberGroupSeparator = ",";

        return number.ToString("N0", numberFormat);
    }

    /// <summary>
    /// 将 <see cref="FormatUtf8NumberByChineseNotation"/> 或 <see cref="FormatNumberByChineseNotation"/> 格式化成的数字字符串解析回数字
    /// </summary>
    public static long ParseFormattedChineseNumber(string formatted)
    {
        if (formatted == "0") return 0;

        var sign  = 1;
        var index = 0;

        // 处理负数
        if (formatted[0] == '-')
        {
            sign  = -1;
            index = 1;
        }

        long zhao = 0, yi = 0, wan = 0, ge = 0;

        // 兆位
        var zhaoIndex = -1;
        foreach (var c in new[] { '兆' })
        {
            var pos = formatted.IndexOf('兆', index);
            if (pos != -1 && (zhaoIndex == -1 || pos < zhaoIndex))
                zhaoIndex = pos;
        }

        if (zhaoIndex != -1)
        {
            zhao  = long.Parse(formatted.Substring(index, zhaoIndex - index));
            index = zhaoIndex + 1;
        }

        // 亿位
        var yiIndex = -1;
        foreach (var c in new[] { '亿', '億' })
        {
            var pos = formatted.IndexOf(c, index);
            if (pos != -1 && (yiIndex == -1 || pos < yiIndex))
                yiIndex = pos;
        }

        if (yiIndex != -1)
        {
            yi    = long.Parse(formatted.Substring(index, yiIndex - index));
            index = yiIndex + 1;
        }

        // 万位
        var wanIndex = -1;
        foreach (var c in new[] { '万', '萬' })
        {
            var pos = formatted.IndexOf(c, index);
            if (pos != -1 && (wanIndex == -1 || pos < wanIndex))
                wanIndex = pos;
        }

        if (wanIndex != -1)
        {
            wan   = long.Parse(formatted.Substring(index, wanIndex - index));
            index = wanIndex + 1;
        }

        // 个位
        if (index < formatted.Length)
        {
            var geStr = formatted[index..];

            // 移除补零的零
            if (geStr.Length > 0)
            {
                var firstChar = geStr[0];
                if (firstChar is '零' or '0')
                    geStr = geStr[1..];
            }

            geStr = geStr.Replace(",", "");

            if (geStr.Length > 0) { ge = long.Parse(geStr); }
        }

        // 计算最终值（兆 * 1万亿 + 亿 * 1亿 + 万 * 1万 + 个）
        var result = (zhao * 1000000000000L) + (yi * 100000000L) + (wan * 10000L) + ge;
        return result * sign;
    }
    
    /// <summary>
    /// 将指定的 <see cref="ReadOnlySeString"/> 首字母转为大写, 并格式化为 <see cref="string"/>
    /// </summary>
    public static string ToTitleCaseExtended(ReadOnlySeString s, sbyte article = 0)
    {
        if (article == 1)
            return s.ToDalamudString().ToString();

        var sb        = new StringBuilder(s.ToDalamudString().ToString());
        var lastSpace = true;
        for (var i = 0; i < sb.Length; ++i)
        {
            if (sb[i] == ' ')
            {
                lastSpace = true;
            }
            else if (lastSpace)
            {
                lastSpace = false;
                sb[i]     = char.ToUpperInvariant(sb[i]);
            }
        }

        return sb.ToString();
    }
}
