using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace OmenTools.Extensions;

public static partial class StringExtension
{
    private const int TIMEOUT_MS = 1000;

    [GeneratedRegex(@"^---[\s\S]*?---\s*$", RegexOptions.Multiline, TIMEOUT_MS)]
    private static partial Regex FrontMatterRegex();

    [GeneratedRegex(@":::[^\n]*\n|:::|:::\s*$", RegexOptions.Multiline, TIMEOUT_MS)]
    private static partial Regex CustomContainerRegex();

    [GeneratedRegex(@"`{3,}\w*\s*([\s\S]*?)\s*`{3,}", RegexOptions.None, TIMEOUT_MS)]
    private static partial Regex CodeFenceRegex();

    [GeneratedRegex(@"^#{1,6}\s+(.*)$", RegexOptions.Multiline, TIMEOUT_MS)]
    private static partial Regex HeaderRegex();

    [GeneratedRegex(@"!\[([^\]]*)\]\([^)]+\)", RegexOptions.None, TIMEOUT_MS)]
    private static partial Regex ImageRegex();

    [GeneratedRegex(@"(?<!\!)\[([^\]]*)\]\([^)]+\)", RegexOptions.None, TIMEOUT_MS)]
    private static partial Regex LinkRegex();

    [GeneratedRegex("~~(.*?)~~", RegexOptions.Singleline, TIMEOUT_MS)]
    private static partial Regex StrikeThroughRegex();

    [GeneratedRegex("`([^`]+)`", RegexOptions.Singleline, TIMEOUT_MS)]
    private static partial Regex InlineCodeRegex();

    [GeneratedRegex("==([^=]+)==", RegexOptions.Singleline, TIMEOUT_MS)]
    private static partial Regex HighlightRegex();

    [GeneratedRegex(@"\+\+([^+]+)\+\+", RegexOptions.Singleline, TIMEOUT_MS)]
    private static partial Regex InsertRegex();

    [GeneratedRegex(@"^>\s?(.*)$", RegexOptions.Multiline, TIMEOUT_MS)]
    private static partial Regex BlockquoteRegex();

    [GeneratedRegex(@"^[\s]*([-*+]|\d+.)\s+(.*)$", RegexOptions.Multiline, TIMEOUT_MS)]
    private static partial Regex ListRegex();

    [GeneratedRegex(@"\[\^(.*?)\]:?", RegexOptions.None, TIMEOUT_MS)]
    private static partial Regex FootnoteRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.None, TIMEOUT_MS)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"^\|.*?\|\s*$", RegexOptions.Multiline, TIMEOUT_MS)]
    private static partial Regex TableRegex();

    [GeneratedRegex(@"\n{2,}", RegexOptions.None, TIMEOUT_MS)]
    private static partial Regex MultipleBlankLineRegex();

    [GeneratedRegex("[ ]{2,}", RegexOptions.None, TIMEOUT_MS)]
    private static partial Regex MultipleSpaceRegex();

    extension(char c)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFullWidth() =>
            c is >= '\uFF01' and <= '\uFF5E' or '\u3000';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHalfWidth() =>
            c is >= '\u0020' and <= '\u007E';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ToHalfWidth() =>
            c switch
            {
                >= '\uFF01' and <= '\uFF5E' => (char)(c - 0xFEE0),
                '\u3000'                    => '\u0020',
                _                           => c
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ToFullWidth() =>
            c switch
            {
                >= '\u0021' and <= '\u007E' => (char)(c + 0xFEE0),
                '\u0020'                    => '\u3000',
                _                           => c
            };
    }

    extension(string input)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToLowerAndHalfWidth()
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public bool TryReplaceIgnoreCase(string target, string replacement, [NotNullWhen(true)] out string? result)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(target))
            {
                result = null;
                return false;
            }

            var inputSpan  = input.AsSpan();
            var targetSpan = target.AsSpan();

            var firstIndex = inputSpan.IndexOf(targetSpan, StringComparison.OrdinalIgnoreCase);

            if (firstIndex == -1)
            {
                result = null;
                return false;
            }

            var inputLen   = input.Length;
            var targetLen  = target.Length;
            var replaceLen = replacement.Length;

            if (targetLen == replaceLen)
            {
                var state = (Input: input, Target: target, Replacement: replacement, FirstIndex: firstIndex);

                result = string.Create
                (
                    inputLen,
                    state,
                    static (span, s) =>
                    {
                        var srcSpan = s.Input.AsSpan();
                        var tSpan   = s.Target.AsSpan();
                        var rSpan   = s.Replacement.AsSpan();
                        var tLen    = tSpan.Length;

                        ref var destRef = ref MemoryMarshal.GetReference(span);
                        ref var srcRef  = ref MemoryMarshal.GetReference(srcSpan);

                        var currentPos = 0;
                        var matchIdx   = s.FirstIndex;

                        while (true)
                        {
                            var count = matchIdx - currentPos;

                            if (count > 0)
                                Unsafe.CopyBlockUnaligned
                                (
                                    ref Unsafe.As<char, byte>(ref Unsafe.Add(ref destRef, currentPos)),
                                    ref Unsafe.As<char, byte>(ref Unsafe.Add(ref srcRef,  currentPos)),
                                    (uint)(count * 2)
                                );

                            rSpan.CopyTo(span[matchIdx..]);

                            currentPos = matchIdx + tLen;

                            var remainingSpan = srcSpan[currentPos..];
                            var nextRel       = remainingSpan.IndexOf(tSpan, StringComparison.OrdinalIgnoreCase);

                            if (nextRel == -1) break;

                            matchIdx = currentPos + nextRel;
                        }

                        var remaining = srcSpan.Length - currentPos;

                        if (remaining > 0)
                            Unsafe.CopyBlockUnaligned
                            (
                                ref Unsafe.As<char, byte>(ref Unsafe.Add(ref destRef, currentPos)),
                                ref Unsafe.As<char, byte>(ref Unsafe.Add(ref srcRef,  currentPos)),
                                (uint)(remaining * 2)
                            );
                    }
                );
                return true;
            }

            long maxRequiredLength = inputLen;

            if (replaceLen > targetLen)
            {
                var growth = (long)(inputLen / targetLen) * (replaceLen - targetLen);
                maxRequiredLength += growth;
            }

            char[]? rentedArray = null;
            var buffer = maxRequiredLength <= 512
                             ? stackalloc char[512]
                             : rentedArray = ArrayPool<char>.Shared.Rent((int)maxRequiredLength);

            try
            {
                ref var bufferStart = ref MemoryMarshal.GetReference(buffer);
                ref var inputStart  = ref MemoryMarshal.GetReference(inputSpan);

                var writePos           = 0;
                var currentSearchStart = 0;
                var matchIndex         = firstIndex;

                var replaceSpan = replacement.AsSpan();

                while (true)
                {
                    var count = matchIndex - currentSearchStart;

                    if (count > 0)
                    {
                        Unsafe.CopyBlockUnaligned
                        (
                            ref Unsafe.As<char, byte>(ref Unsafe.Add(ref bufferStart, writePos)),
                            ref Unsafe.As<char, byte>(ref Unsafe.Add(ref inputStart,  currentSearchStart)),
                            (uint)(count * 2)
                        );
                        writePos += count;
                    }

                    if (replaceLen > 0)
                    {
                        replaceSpan.CopyTo(buffer[writePos..]);
                        writePos += replaceLen;
                    }

                    currentSearchStart = matchIndex + targetLen;

                    if (currentSearchStart >= inputLen) break;

                    var searchSlice       = inputSpan[currentSearchStart..];
                    var nextRelativeIndex = searchSlice.IndexOf(targetSpan, StringComparison.OrdinalIgnoreCase);

                    if (nextRelativeIndex == -1) break;

                    matchIndex = currentSearchStart + nextRelativeIndex;
                }

                var remaining = inputLen - currentSearchStart;

                if (remaining > 0)
                {
                    Unsafe.CopyBlockUnaligned
                    (
                        ref Unsafe.As<char, byte>(ref Unsafe.Add(ref bufferStart, writePos)),
                        ref Unsafe.As<char, byte>(ref Unsafe.Add(ref inputStart,  currentSearchStart)),
                        (uint)(remaining * 2)
                    );
                    writePos += remaining;
                }

                result = new string(buffer[..writePos]);
                return true;
            }
            finally
            {
                if (rentedArray != null)
                    ArrayPool<char>.Shared.Return(rentedArray);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsChinese()
        {
            if (string.IsNullOrEmpty(input))
                return false;

            foreach (var rune in input.EnumerateRunes())
            {
                if (!string.IsChineseRune(rune))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAnyChinese()
        {
            if (string.IsNullOrEmpty(input))
                return false;

            foreach (var rune in input.EnumerateRunes())
            {
                if (string.IsChineseRune(rune))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsChineseRune(Rune rune)
        {
            var value = (uint)rune.Value;

            return value - 0x4E00u  <= 0x9FFFu  - 0x4E00u  ||
                   value - 0x3400u  <= 0x4DBFu  - 0x3400u  ||
                   value - 0x20000u <= 0x2FA1Fu - 0x20000u ||
                   value - 0x30000u <= 0x323AFu - 0x30000u ||
                   value - 0xF900u  <= 0xFAFFu  - 0xF900u;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string SanitizeMarkdown()
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = FrontMatterRegex().Replace(input, string.Empty);
            input = CustomContainerRegex().Replace(input, string.Empty);
            input = CodeFenceRegex().Replace(input, "$1");
            input = ImageRegex().Replace(input, "$1");
            input = LinkRegex().Replace(input, "$1");
            input = FootnoteRegex().Replace(input, string.Empty);
            input = HeaderRegex().Replace(input, "$1");
            input = StrikeThroughRegex().Replace(input, "$1");
            input = HighlightRegex().Replace(input, "$1");
            input = InsertRegex().Replace(input, "$1");
            input = InlineCodeRegex().Replace(input, "$1");
            input = BlockquoteRegex().Replace(input, "$1");
            input = ListRegex().Replace(input, "$2");
            input = HtmlTagRegex().Replace(input, string.Empty);
            input = TableRegex().Replace(input, " ");
            input = MultipleBlankLineRegex().Replace(input, "\n");
            input = MultipleSpaceRegex().Replace(input, " ");

            return input.Trim();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? NullIfEmpty() =>
            string.IsNullOrEmpty(input) ? null : input;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? NullIfWhitespace() =>
            string.IsNullOrWhiteSpace(input) ? null : input;
    }
}
