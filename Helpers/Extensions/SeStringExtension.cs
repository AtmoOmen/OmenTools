using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.String;
using Lumina.Text.ReadOnly;

namespace OmenTools.Helpers;

public static class SeStringExtension
{
    private const char SECountBaseChar = '\ue08f';

    public static char ToSEChar(this uint integer) => 
        integer <= 9 ? (char)(SECountBaseChar + integer) : char.MinValue;

    public static string ToSECountString(this object value)
    {
        if (value == null) 
            return string.Empty;
        
        var s = value.ToString();
        var result = new char[s.Length];
        for (var i = 0; i < s.Length; i++)
        {
            if (char.IsDigit(s[i]))
                result[i] = (char)(SECountBaseChar + (s[i] - '0'));
            else
                result[i] = s[i];
        }
        return new string(result);
    }

    public static string ExtractText(this ReadOnlySeString s, bool onlyFirst = false) => 
        s.ToDalamudString().ExtractText(onlyFirst);

    public static unsafe string ExtractText(this Utf8String s)
    {
        var str = MemoryHelper.ReadSeString(&s);
        return str.ExtractText();
    }

    public static string ExtractText(this Dalamud.Game.Text.SeStringHandling.SeString seStr, bool onlyFirst = false)
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

    public static SeStringBuilder AddRange(this SeStringBuilder b, IEnumerable<Payload> payloads)
    {
        foreach (var x in payloads)
            b = b.Add(x);

        return b;
    }
}
