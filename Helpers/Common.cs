using Timer = System.Timers.Timer;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
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