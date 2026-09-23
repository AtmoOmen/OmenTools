using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Utility;
using Jeffijoe.MessageFormat;
using Lumina.Data;
using Lumina.Excel.Sheets;
using Lumina.Text.Payloads;
using Lumina.Text.ReadOnly;
using OmenTools.Interop.Game.Lumina;
using DSeString = Dalamud.Game.Text.SeStringHandling.SeString;
using DSeStringBuilder = Dalamud.Game.Text.SeStringHandling.SeStringBuilder;
using LSeStringBuilder = Lumina.Text.SeStringBuilder;

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

    extension<T>
    (
        T value
    ) where T : INumber<T>
    {
        public string ToSEHexCount
        (
            string? format = null
        )
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

        public string ToSESmallCount
        (
            string? format = null
        )
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

        public string ToSESquareCount
        (
            string? format = null
        )
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

    extension
    (
        LSeStringBuilder b
    )
    {
        public LSeStringBuilder AppendDalamudSeString
        (
            DSeString value
        ) =>
            b.Append(new ReadOnlySeString(value.Encode()));

        public LSeStringBuilder AppendDalamudSeString
        (
            DSeStringBuilder value
        ) =>
            b.Append(value.Build().Encode());

        public LSeStringBuilder AppendDalamudSeString
        (
            Payload value
        ) =>
            b.Append(new ReadOnlySeString(new DSeString(value).Encode()));

        public LSeStringBuilder AppendRentedSeStringBuilder
        (
            RentedSeStringBuilder value
        ) =>
            b.Append(value.Builder.ToReadOnlySeString());

        public LSeStringBuilder AppendIcon
        (
            BitmapFontIcon icon
        ) =>
            b.AppendIcon((uint)icon);

        public LSeStringBuilder AppendIcon
        (
            SeIconChar icon
        ) =>
            b.Append(icon.ToIconString());

        public LSeStringBuilder AppendFormattable
        (
            IFormattable    value,
            IFormatProvider provider
        ) =>
            b.Append(value.ToString(null, provider) ?? string.Empty);
    }

    // TODO: API 16 移除
    extension
    (
        DSeStringBuilder b
    )
    {
        public DSeStringBuilder AddRange
        (
            IEnumerable<Payload> payloads
        )
        {
            foreach (var x in payloads)
                b = b.Add(x);

            return b;
        }
    }

    extension
    (
        ReadOnlySeString
    )
    {
        public static ReadOnlySeString CreateItemLink
        (
            uint    itemID,
            bool    isHQ                = false,
            string? displayNameOverride = null
        ) =>
            ReadOnlySeString.CreateItemLink
            (
                itemID,
                isHQ ?
                    ItemKind.Hq :
                    ItemKind.Normal,
                displayNameOverride
            );
        
        public static ReadOnlySeString CreateItemLink
        (
            uint     itemID,
            ItemKind kind                = ItemKind.Normal,
            string?  displayNameOverride = null
        )
        {
            var rawID = ItemUtil.GetRawId(itemID, kind);

            var displayName = displayNameOverride ?? ItemUtil.GetItemName(rawID);
            if (displayName.IsEmpty)
                throw new Exception("无法确定物品名称。");

            var textColor     = ItemUtil.GetItemRarityColorType(rawID);
            var textEdgeColor = textColor + 1u;

            using var rssb    = new RentedSeStringBuilder();
            var       builder = rssb.Builder;

            var itemName = rssb.Builder
                               .PushColorType(textColor)
                               .PushEdgeColorType(textEdgeColor)
                               .Append(displayName)
                               .PopEdgeColorType()
                               .PopColorType()
                               .ToReadOnlySeString();

            itemName = ISeStringEvaluator.Instance().EvaluateFromAddon(371, [itemName]);
            
            builder.Clear();

            builder.PushLinkItem(itemID)
                   .Append(itemName)
                   .PopLink();
            
            return builder.ToReadOnlySeString();
        }
        
        public static ReadOnlySeString CreateItemName
        (
            uint    itemID,
            bool    isHQ                = false,
            string? displayNameOverride = null
        ) =>
            ReadOnlySeString.CreateItemName
            (
                itemID,
                isHQ ?
                    ItemKind.Hq :
                    ItemKind.Normal,
                displayNameOverride
            );

        /// <summary>
        ///     非链接，仅为游戏原生风格的物品富文本
        /// </summary>
        public static ReadOnlySeString CreateItemName
        (
            uint     itemID,
            ItemKind kind                = ItemKind.Normal,
            string?  displayNameOverride = null
        )
        {
            var rawID = ItemUtil.GetRawId(itemID, kind);

            var displayName = displayNameOverride ?? ItemUtil.GetItemName(rawID);
            if (displayName.IsEmpty)
                throw new Exception("无法确定物品名称。");

            var textColor     = ItemUtil.GetItemRarityColorType(rawID);
            var textEdgeColor = textColor + 1u;

            using var rssb = new RentedSeStringBuilder();

            var itemName = rssb.Builder
                               .PushColorType(textColor)
                               .PushEdgeColorType(textEdgeColor)
                               .Append(displayName)
                               .PopEdgeColorType()
                               .PopColorType()
                               .ToReadOnlySeString();

            return itemName;
        }

        public static ReadOnlySeString Format
        (
            string          value,
            params object[] args
        ) =>
            ReadOnlySeString.Format(value, CultureInfo.CurrentCulture, args, null);

        public static ReadOnlySeString Format
        (
            string                              value,
            IReadOnlyDictionary<string, object> args,
            Language                            language,
            Action<string>?                     onFormatError = null
        )
        {
            ArgumentNullException.ThrowIfNull(args);

            var culture = ResolveCulture(language);

            Dictionary<string, object>? restored = null;
            Dictionary<string, object>? plain    = null;

            foreach (var (name, argValue) in args)
            {
                if (!IsSeStringArgument(argValue))
                    continue;

                restored ??= new(StringComparer.Ordinal);
                plain    ??= new(StringComparer.Ordinal);

                var token = $"<se:{restored.Count}>";
                restored[token] = argValue;
                plain[name]     = token;
            }

            if (plain != null)
            {
                foreach (var (name, argValue) in args)
                    plain.TryAdd(name, argValue);
            }

            string message;

            try
            {
                message = MessageFormatter.Format(value, plain ?? args, culture);
            }
            catch (Exception ex)
            {
                onFormatError?.Invoke(ex.Message);
                return value;
            }

            if (restored == null)
                return message;

            using var rented = new RentedSeStringBuilder();
            AppendWithSeStringArguments(rented.Builder, message, restored, culture);
            return rented.Builder.ToReadOnlySeString();
        }

        public static string FormatText
        (
            string                              value,
            IReadOnlyDictionary<string, object> args,
            Language                            language,
            Action<string>?                     onFormatError = null
        ) =>
            ReadOnlySeString.Format(value, args, language, onFormatError).ExtractText();

        public static ReadOnlySeString Format
        (
            string           value,
            IFormatProvider? provider,
            params object[]  args
        ) =>
            ReadOnlySeString.Format(value, provider ?? CultureInfo.CurrentCulture, args, null);

        internal static ReadOnlySeString Format
        (
            string          value,
            IFormatProvider provider,
            object[]        args,
            Action<string>? onFormatError
        )
        {
            ArgumentNullException.ThrowIfNull(args);

            using var rented = new RentedSeStringBuilder();
            AppendFormattedText(rented.Builder, value, args, provider, onFormatError);
            return rented.Builder.ToReadOnlySeString();
        }
    }

    private static bool IsSeStringArgument
    (
        object? value
    ) =>
        value is DSeString or
            DSeStringBuilder or
            Payload or
            ReadOnlySeString or
            ReadOnlySePayload or
            RentedSeStringBuilder;

    private static CultureInfo ResolveCulture
    (
        Language language
    ) =>
        language switch
        {
            Language.Japanese           => CultureInfo.GetCultureInfo("ja-JP"),
            Language.English            => CultureInfo.GetCultureInfo("en-US"),
            Language.German             => CultureInfo.GetCultureInfo("de-DE"),
            Language.French             => CultureInfo.GetCultureInfo("fr-FR"),
            Language.ChineseSimplified  => CultureInfo.GetCultureInfo("zh-CN"),
            Language.TraditionalChinese => CultureInfo.GetCultureInfo("zh-TW"),
            Language.Korean             => CultureInfo.GetCultureInfo("ko-KR"),
            _                           => CultureInfo.InvariantCulture
        };

    private static void AppendWithSeStringArguments
    (
        LSeStringBuilder           builder,
        string                     message,
        Dictionary<string, object> restored,
        IFormatProvider            provider
    )
    {
        var start = 0;

        while (start < message.Length)
        {
            var next = message.IndexOf("<se:", start, StringComparison.Ordinal);

            if (next < 0)
            {
                builder.Append(message.AsSpan(start));
                return;
            }

            var end = message.IndexOf('>', next);

            if (end < 0)
            {
                builder.Append(message.AsSpan(start));
                return;
            }

            var token = message[next..(end + 1)];

            if (!restored.TryGetValue(token, out var value))
            {
                builder.Append(message.AsSpan(start, end + 1 - start));
                start = end + 1;
                continue;
            }

            builder.Append(message.AsSpan(start, next - start));
            AppendFormattedArgument(builder, value, provider);
            start = end + 1;
        }
    }

    private static void AppendFormattedText
    (
        LSeStringBuilder builder,
        string           text,
        object[]         args,
        IFormatProvider  provider,
        Action<string>?  onFormatError
    )
    {
        for (var index = 0; index < text.Length;)
        {
            var current = text[index];

            switch (current)
            {
                case '{' when index + 1 < text.Length && text[index + 1] == '{':
                    builder.Append('{');
                    index += 2;
                    continue;
                case '}' when index + 1 < text.Length && text[index + 1] == '}':
                    builder.Append('}');
                    index += 2;
                    continue;
            }

            if (current != '{' || !TryReadPlaceholder(text, index, out var nextIndex, out var argumentIndex))
            {
                builder.Append(current.ToString());
                index++;
                continue;
            }

            var token = text[index..nextIndex];

            if ((uint)argumentIndex < (uint)args.Length)
                AppendFormattedArgument(builder, args[argumentIndex], provider);
            else
            {
                onFormatError?.Invoke(token);
                builder.Append(token);
            }

            index = nextIndex;
        }
    }

    internal static void AppendFormattedArgument
    (
        LSeStringBuilder builder,
        object?          value,
        IFormatProvider  provider
    )
    {
        switch (value)
        {
            case null:
                return;
            case DSeString dalamudSeString:
                builder.AppendDalamudSeString(dalamudSeString);
                return;
            case DSeStringBuilder dalamudSeStringBuilder:
                builder.AppendDalamudSeString(dalamudSeStringBuilder);
                return;
            case Payload payload:
                builder.AppendDalamudSeString(payload);
                return;
            case ReadOnlySeString readOnlySeString:
                builder.Append(readOnlySeString);
                return;
            case ReadOnlySePayload readOnlySePayload:
                builder.Append(readOnlySePayload);
                return;
            case RentedSeStringBuilder rentedSeStringBuilder:
                builder.AppendRentedSeStringBuilder(rentedSeStringBuilder);
                return;
            case LSeStringBuilder seStringBuilder:
                builder.Append(seStringBuilder.ToReadOnlySeString());
                return;
            case BitmapFontIcon icon:
                builder.AppendIcon(icon);
                return;
            case SeIconChar iconChar:
                builder.AppendIcon(iconChar);
                return;
            case IFormattable formattable:
                builder.AppendFormattable(formattable, provider);
                return;
            default:
                builder.Append(value.ToString() ?? string.Empty);
                return;
        }
    }

    private static bool TryReadPlaceholder
    (
        string  format,
        int     startIndex,
        out int nextIndex,
        out int argumentIndex
    )
    {
        nextIndex     = startIndex;
        argumentIndex = -1;

        var index = startIndex + 1;
        if (index >= format.Length || !char.IsAsciiDigit(format[index]))
            return false;

        var value = 0;

        while (index < format.Length && char.IsAsciiDigit(format[index]))
        {
            value = (value * 10) + format[index] - '0';
            index++;
        }

        if (index >= format.Length || format[index] != '}')
            return false;

        argumentIndex = value;
        nextIndex     = index + 1;
        return true;
    }

    extension
    (
        string input
    )
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SanitizeSEIcon() => input.AsSpan().SanitizeSEIcon();
    }

    extension
    (
        ReadOnlySpan<char> input
    )
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

    extension
    (
        ReadOnlySeStringSpan span
    )
    {
        public ReadOnlySeString PraseAutoTranslate()
        {
            using var rentedOuter = new RentedSeStringBuilder();
            var       builder     = rentedOuter.Builder;

            var counter = -1;

            foreach (var payload in span)
            {
                counter++;

                using var rented = new RentedSeStringBuilder();

                if (payload.Type            != ReadOnlySePayloadType.Macro  ||
                    payload.MacroCode       != MacroCode.Fixed              ||
                    payload.ExpressionCount != 2                            ||
                    !payload.TryGetExpression(out var expr1, out var expr2) ||
                    !expr1.TryGetUInt(out var group)                        ||
                    !expr2.TryGetUInt(out var rowID)                        ||
                    !LuminaGetter.TryGetRow(rowID, out Completion macroRow) ||
                    macroRow.Group != group + 1)
                {

                    if (counter      == 0                          &&
                        payload.Type == ReadOnlySePayloadType.Text &&
                        string.IsNullOrEmpty(payload.ToString().Trim()))
                        continue;

                    builder.Append(rented.Builder.Append(payload).ToReadOnlySeString());
                    continue;
                }

                builder.Append(payload);
            }

            return builder.ToReadOnlySeString();
        }
    }

    extension
    (
        char c
    )
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSEIcon
        (
            int     start,
            ulong[] bitmap
        )
        {
            var adjustedValue = c - start;
            var index         = adjustedValue >> 6;
            var bit           = adjustedValue              & 63;
            return index < bitmap.Length && (bitmap[index] & (1UL << bit)) != 0;
        }
    }
}
