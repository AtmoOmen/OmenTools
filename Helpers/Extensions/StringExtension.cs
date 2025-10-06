using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace OmenTools.Helpers;

public static class StringExtension
{
    private static readonly CompareInfo    InvariantCompareInfo     = CultureInfo.InvariantCulture.CompareInfo;
    private const           CompareOptions IgnoreCaseCompareOptions = CompareOptions.IgnoreCase;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFullWidth(this char c) => c is >= '\uFF01' and <= '\uFF5E';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToHalfWidth(this char c) => (char)(c - 0xFEE0);

    public static string ToLowerAndHalfWidth(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var lowercase = input.ToLower();
            
        var result = new StringBuilder();
        foreach (var c in lowercase)
        {
            switch (c)
            {
                case '　':
                    result.Append(' ');
                    continue;
                case >= '！' and <= '～':
                    result.Append((char)(c - 0xFEE0));
                    continue;
                default:
                    result.Append(c);
                    break;
            }
        }

        return result.ToString();
    }

    public static bool TryReplaceIgnoreCase(this string origText, string input, string replacement, out string? result)
    {
        result = null;
        if (string.IsNullOrEmpty(origText) || string.IsNullOrEmpty(input))
            return false;

        var index = InvariantCompareInfo.IndexOf(origText, input, IgnoreCaseCompareOptions);
        if (index == -1)
            return false;

        result = ReplaceAll(origText, input, replacement);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ReplaceAll(string origText, string input, string replacement)
    {
        var inputLength        = input.Length;
        var replacementLength  = replacement.Length;
        var capacityMultiplier = Math.Max(1, replacementLength / inputLength);
        
        char[]? rentedArray = null;
        var buffer = origText.Length <= 256
                         ? stackalloc char[256]
                         : (rentedArray = ArrayPool<char>.Shared.Rent(origText.Length * capacityMultiplier));

        try
        {
            var writePos   = 0;
            var startIndex = 0;
            while (true)
            {
                var index = InvariantCompareInfo.IndexOf(origText, input, startIndex, origText.Length - startIndex, IgnoreCaseCompareOptions);
                if (index == -1)
                {
                    origText.AsSpan(startIndex).CopyTo(buffer[writePos..]);
                    writePos += origText.Length - startIndex;
                    break;
                }

                var count = index - startIndex;
                origText.AsSpan(startIndex, count).CopyTo(buffer[writePos..]);
                writePos += count;

                replacement.AsSpan().CopyTo(buffer[writePos..]);
                writePos += replacementLength;

                startIndex = index + inputLength;
            }

            return new string(buffer[..writePos]);
        }
        finally
        {
            if (rentedArray != null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    public static bool TryReplaceIgnoreCase(this ReadOnlySpan<char> origText, ReadOnlySpan<char> input, ReadOnlySpan<char> replacement, out string? result)
    {
        result = null;
        if (origText.IsEmpty || input.IsEmpty)
            return false;

        var index = InvariantCompareInfo.IndexOf(origText, input, IgnoreCaseCompareOptions);
        if (index == -1)
            return false;

        result = ReplaceAll(origText, input, replacement);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ReplaceAll(ReadOnlySpan<char> origText, ReadOnlySpan<char> input, ReadOnlySpan<char> replacement)
    {
        var inputLength        = input.Length;
        var replacementLength  = replacement.Length;
        var capacityMultiplier = Math.Max(1, replacementLength / inputLength);
    
        char[]? rentedArray = null;
        var buffer = origText.Length <= 256
                         ? stackalloc char[256]
                         : (rentedArray = ArrayPool<char>.Shared.Rent(origText.Length * capacityMultiplier));

        try
        {
            var writePos   = 0;
            var startIndex = 0;
            while (true)
            {
                var remainingText = origText[startIndex..];
                var index         = InvariantCompareInfo.IndexOf(remainingText, input, IgnoreCaseCompareOptions);
                if (index == -1)
                {
                    remainingText.CopyTo(buffer[writePos..]);
                    writePos += remainingText.Length;
                    break;
                }

                origText.Slice(startIndex, index).CopyTo(buffer[writePos..]);
                writePos += index;

                replacement.CopyTo(buffer[writePos..]);
                writePos += replacementLength;

                startIndex += index + inputLength;
            }

            return new string(buffer[..writePos]);
        }
        finally
        {
            if (rentedArray != null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    public static string ToHexString(this IEnumerable<byte> bytes, char separator = ' ')
    {
        var first = true;
        var sb    = new StringBuilder();
        foreach (var x in bytes)
        {
            if (first)
                first = false;
            else
                sb.Append(separator);

            sb.Append($"{x:X2}");
        }

        return sb.ToString();
    }

    public static string ReplaceFirst(this string text, string search, string replace)
    {
        var pos = text.IndexOf(search, StringComparison.Ordinal);
        return pos < 0 ? text : string.Concat(text.AsSpan(0, pos), replace, text.AsSpan(pos + search.Length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithAny(this string source, params string[] values)
        => source.StartsWithAny(values, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithAny(this   string   source, StringComparison stringComparison = StringComparison.Ordinal,
                                     params string[] values)
        => source.StartsWithAny(values, stringComparison);

    public static bool StartsWithAny(
        this string source, IEnumerable<string> compareTo, StringComparison stringComparison = StringComparison.Ordinal)
    {
        foreach (var x in compareTo)
        {
            if (source.StartsWith(x, stringComparison))
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<string> Split(this string str, int chunkSize)
    {
        return Enumerable.Range(0, str.Length / chunkSize)
                         .Select(i => str.Substring(i * chunkSize, chunkSize));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Default(this string s, string defaultValue)
        => string.IsNullOrEmpty(s) ? defaultValue : s;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCase(this string s, string other)
        => s.Equals(other, StringComparison.OrdinalIgnoreCase);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? NullWhenEmpty(this string? s) => 
        s == string.Empty ? null : s;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Cut(this string s, int num)
    {
        if (s.Length <= num) return s;
        return s[..num] + "...";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Repeat(this string s, int num)
    {
        StringBuilder str = new();
        for (var i = 0; i < num; i++)
            str.Append(s);

        return str.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(this IEnumerable<string> e, string separator) => 
        string.Join(separator, e);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(this string obj, IEnumerable<string> values) => 
        values.Any(obj.Contains);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(this string obj, params string[] values) => 
        values.Any(obj.Contains);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(this string obj, StringComparison comp, params string[] values) => 
        values.Any(x => obj.Contains(x, comp));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCaseAny(this string obj, params string[] values) => 
        values.Any(x => x.Equals(obj, StringComparison.OrdinalIgnoreCase));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCaseAny(this string obj, IEnumerable<string> values) => 
        values.Any(x => x.Equals(obj, StringComparison.OrdinalIgnoreCase));

    public static bool ContainsIgnoreCase(this IEnumerable<string> haystack, string needle) => 
        haystack.Any(x => x.EqualsIgnoreCase(needle));
}
