using System.Runtime.CompilerServices;
using System.Text;
using Dalamud.Game.Text;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SanitizeXML(string text)
    {
        if(string.IsNullOrEmpty(text)) return string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            switch (c)
            {
                case '\\':
                case '/':
                case '*':
                case '?':
                case '|':
                case ':':
                case '：':
                    sb.Append(' ');
                    break;
                case '&':
                    sb.Append("&amp;");
                    break;
                case '<':
                    sb.Append("&lt;");
                    break;
                case '>':
                    sb.Append("&gt;");
                    break;
                case '"':
                    sb.Append("&quot;");
                    break;
                case '\'':
                    sb.Append("&apos;");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SanitizeSEIcon(string input) => 
        SanitizeSEIcon(input.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SanitizeSEIcon(ReadOnlySpan<char> input)
    {
        if(input.IsEmpty) return string.Empty;

        var (start, end, bitmap) = SEIconBitmap.Value;
        Span<char> output      = stackalloc char[input.Length];
        var        outputIndex = 0;

        foreach (var c in input)
        {
            if(c < start || c > end || !IsSeIcon(c, start, bitmap))
                output[outputIndex++] = c;
        }

        return new string(output[..outputIndex]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSeIcon(char c, int start, ulong[] bitmap)
    {
        var adjustedValue = c - start;
        var index         = adjustedValue >> 6;
        var bit           = adjustedValue              & 63;
        return index < bitmap.Length && (bitmap[index] & (1UL << bit)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SanitizeBase64ToFilename(ReadOnlySpan<char> base64String)
    {
        if(base64String.IsEmpty)
            return string.Empty;

        var        maxLength = Math.Min(base64String.Length, 220);
        Span<char> buffer    = stackalloc char[maxLength];
        var        length    = 0;

        for (var i = 0; i < maxLength; i++)
        {
            var c = base64String[i];
            if(c == '=') break;

            buffer[length++] = c switch
            {
                '+' => '_',
                '/' => '_',
                _   => c
            };
        }

        return new string(buffer[..length]);
    }

    private static readonly Lazy<(int Start, int End, ulong[] Bitmap)> SEIconBitmap =
        new(() =>
        {
            var seIcons    = Enum.GetValues<SeIconChar>().Select(i => (int)i).ToList();
            var start      = seIcons.Min();
            var end        = seIcons.Max();
            var range      = end - start + 1;
            var bitmapSize = (range + 63) / 64; // 向上取整到最接近的 64 的倍数
            var bitmap     = new ulong[bitmapSize];

            foreach (var icon in seIcons)
            {
                var adjustedValue = icon - start;
                var index         = adjustedValue >> 6;
                var bit           = adjustedValue & 63;
                bitmap[index] |= 1UL << bit;
            }

            return (start, end, bitmap);
        });
}
