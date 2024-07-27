using System.Text.RegularExpressions;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    public static string MarkdownToPlainText(string markdown)
    {
        markdown = Regex.Replace(markdown, @"^\#{1,6}\s*", "", RegexOptions.Multiline);

        markdown = Regex.Replace(markdown, @"(\*{1,2})(.*?)(\*{1,2})", "$2");
        markdown = Regex.Replace(markdown, @"_{1,2}(.*?)_{1,2}", "$1");

        markdown = Regex.Replace(markdown, @"(`{1,3})(.*?)(`{1,3})", "$2");

        markdown = Regex.Replace(markdown, @"\[(.*?)\]\((.*?)\)", "$1");
        markdown = Regex.Replace(markdown, @"\!\[(.*?)\]\((.*?)\)", "$1");

        markdown = Regex.Replace(markdown, @"^> ?(.*)$", "$1", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^-\s+(.*)$", "$1", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^\d+\.\s+(.*)$", "$1", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^---\s*$", "", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^\|[\s\S]+?\|\s*$", "", RegexOptions.Multiline);

        markdown = Regex.Replace(markdown, @"<[^>]*>", "");
        markdown = Regex.Replace(markdown, @"\[\^(.*?)\]:", "");

        markdown = Regex.Replace(markdown, @"`{1,3}(.*?)`{1,3}", "$1");
        markdown = Regex.Replace(markdown, @"~~(.*?)~~", "$1");

        markdown = Regex.Replace(markdown, @"==([^=]+)==", "$1");
        markdown = Regex.Replace(markdown, @"\+\+([^+]+)\+\+", "$1");

        markdown = Regex.Replace(markdown, @"::: collapse [^\n]*\n", "");
        markdown = Regex.Replace(markdown, @"::: segment blue\n", "");
        markdown = Regex.Replace(markdown, @"\n:::", "");

        markdown = Regex.Replace(markdown, @"[ ]+", " ");
        markdown = Regex.Replace(markdown, @"\n+", "\n");

        return markdown.Trim();
    }

    public static bool IsChineseString(string text) => text.All(IsChineseCharacter);

    public static bool IsChineseCharacter(char c) => (c >= 0x4E00 && c <= 0x9FA5) || (c >= 0x3400 && c <= 0x4DB5);

    public static DateTime UnixSecondToDateTime(long unixTimeStampS) 
        => DateTimeOffset.FromUnixTimeSeconds(unixTimeStampS).LocalDateTime;

    public static DateTime UnixMillisecondToDateTime(long unixTimeStampMS) 
        => DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStampMS).LocalDateTime;

    public static Vector4 HexToVector4(string hexColor, bool includeAlpha = true)
    {
        if (!hexColor.StartsWith('#')) throw new ArgumentException("Invalid hex color format");

        hexColor = hexColor[1..];

        int r, g, b, a;
        switch (hexColor.Length)
        {
            case 3:
                r = Convert.ToInt32(hexColor.Substring(0, 1), 16) * 17;
                g = Convert.ToInt32(hexColor.Substring(1, 1), 16) * 17;
                b = Convert.ToInt32(hexColor.Substring(2, 1), 16) * 17;
                a = includeAlpha ? 255 : 0;
                break;
            case 4:
                r = Convert.ToInt32(hexColor.Substring(0, 1), 16) * 17;
                g = Convert.ToInt32(hexColor.Substring(1, 1), 16) * 17;
                b = Convert.ToInt32(hexColor.Substring(2, 1), 16) * 17;
                a = Convert.ToInt32(hexColor.Substring(3, 1), 16) * 17;
                break;
            case 6:
                r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                a = includeAlpha ? 255 : 0;
                break;
            case 8:
                r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                a = Convert.ToInt32(hexColor.Substring(6, 2), 16);
                break;
            default:
                throw new ArgumentException("Invalid hex color length");
        }

        return new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }

    public static Vector4 DarkenColor(Vector4 originalColor, float darkenAmount)
    {
        darkenAmount = Math.Clamp(darkenAmount, 0f, 1f);

        var newR = Math.Max(0, originalColor.X - originalColor.X * darkenAmount);
        var newG = Math.Max(0, originalColor.Y - originalColor.Y * darkenAmount);
        var newB = Math.Max(0, originalColor.Z - originalColor.Z * darkenAmount);

        return new Vector4(newR, newG, newB, originalColor.W);
    }
    
    public static Vector4 LightenColor(Vector4 originalColor, float lightenAmount)
    {
        lightenAmount = Math.Clamp(lightenAmount, 0f, 1f);

        var newR = Math.Min(1, originalColor.X + (1 - originalColor.X) * lightenAmount);
        var newG = Math.Min(1, originalColor.Y + (1 - originalColor.Y) * lightenAmount);
        var newB = Math.Min(1, originalColor.Z + (1 - originalColor.Z) * lightenAmount);

        return new Vector4(newR, newG, newB, originalColor.W);
    }

    public static string UTF8StringToString(Utf8String str)
    {
        if (str.StringPtr == null || str.BufUsed <= 1)
            return string.Empty;

        return Encoding.UTF8.GetString(str.StringPtr, (int)str.BufUsed - 1);
    }

    public static void MoveItemToPosition<T>(List<T> list, Func<T, bool> sourceItemSelector, int targetedIndex)
    {
        var sourceIndex = -1;
        for (var i = 0; i < list.Count; i++)
            if (sourceItemSelector(list[i]))
            {
                sourceIndex = i;
                break;
            }

        if (sourceIndex == targetedIndex) return;
        var item = list[sourceIndex];
        list.RemoveAt(sourceIndex);
        list.Insert(targetedIndex, item);
    }
}