namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
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
                CreateNoWindow = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            };
            process.Start();
        }
        catch (Exception ex)
        {
            // ignored
        }
    }

    public static DateTime UnixTimeStampToDateTime(double unixTimeStampS)
    {
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dtDateTime.AddSeconds(unixTimeStampS).ToLocalTime();
    }

    public static DateTime UnixTimeStampToDateTime(long unixTimeStampMS)
    {
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dtDateTime.AddMilliseconds(unixTimeStampMS).ToLocalTime();
    }

    public static Vector4 HexColorToVector4(string hexColor)
    {
        if (hexColor.StartsWith('#'))
            hexColor = hexColor[1..];

        var bytes = new byte[4];
        for (var i = 0; i < 4; i++)
        {
            bytes[i] = byte.Parse(hexColor.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
        }

        var r = bytes[0] / 255f;
        var g = bytes[1] / 255f;
        var b = bytes[2] / 255f;
        var a = bytes.Length > 3 ? bytes[3] / 255f : 1f;

        return new Vector4(r, g, b, a);
    }

    public static string UTF8StringToString(Utf8String str)
    {
        if (str.StringPtr == null || str.BufUsed <= 1)
            return string.Empty;
        return Encoding.UTF8.GetString(str.StringPtr, (int)str.BufUsed - 1);
    }

    public static string FetchText(this Lumina.Text.SeString s, bool onlyFirst = false)
    {
        return s.ToDalamudString().FetchText(onlyFirst);
    }

    public static string FetchText(this Utf8String s, bool onlyFirst = false)
    {
        var str = MemoryHelper.ReadSeString(&s);
        return str.FetchText();
    }

    public static string FetchText(this SeString seStr, bool onlyFirst = false)
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