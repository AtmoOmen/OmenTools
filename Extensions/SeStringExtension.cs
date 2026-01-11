using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using Lumina.Text.Payloads;
using Lumina.Text.ReadOnly;

namespace OmenTools.Extensions;

public static class SeStringExtension
{
    private const char SE_SQUARE_COUNT_BASE_CHAR = '\uE08F';
    private const char SE_SMALL_COUNT_BASE_CHAR  = '\uE060';
    private const char SE_HEX_COUNT_BASE_CHAR    = '\uE0B1';

    private static readonly Lazy<(int Start, int End, ulong[] Bitmap)> SEIconBitmap =
        new
        (() =>
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
            }
        );

    extension<T>(T value) where T : INumber<T>
    {
        public string ToSEHexCount(string? format = null)
        {
            var raw = value.ToString(format, CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(raw))
                return string.Empty;
            return string.Create
            (
                raw.Length,
                raw,
                (span, state) =>
                {
                    for (var i = 0; i < state.Length; i++)
                    {
                        var c = state[i];

                        if (char.IsAsciiDigit(c))
                            span[i] = (char)(SE_HEX_COUNT_BASE_CHAR + (c - '0'));
                        else
                            span[i] = c;
                    }
                }
            );
        }

        public string ToSESmallCount(string? format = null)
        {
            var raw = value.ToString(format, CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(raw))
                return string.Empty;
            return string.Create
            (
                raw.Length,
                raw,
                (span, state) =>
                {
                    for (var i = 0; i < state.Length; i++)
                    {
                        var c = state[i];

                        if (char.IsAsciiDigit(c))
                            span[i] = (char)(SE_SMALL_COUNT_BASE_CHAR + (c - '0'));
                        else
                            span[i] = c;
                    }
                }
            );
        }

        public string ToSESquareCount(string? format = null)
        {
            var raw = value.ToString(format, CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(raw))
                return string.Empty;
            return string.Create
            (
                raw.Length,
                raw,
                (span, state) =>
                {
                    for (var i = 0; i < state.Length; i++)
                    {
                        var c = state[i];

                        if (char.IsAsciiDigit(c))
                            span[i] = (char)(SE_SQUARE_COUNT_BASE_CHAR + (c - '0'));
                        else
                            span[i] = c;
                    }
                }
            );
        }
    }

    extension(SeStringBuilder b)
    {
        public SeStringBuilder AddRange(IEnumerable<Payload> payloads)
        {
            foreach (var x in payloads)
                b = b.Add(x);

            return b;
        }
    }

    extension(string input)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SanitizeSEIcon() => input.AsSpan().SanitizeSEIcon();
    }

    extension(ReadOnlySpan<char> input)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SanitizeSEIcon()
        {
            if (input.IsEmpty) return string.Empty;

            var (start, end, bitmap) = SEIconBitmap.Value;
            Span<char> output      = stackalloc char[input.Length];
            var        outputIndex = 0;

            foreach (var c in input)
            {
                if (c < start || c > end || !c.IsSEIcon(start, bitmap))
                    output[outputIndex++] = c;
            }

            return new string(output[..outputIndex]);
        }
    }

    extension(ReadOnlySeStringSpan span)
    {
        public ReadOnlySeString PraseAutoTranslate()
        {
            var builder = new SeStringBuilder();

            var counter = -1;
            foreach (var payload in span)
            {
                counter++;
                if (payload.Type            != ReadOnlySePayloadType.Macro  ||
                    payload.MacroCode       != MacroCode.Fixed              ||
                    payload.ExpressionCount != 2                            ||
                    !payload.TryGetExpression(out var expr1, out var expr2) ||
                    !expr1.TryGetUInt(out var group)                        ||
                    !expr2.TryGetUInt(out var rowID)                        ||
                    !LuminaGetter.TryGetRow(rowID, out Completion macroRow) ||
                    macroRow.Group != group + 1)
                {
                    using var rented = new RentedSeStringBuilder();

                    if (counter      == 0                          &&
                        payload.Type == ReadOnlySePayloadType.Text &&
                        string.IsNullOrEmpty(payload.ToString().Trim()))
                        continue;
                
                    builder.Append(rented.Builder.Append(payload).ToReadOnlySeString().ToDalamudString());
                    continue;
                }

                builder.Add(new AutoTranslatePayload(macroRow.Group, rowID));
            }

            return builder.Build().Encode();
        }
    }

    extension(char c)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSEIcon(int start, ulong[] bitmap)
        {
            var adjustedValue = c - start;
            var index         = adjustedValue >> 6;
            var bit           = adjustedValue              & 63;
            return index < bitmap.Length && (bitmap[index] & 1UL << bit) != 0;
        }
    }
}
