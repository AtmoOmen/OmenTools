using System.Globalization;
using System.Text.RegularExpressions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.System.String;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
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

    public static bool IsChineseString(string text) => text.All(IsChineseCharacter);

    public static bool IsChineseCharacter(char c) => (c >= 0x4E00 && c <= 0x9FA5) || (c >= 0x3400 && c <= 0x4DB5);
    
    public static Utf8String* FormatNumberByChineseNotation(int num, string lang = "ChineseSimplified", int minusColor = -1, int unitColor = -1)
    {
        if (num == 0) return Utf8String.FromString("0");

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

        var yi               = num              / 100000000;
        var remainingAfterYi = num              % 100000000;
        var wan              = remainingAfterYi / 10000;
        var ge               = remainingAfterYi % 10000;

        var builder = new SeStringBuilder();

        if (isNegative)
        {
            if (minusColor != -1) builder.AddUiForeground((ushort)minusColor);
            builder.AddText("-");
            if (minusColor != -1) builder.AddUiForegroundOff();
        }

        var hasYi  = yi  > 0;
        var hasWan = wan > 0;
        var hasGe  = ge  > 0;

        // 亿
        if (hasYi)
        {
            builder.Append(yi.ToString());
            if (unitColor != -1) builder.AddUiForeground((ushort)unitColor);
            builder.AddText(亿);
            if (unitColor != -1) builder.AddUiForegroundOff();
        }

        // 万
        if (hasWan)
        {
            builder.Append(wan.ToString());
            if (unitColor != -1) builder.AddUiForeground((ushort)unitColor);
            builder.AddText(万);
            if (unitColor != -1) builder.AddUiForegroundOff();
        }
        else if (hasYi && hasGe) // 亿存在但万为 0 且个存在时补零
            builder.Append(零);

        // 个
        if (hasGe)
        {
            // 当高位存在且个不足四位时补零 (1亿零1000)
            var needsZero = (hasYi || hasWan) && ge < 1000;
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
    
    public static Utf8String* FormatNumberByTenThousand(int number)
    {
        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        numberFormat.NumberGroupSizes     = [4];
        numberFormat.NumberGroupSeparator = ",";
        
        return Utf8String.FromString(number.ToString("N0", numberFormat));
    }

    public static int ParseFormattedChineseNumber(string formatted)
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

        long yi = 0, wan = 0, ge = 0;

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

            geStr = geStr.Replace(",", ""); // 移除逗号分隔符

            if (geStr.Length > 0) { ge = long.Parse(geStr); }
        }

        var result = (yi * 100000000L) + (wan * 10000L) + ge;
        result *= sign;

        checked
        {
            return (int)result;
        }
    }
}
