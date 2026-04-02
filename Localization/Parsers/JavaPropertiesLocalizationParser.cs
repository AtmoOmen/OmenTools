using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using OmenTools.Localization.Abstractions;

namespace OmenTools.Localization.Parsers;

public sealed class JavaPropertiesLocalizationParser
(
    Encoding? encoding = null
) : ILocalizationParser
{
    private readonly Encoding encoding = encoding ?? Encoding.UTF8;

    public FrozenDictionary<string, string> Parse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var                  reader = new StreamReader(stream, encoding, true, leaveOpen: true);
        Dictionary<string, string> result = new(StringComparer.Ordinal);

        foreach (var line in ReadLogicalLines(reader))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith('#') || trimmed.StartsWith('!'))
                continue;

            var (rawKey, rawValue) = SplitKeyValue(line);
            var key = Unescape(rawKey);

            if (string.IsNullOrWhiteSpace(key))
                continue;

            result[key] = Unescape(rawValue);
        }

        return result.ToFrozenResource();
    }

    private static IEnumerable<string> ReadLogicalLines(StreamReader reader)
    {
        StringBuilder? builder = null;

        while (reader.ReadLine() is { } line)
        {
            var segment = builder == null ? line : TrimLeadingWhitespace(line);

            if (!HasContinuation(segment))
            {
                if (builder == null)
                {
                    yield return segment;
                    continue;
                }

                builder.Append(segment);
                yield return builder.ToString();
                builder = null;
                continue;
            }

            segment = segment[..^1];

            builder ??= new();
            builder.Append(segment);
        }

        if (builder is { Length: > 0 })
            yield return builder.ToString();
    }

    private static string TrimLeadingWhitespace(string value)
    {
        var index = 0;
        while (index < value.Length && char.IsWhiteSpace(value[index]))
            index++;

        return index == 0 ? value : value[index..];
    }

    private static bool HasContinuation(string value)
    {
        var backslashCount = 0;

        for (var index = value.Length - 1; index >= 0 && value[index] == '\\'; index--)
            backslashCount++;

        return backslashCount % 2 == 1;
    }

    private static (string Key, string Value) SplitKeyValue(string line)
    {
        var separatorIndex = -1;
        var escaped        = false;

        for (var index = 0; index < line.Length; index++)
        {
            var current = line[index];

            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (current == '\\')
            {
                escaped = true;
                continue;
            }

            if (current is '=' or ':' || char.IsWhiteSpace(current))
            {
                separatorIndex = index;
                break;
            }
        }

        if (separatorIndex < 0)
            return (line.Trim(), string.Empty);

        var keyEnd = separatorIndex;
        while (keyEnd > 0 && char.IsWhiteSpace(line[keyEnd - 1]))
            keyEnd--;

        var valueStart = separatorIndex;
        while (valueStart < line.Length && char.IsWhiteSpace(line[valueStart]))
            valueStart++;

        if (valueStart < line.Length && line[valueStart] is '=' or ':')
            valueStart++;

        while (valueStart < line.Length && char.IsWhiteSpace(line[valueStart]))
            valueStart++;

        return (line[..keyEnd], valueStart < line.Length ? line[valueStart..] : string.Empty);
    }

    private static string Unescape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        StringBuilder builder = new(value.Length);

        for (var index = 0; index < value.Length; index++)
        {
            var current = value[index];

            if (current != '\\' || index == value.Length - 1)
            {
                builder.Append(current);
                continue;
            }

            var escaped = value[++index];

            switch (escaped)
            {
                case 't':
                    builder.Append('\t');
                    break;
                case 'r':
                    builder.Append('\r');
                    break;
                case 'n':
                    builder.Append('\n');
                    break;
                case 'f':
                    builder.Append('\f');
                    break;
                case '\\':
                    builder.Append('\\');
                    break;
                case ' ':
                    builder.Append(' ');
                    break;
                case ':':
                    builder.Append(':');
                    break;
                case '=':
                    builder.Append('=');
                    break;
                case '#':
                    builder.Append('#');
                    break;
                case '!':
                    builder.Append('!');
                    break;
                case 'u' when index + 4 < value.Length:
                    if (ushort.TryParse(value.AsSpan(index + 1, 4), NumberStyles.HexNumber, null, out var unicode))
                    {
                        builder.Append((char)unicode);
                        index += 4;
                        break;
                    }

                    builder.Append("\\u");
                    break;
                default:
                    builder.Append(escaped);
                    break;
            }
        }

        return builder.ToString();
    }
}
