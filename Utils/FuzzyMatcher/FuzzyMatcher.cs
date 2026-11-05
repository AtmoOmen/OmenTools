using System.Buffers;
using System.Globalization;
using System.Numerics;
using System.Text;
using TinyPinyin;

namespace OmenTools.Utils.FuzzyMatcher;

/// <summary>
///     预先建立文本与拼音索引，按字段相关度执行多词、精确短语和排除词搜索。
/// </summary>
/// <typeparam name="T">索引中保存并由搜索返回的数据类型。</typeparam>
public sealed class FuzzyMatcher<T> : IDisposable
{
    private          SearchEntry[]    entries;
    private readonly IComparer<Match> matchComparer;
    private readonly CompareInfo      titleCompareInfo = CultureInfo.GetCultureInfo("zh-CN").CompareInfo;

    private volatile CacheEntry[] cache = [];

    /// <summary>
    ///     根据调用方定义的字段建立索引，预先计算文本、拼音、字符掩码和排序键。
    /// </summary>
    /// <param name="items">需要建立索引的数据。</param>
    /// <param name="fieldsSelector">每项数据的搜索字段；首个字段同时提供标题排序文本。</param>
    /// <param name="tieBreaker">相关度与标题均相同时使用的最终排序规则。</param>
    public FuzzyMatcher
    (
        IEnumerable<T>                         items,
        Func<T, IEnumerable<FuzzySearchField>> fieldsSelector,
        Comparison<T>?                         tieBreaker = null
    )
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(fieldsSelector);

        matchComparer = new MatchComparer(tieBreaker);
        var readings = new Dictionary<char, string[]>();
        entries =
        [
            .. items.Where(static item => item is not null)
                    .Select(item => new SearchEntry(item, fieldsSelector(item), readings, titleCompareInfo))
                    .Where(static entry => entry.Fields.Length > 0)
        ];

        for (var index = 0; index < entries.Length; index++)
            entries[index].Order = index;
    }

    /// <summary>
    ///     按相关度返回全部匹配项；空查询返回空数组，重复查询直接复用缓存结果。
    /// </summary>
    /// <param name="query">支持多词、引号短语及减号排除词的查询文本。</param>
    /// <returns>按最弱词匹配、标题短语、总分和标题排序的结果数组。</returns>
    public T[] Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        foreach (var cached in cache)
        {
            if (string.Equals(cached.Query, query, StringComparison.Ordinal))
                return cached.Results;
        }

        var terms = ParseTerms(query);
        if (terms.Length == 0)
            return [];

        var results = SearchCore(terms);
        Store(query, results);
        return results;
    }

    /// <summary>
    ///     释放索引和查询缓存持有的数据引用。
    /// </summary>
    public void Dispose()
    {
        entries = [];
        cache   = [];
    }

    private T[] SearchCore
    (
        SearchTerm[] terms
    )
    {
        var phrase     = BuildPhrase(terms);
        var phraseTerm = default(SearchTerm);
        var hasPhrase  = false;

        if (phrase != null)
        {
            phraseTerm = new(phrase, false, true);
            hasPhrase  = true;
        }

        var buffer = ArrayPool<Match>.Shared.Rent
        (
            entries.Length > 0 ?
                entries.Length :
                1
        );

        try
        {
            var matches = buffer.AsSpan();
            var count   = 0;

            foreach (var entry in entries)
            {
                var weakest = int.MaxValue;
                var total   = 0;
                var matched = true;

                foreach (var term in terms)
                {
                    if (term.IsASCIIAlphanumeric &&
                        entry.MaskReliable       &&
                        (entry.ConsumableMask & term.Mask) != term.Mask)
                    {
                        if (term.Excluded)
                            continue;

                        matched = false;
                        break;
                    }

                    var best = 0;

                    foreach (var field in entry.Fields)
                    {
                        best = Math.Max(best, ScoreField(field, in term));

                        if (best >= field.MaxScore)
                            break;
                    }

                    if (term.Excluded)
                    {
                        if (best == 0)
                            continue;

                        matched = false;
                        break;
                    }

                    if (best == 0)
                    {
                        matched = false;
                        break;
                    }

                    weakest =  Math.Min(weakest, best);
                    total   += best;
                }

                if (!matched)
                    continue;

                var phraseScore = 0;

                if (hasPhrase)
                    phraseScore = ScoreField(entry.Fields[0], in phraseTerm);

                matches[count++] = new
                (
                    entry,
                    total > 0 ?
                        weakest :
                        0,
                    total,
                    phraseScore
                );
            }

            matches = matches[..count];
            matches.Sort(matchComparer);

            var results = new T[count];

            for (var index = 0; index < count; index++)
                results[index] = matches[index].Entry.Item;

            return results;
        }
        finally
        {
            ArrayPool<Match>.Shared.Return(buffer, true);
        }
    }

    private static string? BuildPhrase
    (
        SearchTerm[] terms
    )
    {
        var length = 0;

        foreach (var term in terms)
        {
            if (!term.Excluded)
                length += term.Text.Length;
        }

        if (length == 0)
            return null;

        return string.Create
        (
            length,
            terms,
            static (span, source) =>
            {
                var offset = 0;

                foreach (var term in source)
                {
                    if (term.Excluded)
                        continue;

                    term.Text.CopyTo(span[offset..]);
                    offset += term.Text.Length;
                }
            }
        );
    }

    private void Store
    (
        string query,
        T[]    results
    )
    {
        var current = cache;
        var next    = new CacheEntry[Math.Min(current.Length + 1, CACHE_CAPACITY)];
        next[0] = new(query, results);
        Array.Copy(current, 0, next, 1, Math.Min(current.Length, CACHE_CAPACITY - 1));
        cache = next;
    }

    private static int ScoreField
    (
        SearchField   field,
        in SearchTerm term
    )
    {
        var text = term.Text;
        var literal = term.IsASCIIAlphanumeric ?
                          field.LiteralText :
                          field.Text;
        var position = literal.IndexOf(text, StringComparison.Ordinal);

        if (position >= 0)
        {
            if (!term.Excluded              &&
                text.Length == 1            &&
                !field.AllowSingleCharacter &&
                field.Text.Length != 1)
                return 0;

            if (field.Text.Length == text.Length)
                return field.NativeWeight + 90;

            if (field.ShortMatchPenalty > 0 &&
                text.Length             <= 3)
                return field.NativeWeight - field.ShortMatchPenalty;

            return field.NativeWeight +
                   (position == 0 ?
                        60 :
                        30);
        }

        if (term.Exact ||
            term.Excluded)
            return ScorePinyin(field, in term, false);

        if (field.AllowChineseSplit &&
            text.Length >= 4        &&
            term.IsChinese)
        {
            for (var split = 2; split <= text.Length - 2; split++)
            {
                var left      = text.AsSpan(0, split);
                var right     = text.AsSpan(split);
                var remaining = field.Text.AsSpan();
                var offset    = 0;

                while (remaining.IndexOf(left) is var start &&
                       start >= 0)
                {
                    var begin     = offset + start;
                    var end       = begin  + left.Length;
                    var length    = Math.Min(field.Text.Length - end, right.Length + MAX_CHINESE_GAP);
                    var preceding = Math.Max(0, begin          - right.Length      - MAX_CHINESE_GAP);
                    if (field.Text.AsSpan(end,       length).Contains(right, StringComparison.Ordinal) ||
                        field.Text.AsSpan(preceding, begin - preceding).Contains(right, StringComparison.Ordinal))
                        return field.NativeWeight - 100;

                    offset    += start + 1;
                    remaining =  field.Text.AsSpan(offset);
                }
            }
        }

        if (field.AllowAcronym &&
            text.Length >= 2   &&
            field.Acronym.Contains(text, StringComparison.Ordinal))
            return field.NativeWeight - 200;

        return ScorePinyin(field, in term, true);
    }

    private static int ScorePinyin
    (
        SearchField   field,
        in SearchTerm term,
        bool          allowInitials
    )
    {
        if (field.PinyinWeight == 0 ||
            !term.HasASCIILetter    ||
            term.Text.Length < 2)
            return 0;

        if (term.IsASCIIAlphanumeric &&
            field.MaskReliable       &&
            (field.ConsumableMask & term.Mask) != term.Mask)
            return 0;

        if (term.Text.Length > field.MaxConsumable)
            return 0;

        if (term.IsASCIIAlphanumeric)
        {
            if ((field.StartMask & term.FirstBit) == 0)
                return 0;

            if (!IsSubsequence(field.PinyinStream, term.Text))
                return 0;
        }

        var text = term.Text.AsSpan();

        return text.Length <= MAX_ACTIVE_TEXT_LENGTH ?
                   ScorePinyinActive(field, in term, allowInitials, text) :
                   ScorePinyinDense(field, in term, allowInitials, text);
    }

    private static int ScorePinyinActive
    (
        SearchField        field,
        in SearchTerm      term,
        bool               allowInitials,
        ReadOnlySpan<char> text
    )
    {
        var last      = text.Length;
        var finalBit  = 1UL << last;
        var rowLength = last + 1;
        var allowBest = term is { Exact: false, Excluded: false };
        var buffer = rowLength <= MAX_STACK_ROW_LENGTH ?
                         stackalloc int[rowLength * 2] :
                         new int[rowLength        * 2];
        var previous = buffer[..rowLength];
        var current  = buffer[rowLength..];
        previous.Fill(-1);
        current.Fill(-1);

        var previousActive = 0UL;
        var currentActive  = 0UL;
        var best           = 0;

        var source = field.LiteralText;

        for (var index = 0; index < source.Length; index++)
        {
            if (source[index] == ' ')
            {
                if (index     > 0                                &&
                    index + 1 < source.Length                    &&
                    char.IsAsciiLetterOrDigit(source[index - 1]) &&
                    char.IsAsciiLetterOrDigit(source[index + 1]))
                {
                    var stale = previousActive;

                    while (stale != 0)
                    {
                        previous[BitOperations.TrailingZeroCount(stale)] =  -1;
                        stale                                            &= stale - 1;
                    }

                    previousActive = 0;
                }

                continue;
            }

            previous[0]    =  0;
            previousActive |= 1UL;

            var outdated = currentActive;

            while (outdated != 0)
            {
                current[BitOperations.TrailingZeroCount(outdated)] =  -1;
                outdated                                           &= outdated - 1;
            }

            currentActive = 0;

            var active = previousActive & ~finalBit;

            while (active != 0)
            {
                var consumed = BitOperations.TrailingZeroCount(active);
                active &= active - 1;

                var initials = previous[consumed];
                var target   = text[consumed];

                if (source[index] == target)
                {
                    var next = consumed + 1;

                    if (current[next] < 0 ||
                        initials      < current[next])
                    {
                        current[next] =  initials;
                        currentActive |= 1UL << next;
                    }
                }

                var remaining = text[consumed..];
                var readings  = field.Readings[index];

                if (readings.Length > 0)
                {
                    foreach (var reading in readings)
                    {
                        var sameInitial = reading[0] == target;

                        if (sameInitial && remaining.StartsWith(reading, StringComparison.Ordinal))
                        {
                            var next = consumed + reading.Length;

                            if (current[next] < 0 ||
                                initials      < current[next])
                            {
                                current[next] =  initials;
                                currentActive |= 1UL << next;
                            }
                        }
                        else if (allowBest                            &&
                                 initials         == 0                &&
                                 remaining.Length >= 2                &&
                                 reading.Length   >= remaining.Length &&
                                 reading.AsSpan().StartsWith(remaining, StringComparison.Ordinal))
                            best = Math.Max(best, field.PinyinWeight - 40);

                        if (allowInitials       &&
                            field.AllowInitials &&
                            sameInitial)
                        {
                            var next = consumed + 1;

                            if (current[next] < 0 ||
                                initials + 1  < current[next])
                            {
                                current[next] =  initials + 1;
                                currentActive |= 1UL << next;
                            }
                        }
                    }
                }
            }

            if ((currentActive & finalBit) != 0)
            {
                var initials = current[last];

                if (initials == 0)
                    return field.PinyinWeight + 30;

                best = Math.Max(best, field.PinyinWeight - 250 + Math.Min(60, last - initials));
            }

            var swapBuffer = previous;
            previous = current;
            current  = swapBuffer;

            (previousActive, currentActive) = (currentActive, previousActive);
        }

        return best;
    }

    private static int ScorePinyinDense
    (
        SearchField        field,
        in SearchTerm      term,
        bool               allowInitials,
        ReadOnlySpan<char> text
    )
    {
        var rowLength = text.Length + 1;
        var allowBest = term is { Exact: false, Excluded: false };
        var buffer = rowLength <= MAX_STACK_ROW_LENGTH ?
                         stackalloc int[rowLength * 2] :
                         new int[rowLength        * 2];
        var previous = buffer[..rowLength];
        var current  = buffer[rowLength..];
        previous.Fill(-1);
        var best = 0;

        var source = field.LiteralText;

        for (var index = 0; index < source.Length; index++)
        {
            if (source[index] == ' ')
            {
                if (index     > 0                                &&
                    index + 1 < source.Length                    &&
                    char.IsAsciiLetterOrDigit(source[index - 1]) &&
                    char.IsAsciiLetterOrDigit(source[index + 1]))
                    previous.Fill(-1);

                continue;
            }

            previous[0] = 0;
            current.Fill(-1);

            for (var consumed = 0; consumed < text.Length; consumed++)
            {
                var initials = previous[consumed];
                if (initials < 0)
                    continue;

                var target = text[consumed];

                if (source[index] == target)
                {
                    var next = consumed + 1;
                    if (current[next] < 0 ||
                        initials      < current[next])
                        current[next] = initials;
                }

                var remaining = text[consumed..];
                var readings  = field.Readings[index];

                if (readings.Length == 0)
                    continue;

                foreach (var reading in readings)
                {
                    var sameInitial = reading[0] == target;

                    if (sameInitial && remaining.StartsWith(reading, StringComparison.Ordinal))
                    {
                        var next = consumed + reading.Length;
                        if (current[next] < 0 ||
                            initials      < current[next])
                            current[next] = initials;
                    }
                    else if (allowBest                            &&
                             initials         == 0                &&
                             remaining.Length >= 2                &&
                             reading.Length   >= remaining.Length &&
                             reading.AsSpan().StartsWith(remaining, StringComparison.Ordinal))
                        best = Math.Max(best, field.PinyinWeight - 40);

                    if (allowInitials       &&
                        field.AllowInitials &&
                        sameInitial)
                    {
                        var next = consumed + 1;
                        if (current[next] < 0 ||
                            initials + 1  < current[next])
                            current[next] = initials + 1;
                    }
                }
            }

            if (current[text.Length] >= 0)
            {
                var initials = current[text.Length];

                if (initials == 0)
                    return field.PinyinWeight + 30;

                best = Math.Max(best, field.PinyinWeight - 250 + Math.Min(60, text.Length - initials));
            }

            var swap = previous;
            previous = current;
            current  = swap;
        }

        return best;
    }

    private static SearchTerm[] ParseTerms
    (
        string query
    )
    {
        var input = query.IsNormalized(NormalizationForm.FormKC) ?
                        query :
                        query.Normalize(NormalizationForm.FormKC);
        var terms = new List<SearchTerm>(4);
        var index = 0;

        while (index < input.Length)
        {
            if (char.IsWhiteSpace(input[index]))
            {
                index++;
                continue;
            }

            var excluded = input[index] == '-';

            if (excluded)
                index++;

            var            quoted       = false;
            var            exact        = false;
            var            segmentStart = index;
            StringBuilder? builder      = null;

            while (index < input.Length)
            {
                var character = input[index];

                if (character is '"' or '“' or '”' or '「' or '」')
                {
                    builder ??= new StringBuilder(input.Length);
                    builder.Append(input, segmentStart, index - segmentStart);

                    quoted = !quoted;
                    exact  = true;
                    index++;
                    segmentStart = index;
                    continue;
                }

                if (!quoted &&
                    char.IsWhiteSpace(character))
                    break;

                index++;
            }

            string raw;

            if (builder == null)
                raw = input[segmentStart..index];
            else
            {
                builder.Append(input, segmentStart, index - segmentStart);
                raw = builder.ToString();
            }

            var text = Normalize(raw);
            if (text.Length > 0)
                terms.Add(new(text, excluded, exact));
        }

        return Distinct(terms);
    }

    private static SearchTerm[] Distinct
    (
        List<SearchTerm> terms
    )
    {
        for (var index = 0; index < terms.Count; index++)
        {
            for (var other = terms.Count - 1; other > index; other--)
            {
                if (terms[index] != terms[other])
                    continue;

                terms.RemoveAt(other);
            }
        }

        return [.. terms];
    }

    private static string Normalize
    (
        string text,
        bool   preserveBoundaries = false
    )
    {
        if (!preserveBoundaries &&
            IsPlainASCII(text))
            return text;

        var normalized = text.IsNormalized(NormalizationForm.FormKD) ?
                             text :
                             text.Normalize(NormalizationForm.FormKD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (character      == '̈' &&
                builder.Length > 0    &&
                builder[^1]    == 'u')
                builder[^1] = 'v';
            else if (char.IsLetterOrDigit(character))
                builder.Append(char.ToLowerInvariant(character));
            else if (preserveBoundaries                        &&
                     builder.Length                     > 0    &&
                     builder[^1]                        != ' ' &&
                     char.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(' ');
        }

        return builder.ToString();
    }

    private static bool IsPlainASCII
    (
        string text
    )
    {
        foreach (var character in text)
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
                continue;

            return false;
        }

        return true;
    }

    private static bool IsChinese(char character) =>
        character is >= '\u3400' and <= '\u9fff' or >= '豈' and <= '﫿';

    private static bool IsSubsequence
    (
        string stream,
        string text
    )
    {
        if (stream.Length == 0)
            return false;

        var span     = stream.AsSpan();
        var position = 0;

        while (true)
        {
            var index = span.IndexOf(text[position]);
            if (index < 0)
                return false;

            position++;

            if (position == text.Length)
                return true;

            span = span[(index + 1)..];
        }
    }

    private static bool TryGetMaskBit
    (
        char      character,
        out ulong bit
    )
    {
        switch (character)
        {
            case >= 'a' and <= 'z':
                bit = 1UL << (character - 'a');
                return true;
            case >= '0' and <= '9':
                bit = 1UL << (26 + character - '0');
                return true;
            default:
                bit = 0;
                return false;
        }
    }

    private readonly record struct SearchTerm
    {
        public string Text                { get; }
        public bool   Excluded            { get; }
        public bool   Exact               { get; }
        public bool   HasASCIILetter      { get; }
        public bool   IsASCIIAlphanumeric { get; }
        public bool   IsChinese           { get; }
        public ulong  Mask                { get; }
        public ulong  FirstBit            { get; }

        public SearchTerm(string text, bool excluded, bool exact)
        {
            Text     = text;
            Excluded = excluded;
            Exact    = exact;

            var hasASCIILetter    = false;
            var asciiAlphanumeric = text.Length > 0;
            var mask              = 0UL;

            foreach (var character in text)
            {
                if (char.IsAsciiLetter(character))
                    hasASCIILetter = true;

                if (!char.IsAsciiLetterOrDigit(character))
                    asciiAlphanumeric = false;

                if (TryGetMaskBit(character, out var bit))
                    mask |= bit;
            }

            HasASCIILetter      = hasASCIILetter;
            IsASCIIAlphanumeric = asciiAlphanumeric;
            IsChinese           = text.IsChinese();
            Mask                = mask;
            FirstBit = TryGetMaskBit(text[0], out var firstBit) ?
                           firstBit :
                           0;
        }
    }

    private readonly struct Match
    (
        SearchEntry entry,
        int         weakest,
        int         total,
        int         phrase
    )
    {
        public SearchEntry Entry   { get; } = entry;
        public int         Weakest { get; } = weakest;
        public int         Total   { get; } = total;
        public int         Phrase  { get; } = phrase;
    }

    private sealed class MatchComparer
    (
        Comparison<T>? tieBreaker
    ) : IComparer<Match>
    {
        public int Compare(Match left, Match right)
        {
            var result = right.Weakest.CompareTo(left.Weakest);
            if (result != 0)
                return result;

            result = right.Phrase.CompareTo(left.Phrase);
            if (result != 0)
                return result;

            result = right.Total.CompareTo(left.Total);
            if (result != 0)
                return result;

            result = left.Entry.Fields[0].Text.Length.CompareTo(right.Entry.Fields[0].Text.Length);
            if (result != 0)
                return result;

            result = SortKey.Compare(left.Entry.TitleSortKey, right.Entry.TitleSortKey);

            return result != 0 ?
                       result :
                       tieBreaker?.Invoke(left.Entry.Item, right.Entry.Item) ?? left.Entry.Order.CompareTo(right.Entry.Order);
        }
    }

    private sealed record CacheEntry
    (
        string Query,
        T[]    Results
    );

    private sealed class SearchEntry
    {
        public T             Item           { get; }
        public SearchField[] Fields         { get; }
        public SortKey       TitleSortKey   { get; }
        public ulong         ConsumableMask { get; }
        public bool          MaskReliable   { get; }
        public int           Order          { get; set; }

        public SearchEntry
        (
            T                             item,
            IEnumerable<FuzzySearchField> source,
            Dictionary<char, string[]>    readings,
            CompareInfo                   compareInfo
        )
        {
            Item = item;
            var fields = new List<SearchField>();
            var title  = string.Empty;

            foreach (var field in source)
            {
                if (fields.Count == 0)
                    title = field.Text;

                fields.Add(new(field, readings));
            }

            TitleSortKey = compareInfo.GetSortKey(title, CompareOptions.IgnoreCase);
            Fields       = [.. fields.OrderByDescending(static field => field.MaxScore)];
            var mask         = 0UL;
            var maskReliable = true;

            foreach (var field in Fields)
            {
                mask         |= field.ConsumableMask;
                maskReliable &= field.MaskReliable;
            }

            ConsumableMask = mask;
            MaskReliable   = maskReliable;
        }
    }

    private sealed class SearchField
    {
        public string     Text                 { get; }
        public string     LiteralText          { get; }
        public string     Acronym              { get; }
        public string     PinyinStream         { get; }
        public string[][] Readings             { get; }
        public int        NativeWeight         { get; }
        public int        PinyinWeight         { get; }
        public int        MaxScore             { get; }
        public int        MaxConsumable        { get; }
        public ulong      ConsumableMask       { get; }
        public ulong      StartMask            { get; }
        public bool       MaskReliable         { get; }
        public bool       AllowInitials        { get; }
        public bool       AllowSingleCharacter { get; }
        public bool       AllowChineseSplit    { get; }
        public bool       AllowAcronym         { get; }
        public int        ShortMatchPenalty    { get; }

        public SearchField(FuzzySearchField field, Dictionary<char, string[]> readings)
        {
            var text         = field.Text;
            var nativeWeight = field.NativeWeight;
            var pinyinWeight = field.PinyinWeight;
            AllowSingleCharacter = field.AllowSingleCharacter;
            AllowChineseSplit    = field.AllowChineseSplit;
            AllowAcronym         = field.AllowAcronym;
            ShortMatchPenalty    = field.ShortMatchPenalty;
            Text                 = Normalize(text);
            LiteralText          = Normalize(text, true);
            NativeWeight         = nativeWeight;
            AllowInitials        = field.AllowInitials;
            Readings             = new string[LiteralText.Length][];
            var acronym = new StringBuilder();

            for (var index = 0; index < text.Length; index++)
            {
                var character = text[index];
                if (char.IsAsciiLetter(character) &&
                    (index == 0                             ||
                     !char.IsLetterOrDigit(text[index - 1]) ||
                     (char.IsUpper(character) &&
                      (char.IsLower(text[index - 1]) ||
                       (index + 1 < text.Length &&
                        char.IsLower(text[index + 1]))))))
                    acronym.Append(char.ToLowerInvariant(character));
            }

            Acronym = acronym.ToString();

            for (var index = 0; index < LiteralText.Length; index++)
            {
                var character = LiteralText[index];

                if (pinyinWeight == 0 ||
                    !IsChinese(character))
                {
                    Readings[index] = [];
                    continue;
                }

                if (!readings.TryGetValue(character, out var reading))
                {
                    reading = FuzzyPinyinReadings.POLYPHONIC_READINGS.TryGetValue(character, out var alternatives) ?
                                  alternatives :
                                  [Normalize(PinyinHelper.GetPinyin(character))];
                    readings.Add(character, reading);
                }

                Readings[index] = reading;
            }

            PinyinWeight = Readings.Any(static reading => reading.Length > 0) ?
                               pinyinWeight :
                               0;
            MaxScore = Math.Max(nativeWeight + 90, PinyinWeight + 30);

            var mask          = 0UL;
            var startMask     = 0UL;
            var maskReliable  = true;
            var maxConsumable = 0;
            var streamBuilder = new StringBuilder(LiteralText.Length * 2);

            foreach (var character in LiteralText)
            {
                if (!TryGetMaskBit(character, out var bit))
                    continue;

                mask      |= bit;
                startMask |= bit;
            }

            for (var index = 0; index < Readings.Length; index++)
            {
                var maximum = 1;

                foreach (var reading in Readings[index])
                {
                    foreach (var character in reading)
                    {
                        if (TryGetMaskBit(character, out var bit))
                            mask |= bit;
                        else
                            maskReliable = false;
                    }

                    if (reading.Length == 0)
                        continue;

                    if (TryGetMaskBit(reading[0], out var startBit))
                        startMask |= startBit;

                    streamBuilder.Append(reading);
                    maximum = Math.Max(maximum, reading.Length);
                }

                var source = LiteralText[index];
                if (source is > ' ' and < '\u007F')
                    streamBuilder.Append(source);

                if (source != ' ')
                    maxConsumable += maximum;
            }

            ConsumableMask = mask;
            StartMask      = startMask;
            MaskReliable   = maskReliable;
            MaxConsumable  = maxConsumable;
            PinyinStream = PinyinWeight == 0 ?
                               string.Empty :
                               streamBuilder.ToString();
        }
    }

    #region 常量

    private const int MAX_CHINESE_GAP        = 8;
    private const int MAX_STACK_ROW_LENGTH   = 256;
    private const int MAX_ACTIVE_TEXT_LENGTH = 62;
    private const int CACHE_CAPACITY         = 16;

    #endregion
}
