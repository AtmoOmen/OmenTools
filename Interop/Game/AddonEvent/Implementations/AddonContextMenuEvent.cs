using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.UI;
using OmenTools.Interop.Game.AddonEvent.Abstractions;

namespace OmenTools.Interop.Game.AddonEvent;

public unsafe class AddonContextMenuEvent : AddonEventBase
{
    public static bool Select(IReadOnlyList<string> text)
    {
        if (!ContextMenuAddon->IsAddonAndNodesReady()) return false;
        if (!TryScanContextMenuText(text, out var index)) return false;

        return Select(index);
    }

    public static bool Select(string text)
    {
        if (!ContextMenuAddon->IsAddonAndNodesReady()) return false;
        if (!TryScanContextMenuText(text, out var index)) return false;

        return Select(index);
    }

    public static bool Select(int index)
    {
        if (!ContextMenuAddon->IsAddonAndNodesReady()) return false;

        ContextMenuAddon->Callback(0, index, 0U, 0, 0);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryScanContextMenuText(string text, out int index)
    {
        index = -1;
        if (ContextMenuAddon == null) return false;

        var atkValues  = ((AddonContextMenu*)ContextMenuAddon)->AtkValues;
        var entryCount = atkValues[0].UInt;
        if (entryCount == 0) return false;

        Span<char> buffer     = stackalloc char[512];
        var        searchSpan = text.AsSpan();

        for (var i = 0; i < entryCount; i++)
        {
            ref var atkValue = ref atkValues[i + 8];
            if (atkValue.Type == 0 || !atkValue.String.HasValue) continue;

            var utf8Span = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(atkValue.String);
            if (utf8Span.IsEmpty) continue;

            var charCount = Encoding.UTF8.GetCharCount(utf8Span);
            var slice     = charCount <= buffer.Length ? buffer[..charCount] : new char[charCount];

            Encoding.UTF8.GetChars(utf8Span, slice);

            if (slice.Contains(searchSpan, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryScanContextMenuText(IReadOnlyList<string> texts, out int index)
    {
        index = -1;
        if (ContextMenuAddon == null) return false;

        var atkValues  = ((AddonContextMenu*)ContextMenuAddon)->AtkValues;
        var entryCount = atkValues[0].UInt;
        if (entryCount == 0) return false;

        Span<char> buffer = stackalloc char[512];

        for (var i = 0; i < entryCount; i++)
        {
            ref var atkValue = ref atkValues[i + 8];
            if (atkValue.Type == 0 || !atkValue.String.HasValue) continue;

            var utf8Span = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(atkValue.String);
            if (utf8Span.IsEmpty) continue;

            var charCount = Encoding.UTF8.GetCharCount(utf8Span);
            var slice     = charCount <= buffer.Length ? buffer[..charCount] : new char[charCount];

            Encoding.UTF8.GetChars(utf8Span, slice);

            var count = texts.Count;

            for (var j = 0; j < count; j++)
                if (slice.Contains(texts[j].AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    return true;
                }
        }

        return false;
    }
}
