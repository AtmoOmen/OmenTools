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
    public static char ToSeChar(this uint integer) =>
        integer switch
        {
            1 => '\ue0b1',
            2 => '\ue0b2',
            3 => '\ue0b3',
            4 => '\ue0b4',
            5 => '\ue0b5',
            6 => '\ue0b6',
            7 => '\ue0b7',
            8 => '\ue0b8',
            9 => '\ue0b9',
            _ => char.MinValue,
        };

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
