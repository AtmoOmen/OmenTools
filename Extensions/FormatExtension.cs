using System.Globalization;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Utility;
using Jeffijoe.MessageFormat;
using Lumina.Data;
using Lumina.Text.ReadOnly;
using DSeString = Dalamud.Game.Text.SeStringHandling.SeString;
using DSeStringBuilder = Dalamud.Game.Text.SeStringHandling.SeStringBuilder;
using LSeStringBuilder = Lumina.Text.SeStringBuilder;

namespace OmenTools.Extensions;

public static class FormatExtension
{
    extension
    (
        ReadOnlySeString
    )
    {
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
}
