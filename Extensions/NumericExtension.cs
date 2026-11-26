using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Dalamud.Utility;
using Lumina.Data;
using Lumina.Text.ReadOnly;

namespace OmenTools.Extensions;

public static class NumericExtension
{
    extension<T>(T value) where T : IBinaryInteger<T>
    {
        /// <summary>
        ///     判断整数是否包含指定掩码中的任意标志位。
        /// </summary>
        /// <param name="mask">要检查的标志位掩码。</param>
        /// <returns>至少有一个指定标志位置位时返回真。</returns>
        public bool ReadFlag(T mask) =>
            (value & mask) != T.Zero;
    }

    extension<T>(ref T value) where T : struct, IBinaryInteger<T>
    {
        /// <summary>
        ///     将掩码指定的标志位置位，并直接修改整数变量。
        /// </summary>
        /// <param name="mask">要置位的标志位掩码。</param>
        public void SetFlag(T mask) =>
            value |= mask;

        /// <summary>
        ///     将掩码指定的标志位清零，并直接修改整数变量。
        /// </summary>
        /// <param name="mask">要清零的标志位掩码。</param>
        public void ClearFlag(T mask) =>
            value &= ~mask;

        /// <summary>
        ///     根据指定状态设置或清除掩码中的标志位，并直接修改整数变量。
        /// </summary>
        /// <param name="mask">要更新的标志位掩码。</param>
        /// <param name="enabled">是否将指定标志位置位。</param>
        public void UpdateFlag(T mask, bool enabled) =>
            value = enabled ?
                        value | mask :
                        value & ~mask;
    }

    private static bool IsValidFormatLanguage
    (
        Language language
    ) =>
        language switch
        {
            Language.ChineseSimplified or
                Language.ChineseTraditional or
                Language.Japanese or
                Language.TraditionalChinese or
                Language.Korean => true,
            _ => false
        };

    extension<T>(T number) where T : IBinaryInteger<T>, IFormattable
    {
        public ReadOnlySeString ToChineseSeString
        (
            ushort? minusColor = null,
            ushort? unitColor  = null
        )
        {
            if (!IsValidFormatLanguage(GameState.ClientLanguage) || T.IsZero(number))
                return new ReadOnlySeString(number.ToString());

            var    strZhao = "兆";
            string strYi   = "亿", strWan = "万", strZero = "零";

            switch (GameState.ClientLanguage)
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
                case Language.Korean:
                    strZhao = "조";
                    strYi   = "억";
                    strWan  = "만";
                    strZero = "0";
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

            using var rented  = new RentedSeStringBuilder();
            var       builder = rented.Builder;

            if (isNegative)
            {
                if (minusColor != null)
                    builder.PushColorType(minusColor.Value);
                builder.Append("-");
                if (minusColor != null)
                    builder.PopColorType();
            }

            var hasPrinted  = false;
            var pendingZero = false;

            if (zhao > 0)
            {
                builder.Append(zhao.ToString());

                if (unitColor != null)
                {
                    builder.PushColorType(unitColor.Value)
                           .Append(strZhao)
                           .PopColorType();
                }
                else
                    builder.Append(strZhao);

                hasPrinted = true;
            }

            if (yi > 0)
            {
                if (pendingZero || (hasPrinted && yi < valQian))
                    builder.Append(strZero);

                builder.Append(yi.ToString());

                if (unitColor != null)
                {
                    builder.PushColorType(unitColor.Value)
                           .Append(strYi)
                           .PopColorType();
                }
                else
                    builder.Append(strYi);

                hasPrinted  = true;
                pendingZero = false;
            }
            else if (hasPrinted)
                pendingZero = true;

            if (wan > 0)
            {
                if (pendingZero || (hasPrinted && wan < valQian))
                    builder.Append(strZero);

                builder.Append(wan.ToString());

                if (unitColor != null)
                {
                    builder.PushColorType(unitColor.Value)
                           .Append(strWan)
                           .PopColorType();
                }
                else
                    builder.Append(strWan);

                hasPrinted  = true;
                pendingZero = false;
            }
            else if (hasPrinted)
                pendingZero = true;

            if (ge > 0)
            {
                if (pendingZero || (hasPrinted && ge < valQian))
                    builder.Append(strZero);

                builder.Append(ge.ToString());
            }

            return builder.ToReadOnlySeString();
        }

        public string ToChineseString()
        {
            if (!IsValidFormatLanguage(GameState.ClientLanguage) || T.IsZero(number))
                return number.ToString("N0", null);

            char cZhao = '兆', cYi = '亿', cWan = '万', cZero = '零';

            switch (GameState.ClientLanguage)
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
                case Language.Korean:
                    cZhao = '조';
                    cYi   = '억';
                    cWan  = '만';
                    cZero = '0';
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
                builder.Append(cZhao);
                hasPrinted = true;
            }

            if (yi > 0)
            {
                if (pendingZero || (hasPrinted && yi < valQian))
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
                if (pendingZero || (hasPrinted && wan < valQian))
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
                if (pendingZero || (hasPrinted && ge < valQian))
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

            if (!IsValidFormatLanguage(GameState.ClientLanguage) ||
                !number.TryFormat(sourceBuffer, out var charsWritten, null, CultureInfo.InvariantCulture))
                return number.ToString("N0", null) ?? string.Empty;

            ReadOnlySpan<char> source = sourceBuffer[..charsWritten];

            var decimalPointIndex = source.IndexOf('.');
            var isNegative = source.Length > 0 && source[0] == '-' ?
                                 1 :
                                 0;

            var integerEndIndex = decimalPointIndex >= 0 ?
                                      decimalPointIndex :
                                      charsWritten;
            var integerLength = integerEndIndex - isNegative;

            const int GROUP_SIZE = 4;

            var separatorCount = integerLength > 0 ?
                                     (integerLength - 1) / GROUP_SIZE :
                                     0;
            var finalLength = charsWritten + separatorCount;

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

            var idx = span.IndexOfAny('兆', '조');

            if (idx >= 0)
            {
                result += ParseSegment(span[..idx]) * 1_0000_0000_0000;
                span   =  span[(idx + 1)..];
            }

            idx = span.IndexOfAny('亿', '億', '억');

            if (idx >= 0)
            {
                result += ParseSegment(span[..idx]) * 1_0000_0000;
                span   =  span[(idx + 1)..];
            }

            idx = span.IndexOfAny('万', '萬', '만');

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

                return Int128.TryParse(s[i..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ?
                           result :
                           Int128.Zero;
            }
        }
    }
}
