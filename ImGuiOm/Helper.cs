using System.Runtime.CompilerServices;

namespace OmenTools.ImGuiOm;

public static partial class ImGuiOm
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<char> GetDisplaySpan(string text)
    {
        if (string.IsNullOrEmpty(text)) return default;
        var span      = text.AsSpan();
        var hashIndex = span.IndexOf("##");
        return hashIndex == -1 ? span : span[..hashIndex];
    }
}
