using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Lumina.Data;
using Lumina.Text.ReadOnly;

namespace OmenTools.Extensions;

public static class NumericExtension
{
    private static readonly HashSet<Language> ValidFormatLanguages =
    [
        Language.ChineseSimplified,
        Language.ChineseTraditional,
        Language.Japanese,
        Language.TraditionalChinese
    ];

    extension<T>(T number) where T : IBinaryInteger<T>, IFormattable
    {
        public ReadOnlySeString ToChineseSeString
        (
            ushort? minusColor = null,
            ushort? unitColor  = null
        )
        {
            if (!ValidFormatLanguages.Contains(GameState.ClientLanguge) || T.IsZero(number))
                return new ReadOnlySeString(number.ToString());

            const string STR_ZHAO = "兆";
            string       strYi   = "亿", strWan = "万", strZero = "零";

            switch (GameState.ClientLanguge)
            {
                case Language.Japanese:
                    strYi   = "億";
                    strWan  = "万";
                    strZero = "0";
                    break;
                case Language.ChineseTraditional or Language.TraditionalChinese:
                    strYi  = "億";
                    strWan = "萬";
                    break;
            }

            var val           = Int128.CreateChecked(number);
            var isNegative    = val < 0;
            var currentNumber = Int128.Abs(val);

            Int128 valZhao = 1_0000_0000_0000;
            Int128 valYi   = 1_0000_0000;
            Int128 valWan  = 1_0000;
            Int128 valQian = 1000;

            var zhao = currentNumber / valZhao;
            var remZ = currentNumber % valZhao;
            var yi   = remZ          / valYi;
            var remY = remZ          % valYi;
            var wan  = remY          / valWan;
            var ge   = remY          % valWan;

            var builder = new SeStringBuilder();

            if (isNegative)
            {
                if (minusColor != null)
                    builder.AddUiForeground(minusColor.Value);
                builder.AddText("-");
                if (minusColor != null)
                    builder.AddUiForegroundOff();
            }

            var hasPrinted  = false;
            var pendingZero = false;

            if (zhao > 0)
            {
                builder.AddText(zhao.ToString());

                if (unitColor != null)
                    builder.AddUiForeground(STR_ZHAO, unitColor.Value);
                else
                    builder.AddText(STR_ZHAO);

                hasPrinted = true;
            }

            if (yi > 0)
            {
                if (pendingZero || hasPrinted && yi < valQian)
                    builder.AddText(strZero);

                builder.AddText(yi.ToString());

                if (unitColor != null)
                    builder.AddUiForeground(strYi, unitColor.Value);
                else
                    builder.AddText(strYi);

                hasPrinted  = true;
                pendingZero = false;
            }
            else if (hasPrinted)
                pendingZero = true;

            if (wan > 0)
            {
                if (pendingZero || hasPrinted && wan < valQian)
                    builder.AddText(strZero);

                builder.AddText(wan.ToString());

                if (unitColor != null)
                    builder.AddUiForeground(strWan, unitColor.Value);
                else
                    builder.AddText(strWan);

                hasPrinted  = true;
                pendingZero = false;
            }
            else if (hasPrinted)
                pendingZero = true;

            if (ge > 0)
            {
                if (pendingZero || hasPrinted && ge < valQian)
                    builder.AddText(strZero);

                builder.AddText(ge.ToString());
            }

            return builder.Build().Encode();
        }

        public string ToChineseString()
        {
            if (!ValidFormatLanguages.Contains(GameState.ClientLanguge) || T.IsZero(number))
                return number.ToString("N0", null);

            const char C_ZHAO = '兆';
            char       cYi    = '亿', cWan = '万', cZero = '零';

            switch (GameState.ClientLanguge)
            {
                case Language.Japanese:
                    cYi   = '億';
                    cWan  = '万';
                    cZero = '0';
                    break;
                case Language.ChineseTraditional:
                case Language.TraditionalChinese:
                    cYi  = '億';
                    cWan = '萬';
                    break;
            }

            var val           = Int128.CreateChecked(number);
            var isNegative    = val < 0;
            var currentNumber = Int128.Abs(val);

            Int128 valZhao = 1_0000_0000_0000;
            Int128 valYi   = 1_0000_0000;
            Int128 valWan  = 1_0000;
            Int128 valQian = 1000;

            var zhao = currentNumber / valZhao;
            var remZ = currentNumber % valZhao;
            var yi   = remZ          / valYi;
            var remY = remZ          % valYi;
            var wan  = remY          / valWan;
            var ge   = remY          % valWan;

            var builder = new StringBuilder(64);

            if (isNegative)
                builder.Append('-');

            var hasPrinted  = false;
            var pendingZero = false;

            if (zhao > 0)
            {
                builder.Append(zhao);
                builder.Append(C_ZHAO);
                hasPrinted = true;
            }

            if (yi > 0)
            {
                if (pendingZero || hasPrinted && yi < valQian)
                    builder.Append(cZero);

                builder.Append(yi);
                builder.Append(cYi);

                hasPrinted  = true;
                pendingZero = false;
            }
            else if (hasPrinted)
                pendingZero = true;

            if (wan > 0)
            {
                if (pendingZero || hasPrinted && wan < valQian)
                    builder.Append(cZero);

                builder.Append(wan);
                builder.Append(cWan);

                hasPrinted  = true;
                pendingZero = false;
            }
            else if (hasPrinted)
                pendingZero = true;

            if (ge > 0)
            {
                if (pendingZero || hasPrinted && ge < valQian)
                    builder.Append(cZero);

                builder.Append(ge);
            }

            return builder.ToString();
        }
    }

    extension<T>(T number) where T : INumber<T>, IFormattable
    {
        [SkipLocalsInit]
        public string ToMyriadString()
        {
            Span<char> sourceBuffer = stackalloc char[128];

            if (!ValidFormatLanguages.Contains(GameState.ClientLanguge) ||
                !number.TryFormat(sourceBuffer, out var charsWritten, null, CultureInfo.InvariantCulture))
                return number.ToString("N0", null) ?? string.Empty;

            ReadOnlySpan<char> source = sourceBuffer[..charsWritten];

            var decimalPointIndex = source.IndexOf('.');
            var isNegative        = source.Length > 0 && source[0] == '-' ? 1 : 0;

            var integerEndIndex = decimalPointIndex >= 0 ? decimalPointIndex : charsWritten;
            var integerLength   = integerEndIndex - isNegative;

            const int GROUP_SIZE = 4;

            var separatorCount = integerLength > 0 ? (integerLength - 1) / GROUP_SIZE : 0;
            var finalLength    = charsWritten + separatorCount;

            return string.Create
            (
                finalLength,
                (number, charsWritten, isNegative, integerEndIndex),
                (span, state) =>
                {
                    Span<char> innerBuffer = stackalloc char[128];

                    state.number.TryFormat(innerBuffer, out var innerWritten, null, CultureInfo.InvariantCulture);
                    ReadOnlySpan<char> src = innerBuffer[..innerWritten];

                    var sourceIndex = src.Length  - 1;
                    var destIndex   = span.Length - 1;
                    var intEnd      = state.integerEndIndex;
                    var negMarker   = state.isNegative;

                    if (intEnd < src.Length)
                    {
                        var decimalLength = src.Length - intEnd;
                        src.Slice(intEnd, decimalLength).CopyTo(span[(destIndex - decimalLength + 1)..]);
                        destIndex   -= decimalLength;
                        sourceIndex -= decimalLength;
                    }

                    var digitCounter = 0;

                    while (sourceIndex >= negMarker)
                    {
                        span[destIndex--] = src[sourceIndex--];
                        digitCounter++;

                        if (digitCounter == 4 && sourceIndex >= negMarker)
                        {
                            span[destIndex--] = ',';
                            digitCounter      = 0;
                        }
                    }

                    if (negMarker > 0)
                        span[destIndex] = '-';
                }
            );
        }
    }

    extension(string str)
    {
        public T FromChineseString<T>() where T : IBinaryInteger<T>
        {
            if (string.IsNullOrWhiteSpace(str))
                return T.Zero;

            var span = str.AsSpan().Trim();
            if (span.IsEmpty)
                return T.Zero;

            var isNegative = false;

            if (span[0] == '-')
            {
                isNegative = true;
                span       = span[1..];
            }

            Int128 result = 0;

            var idx = span.IndexOf('兆');

            if (idx >= 0)
            {
                result += ParseSegment(span[..idx]) * 1_0000_0000_0000;
                span   =  span[(idx + 1)..];
            }

            idx = span.IndexOfAny('亿', '億');

            if (idx >= 0)
            {
                result += ParseSegment(span[..idx]) * 1_0000_0000;
                span   =  span[(idx + 1)..];
            }

            idx = span.IndexOfAny('万', '萬');

            if (idx >= 0)
            {
                result += ParseSegment(span[..idx]) * 1_0000;
                span   =  span[(idx + 1)..];
            }

            if (!span.IsEmpty)
                result += ParseSegment(span);

            if (isNegative)
                result = -result;

            return T.CreateChecked(result);

            static Int128 ParseSegment(ReadOnlySpan<char> s)
            {
                if (s.IsEmpty) return 0;

                var i = 0;
                while (i < s.Length && s[i] == '零')
                    i++;

                if (i == s.Length) return 0;

                return Int128.TryParse(s[i..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : Int128.Zero;
            }
        }
    }
}
