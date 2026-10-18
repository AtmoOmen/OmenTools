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

public static class SeStringFormatExtension
{
    extension
    (
        ReadOnlySeString
    )
    {
        public static ReadOnlySeString FormatText
        (
            string          value,
            params object[] args
        ) =>
            FormatTextCore(value, CultureInfo.CurrentCulture, args, null);

        public static ReadOnlySeString FormatText
        (
            string           value,
            IFormatProvider? provider,
            params object[]  args
        ) =>
            FormatTextCore(value, provider ?? CultureInfo.CurrentCulture, args, null);

        public static ReadOnlySeString FormatICUText
        (
            string                              value,
            IReadOnlyDictionary<string, object> args,
            Language                            language,
            Action<string>?                     onFormatError = null
        )
        {
            ArgumentNullException.ThrowIfNull(args);

            var culture = language.ToCultureInfo();

            var hasSeStringArgument = false;

            foreach (var (_, argValue) in args)
            {
                if (!AppendFormattedArgument(null, argValue, culture))
                    continue;

                hasSeStringArgument = true;
                break;
            }

            Dictionary<string, object>? restored = null;
            Dictionary<string, object>? plain    = null;

            if (hasSeStringArgument)
            {
                restored = new(args.Count, StringComparer.Ordinal);
                plain    = new(args.Count, StringComparer.Ordinal);

                foreach (var (name, argValue) in args)
                {
                    if (AppendFormattedArgument(null, argValue, culture))
                    {
                        var token = $"<se:{restored.Count}>";
                        restored[token] = argValue;
                        plain[name]     = token;
                        continue;
                    }

                    plain[name] = argValue;
                }
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

            using var rented  = new RentedSeStringBuilder();
            var       builder = rented.Builder;
            var       start   = 0;

            while (start < message.Length)
            {
                var next = message.IndexOf("<se:", start, StringComparison.Ordinal);

                if (next < 0)
                {
                    builder.Append(message.AsSpan(start));
                    break;
                }

                var end = message.IndexOf('>', next);

                if (end < 0)
                {
                    builder.Append(message.AsSpan(start));
                    break;
                }

                var token = message[next..(end + 1)];

                if (!restored.TryGetValue(token, out var argValue))
                {
                    builder.Append(message.AsSpan(start, end + 1 - start));
                    start = end + 1;
                    continue;
                }

                builder.Append(message.AsSpan(start, next - start));
                AppendFormattedArgument(builder, argValue, culture);
                start = end + 1;
            }

            return builder.ToReadOnlySeString();
        }

        public static ReadOnlySeString FormatText
        (
            string          value,
            IFormatProvider provider,
            object[]        args,
            Action<string>? onFormatError
        ) =>
            FormatTextCore(value, provider, args, onFormatError);
    }

    private static ReadOnlySeString FormatTextCore
    (
        string          value,
        IFormatProvider provider,
        object[]        args,
        Action<string>? onFormatError
    )
    {
        ArgumentNullException.ThrowIfNull(args);

        var template = value.AsSpan();

        using var rented  = new RentedSeStringBuilder();
        var       builder = rented.Builder;
        var       start   = 0;
        var       index   = 0;

        while (index < template.Length)
        {
            switch (template[index])
            {
                case '{' when index + 1 < template.Length && template[index + 1] == '{':
                    builder.Append(template[start..index]);
                    builder.Append('{');
                    index += 2;
                    start =  index;
                    continue;
                case '}' when index + 1 < template.Length && template[index + 1] == '}':
                    builder.Append(template[start..index]);
                    builder.Append('}');
                    index += 2;
                    start =  index;
                    continue;
                case '{':
                    var digitEnd = index + 1;

                    while (digitEnd < template.Length && char.IsAsciiDigit(template[digitEnd]))
                        digitEnd++;

                    if (digitEnd > index + 1 && digitEnd < template.Length && template[digitEnd] == '}')
                    {
                        builder.Append(template[start..index]);

                        var token = template[index..(digitEnd + 1)];

                        if (int.TryParse(template[(index + 1)..digitEnd], NumberStyles.None, CultureInfo.InvariantCulture, out var argumentIndex) &&
                            argumentIndex < args.Length)
                            AppendFormattedArgument(builder, args[argumentIndex], provider);
                        else
                        {
                            onFormatError?.Invoke(token.ToString());
                            builder.Append(token);
                        }

                        index = digitEnd + 1;
                        start = index;
                        continue;
                    }

                    break;
            }

            index++;
        }

        builder.Append(template[start..]);
        return builder.ToReadOnlySeString();
    }

    private static bool AppendFormattedArgument
    (
        LSeStringBuilder? builder,
        object?           value,
        IFormatProvider   provider
    )
    {
        switch (value)
        {
            case null:
                return false;
            case DSeString dalamudSeString:
                builder?.AppendDalamudSeString(dalamudSeString);
                return true;
            case DSeStringBuilder dalamudSeStringBuilder:
                builder?.AppendDalamudSeString(dalamudSeStringBuilder);
                return true;
            case Payload payload:
                builder?.AppendDalamudSeString(payload);
                return true;
            case ReadOnlySeString readOnlySeString:
                builder?.Append(readOnlySeString);
                return true;
            case ReadOnlySePayload readOnlySePayload:
                builder?.Append(readOnlySePayload);
                return true;
            case RentedSeStringBuilder rentedSeStringBuilder:
                builder?.AppendRentedSeStringBuilder(rentedSeStringBuilder);
                return true;
            case LSeStringBuilder seStringBuilder:
                builder?.Append(seStringBuilder.ToReadOnlySeString());
                return true;
            case BitmapFontIcon icon:
                builder?.AppendIcon(icon);
                return true;
            case SeIconChar iconChar:
                builder?.AppendIcon(iconChar);
                return true;
            case IFormattable formattable:
                builder?.AppendFormattable(formattable, provider);
                return false;
            default:
                builder?.Append(value.ToString() ?? string.Empty);
                return false;
        }
    }
}
