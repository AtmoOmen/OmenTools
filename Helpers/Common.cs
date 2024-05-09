using System.Text.RegularExpressions;
using SeString = Lumina.Text.SeString;

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

    public static bool IsChineseString(string text)
    {
        return text.All(IsChineseCharacter);
    }

    public static bool IsChineseCharacter(char c)
    {
        return (c >= 0x4E00 && c <= 0x9FA5) || (c >= 0x3400 && c <= 0x4DB5);
    }

    public static void OpenFolder(string path, bool selectFile = false)
    {
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            string command;
            string arguments;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (selectFile)
                {
                    command = "explorer.exe";
                    arguments = $"/select,\"{path}\"";
                }
                else
                {
                    command = "explorer.exe";
                    arguments = path;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                command = "open";
                arguments = selectFile ? $"-R \"{path}\"" : path;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                command = "xdg-open";
                arguments = path;
            }
            else
            {
                return;
            }

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !selectFile,
                CreateNoWindow = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            };
            process.Start();
        }
        catch (Exception _)
        {
            // ignored
        }
    }

    public static DateTime UnixSecondToDateTime(double unixTimeStampS)
    {
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dtDateTime.AddSeconds(unixTimeStampS).ToLocalTime();
    }

    public static DateTime UnixMillisecondToDateTime(long unixTimeStampMS)
    {
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dtDateTime.AddMilliseconds(unixTimeStampMS).ToLocalTime();
    }

    public static Vector4 HexToVector4(string hexColor, bool includeAlpha = true)
    {
        if (!hexColor.StartsWith("#")) throw new ArgumentException("Invalid hex color format");

        hexColor = hexColor.Substring(1);

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

    public static string UTF8StringToString(Utf8String str)
    {
        if (str.StringPtr == null || str.BufUsed <= 1)
            return string.Empty;
        return Encoding.UTF8.GetString(str.StringPtr, (int)str.BufUsed - 1);
    }

    public static string FetchText(this SeString s, bool onlyFirst = false)
    {
        return s.ToDalamudString().FetchText(onlyFirst);
    }

    public static string FetchText(this Utf8String s, bool onlyFirst = false)
    {
        var str = MemoryHelper.ReadSeString(&s);
        return str.FetchText();
    }

    public static string FetchText(this Dalamud.Game.Text.SeStringHandling.SeString seStr, bool onlyFirst = false)
    {
        StringBuilder sb = new();
        foreach (var x in seStr.Payloads)
        {
            if (x is not TextPayload tp) continue;
            sb.Append(tp.Text);
            if (onlyFirst) break;
        }

        return sb.ToString();
    }

    public static void Restart(this Timer timer)
    {
        timer.Stop();
        timer.Start();
    }
}