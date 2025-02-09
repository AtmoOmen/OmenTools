using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using FFXIVClientStructs.FFXIV.Client.System.String;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    public static float WorldDirHToCharaRotation(Vector2 direction) 
        => direction == Vector2.Zero ? 0f : MathF.Atan2(direction.X, direction.Y);

    public static float CharaRotationSymmetricTransform(float rotation) 
        => MathF.IEEERemainder(rotation + MathF.PI, 2 * MathF.PI);
    
    public static float CameraDirHToCharaRotation(float cameraDirH)
        => (cameraDirH - MathF.PI) % (2 * MathF.PI);
    
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
