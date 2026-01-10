using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetAddonByName(string addonName, out AtkUnitBase* addonPtr)
    {
        var addon = DService.Instance().Gui.GetAddonByName(addonName).Address;
        if (addon == nint.Zero)
        {
            addonPtr = null;
            return false;
        }

        addonPtr = (AtkUnitBase*)addon;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetAddonByName<T>(string addonName, out T* addonPtr) where T : unmanaged
    {
        var addon = DService.Instance().Gui.GetAddonByName(addonName).Address;
        if (addon == nint.Zero)
        {
            addonPtr = null;
            return false;
        }

        addonPtr = (T*)addon;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* GetAddonByName<T>(string addonName) where T : unmanaged
    {
        var a = DService.Instance().Gui.GetAddonByName(addonName).Address;
        if (a == nint.Zero) return null;

        return (T*)a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AtkUnitBase* GetAddonByName(string name) =>
        GetAddonByName<AtkUnitBase>(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryScanSelectStringText(string text, out int index)
    {
        index = -1;
        if (SelectString == null) return false;

        var entryCount = ((AddonSelectString*)SelectString)->PopupMenu.PopupMenu.EntryCount;
        var atkValues  = ((AddonSelectString*)SelectString)->AtkValues;

        Span<char> buffer     = stackalloc char[512];
        var        searchSpan = text.AsSpan();

        for (var i = 0; i < entryCount; i++)
        {
            ref var atkValue = ref atkValues[i + 7];
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
    public static bool TryScanSelectStringText(IReadOnlyList<string> texts, out int index)
    {
        index = -1;
        if (SelectString == null) return false;

        var entryCount = ((AddonSelectString*)SelectString)->PopupMenu.PopupMenu.EntryCount;
        var atkValues  = ((AddonSelectString*)SelectString)->AtkValues;

        Span<char> buffer = stackalloc char[512];

        for (var i = 0; i < entryCount; i++)
        {
            ref var atkValue = ref atkValues[i + 7];
            if (atkValue.Type == 0 || !atkValue.String.HasValue) continue;

            var utf8Span = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(atkValue.String);
            if (utf8Span.IsEmpty) continue;

            var charCount = Encoding.UTF8.GetCharCount(utf8Span);
            var slice     = charCount <= buffer.Length ? buffer[..charCount] : new char[charCount];

            Encoding.UTF8.GetChars(utf8Span, slice);

            var count = texts.Count;
            for (var j = 0; j < count; j++)
            {
                if (slice.Contains(texts[j].AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    return true;
                }
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryScanSelectIconStringText(string text, out int index)
    {
        index = -1;
        if (SelectIconString == null) return false;

        var entryCount = ((AddonSelectIconString*)SelectIconString)->PopupMenu.PopupMenu.EntryCount;
        var atkValues  = ((AddonSelectIconString*)SelectIconString)->AtkValues;

        Span<char> buffer     = stackalloc char[512];
        var        searchSpan = text.AsSpan();

        for (var i = 0; i < entryCount; i++)
        {
            ref var atkValue = ref atkValues[(i * 3) + 7];
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
    public static bool TryScanSelectIconStringText(IReadOnlyList<string> texts, out int index)
    {
        index = -1;
        if (SelectIconString == null) return false;

        var entryCount = ((AddonSelectIconString*)SelectIconString)->PopupMenu.PopupMenu.EntryCount;
        var atkValues  = ((AddonSelectIconString*)SelectIconString)->AtkValues;

        Span<char> buffer = stackalloc char[512];

        for (var i = 0; i < entryCount; i++)
        {
            ref var atkValue = ref atkValues[(i * 3) + 7];
            if (atkValue.Type == 0 || !atkValue.String.HasValue) continue;

            var utf8Span = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(atkValue.String);
            if (utf8Span.IsEmpty) continue;

            var charCount = Encoding.UTF8.GetCharCount(utf8Span);
            var slice     = charCount <= buffer.Length ? buffer[..charCount] : new char[charCount];

            Encoding.UTF8.GetChars(utf8Span, slice);

            var count = texts.Count;
            for (var j = 0; j < count; j++)
            {
                if (slice.Contains(texts[j].AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    return true;
                }
            }
        }

        return false;
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
            {
                if (slice.Contains(texts[j].AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    return true;
                }
            }
        }

        return false;
    }
}
