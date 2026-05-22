using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using OmenTools.Info.Collections;
using TinyPinyin;

namespace OmenTools.Utils.FuzzyMatcher;

/// <summary>
///     高性能、线程安全的中英文模糊搜索匹配器
/// </summary>
public sealed class FuzzyMatcher<T> : IDisposable
{
    private readonly Func<T, IEnumerable<(IEnumerable<string?> Texts, FuzzySearchWeight Weight)>>? defaultKeySelector;

    private readonly List<SearchEntry>       entries        = [];
    private readonly ReaderWriterLockSlim    lockSlim       = new();
    private readonly LRUCache<CacheKey, T[]> cache          = new(64);
    private readonly IPinyinProvider         pinyinProvider = new DefaultPinyinProvider();

    private bool disposed;

    public FuzzyMatcher
    (
        IEnumerable<T>                                                               items,
        Func<T, IEnumerable<(IEnumerable<string?> Texts, FuzzySearchWeight Weight)>> keySelector
    )
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(keySelector);

        defaultKeySelector = keySelector;

        var itemList = items.Where(static item => item != null).ToList();
        if (itemList.Count == 0) return;

        var builtEntries = new SearchEntry[itemList.Count];

        Parallel.For
        (
            0,
            itemList.Count,
            i =>
            {
                var item       = itemList[i];
                var keyPairs   = keySelector(item);
                var searchKeys = keyPairs.Select(pair => new SearchKey(pair.Texts, pair.Weight, pinyinProvider)).ToArray();
                builtEntries[i] = new SearchEntry(item, searchKeys);
            }
        );

        entries.AddRange(builtEntries);
    }


    /// <summary>
    ///     支持 CancellationToken 的搜索重载
    /// </summary>
    public T[] Search(string query, Comparison<T>? tieBreaker = null, int limit = 30, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        cancellationToken.ThrowIfCancellationRequested();

        var trimmed = query.Trim();
        var hasCjk  = trimmed.Any(IsCjk);

        // 长度限制逻辑：中文允许单字，纯英文/数字等 ASCII 限制最少 2 字符
        if (hasCjk)
        {
            if (Compact(trimmed).Length < 1)
                return [];
        }
        else
        {
            if (Compact(trimmed).Length < 2)
                return [];
        }

        var normalizedQuery = NormalizeText(trimmed);
        var key             = new CacheKey(normalizedQuery, limit, tieBreaker);

        // 1. 读取 LRU 缓存
        lockSlim.EnterReadLock();

        try
        {
            if (cache.TryGet(key, out var cached)) return [.. cached];
        }
        finally
        {
            lockSlim.ExitReadLock();
        }

        // 2. 解析查询词组
        var terms = ParseTerms(trimmed, pinyinProvider);
        if (terms.Count == 0)
            return [];

        // 3. 扫描匹配项并评分
        List<(T Item, int Score)> matches = [];

        lockSlim.EnterReadLock();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entries.Count > 1000)
            {
                var concurrentMatches = new ConcurrentBag<(T Item, int Score)>();
                Parallel.ForEach
                (
                    entries,
                    new ParallelOptions { CancellationToken = cancellationToken },
                    entry =>
                    {
                        if (TryScore(entry, terms, out var score)) concurrentMatches.Add((entry.Item, score));
                    }
                );
                matches.AddRange(concurrentMatches);
            }
            else
            {
                foreach (var entry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (TryScore(entry, terms, out var score)) matches.Add((entry.Item, score));
                }
            }
        }
        finally
        {
            lockSlim.ExitReadLock();
        }

        cancellationToken.ThrowIfCancellationRequested();

        // 4. 使用 Min-Heap 维护 Top-K 结果
        var pq = new PriorityQueue<T, MatchPriority>(matches.Count);

        foreach (var match in matches)
        {
            var priority = new MatchPriority(match.Score, match.Item, tieBreaker);

            if (pq.Count < limit) pq.Enqueue(match.Item, priority);
            else
            {
                pq.TryPeek(out _, out var topPriority);

                if (priority.CompareTo(topPriority) > 0)
                {
                    pq.Dequeue();
                    pq.Enqueue(match.Item, priority);
                }
            }
        }

        var result                                             = new T[pq.Count];
        for (var i = result.Length - 1; i >= 0; i--) result[i] = pq.Dequeue();

        // 5. 写入缓存
        lockSlim.EnterWriteLock();

        try
        {
            cache.AddOrUpdateAll([new(key, result)]);
        }
        finally
        {
            lockSlim.ExitWriteLock();
        }

        return [.. result];
    }

    /// <summary>
    ///     异步流式搜索匹配项
    /// </summary>
    public async IAsyncEnumerable<T> SearchAsync
    (
        string                                     query,
        Comparison<T>?                             tieBreaker        = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var results = Search(query, tieBreaker, int.MaxValue, cancellationToken);

        foreach (var item in results)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
            await Task.Yield();
        }
    }

    #region CRUD 动态管理 APIs

    public void Add(T item)
    {
        if (defaultKeySelector == null)
            throw new InvalidOperationException("No default key selector was provided in the constructor.");
        Add(item, defaultKeySelector);
    }

    public void Add(T item, Func<T, IEnumerable<(IEnumerable<string?> Texts, FuzzySearchWeight Weight)>> keySelector)
    {
        if (item == null) return;
        lockSlim.EnterWriteLock();

        try
        {
            var keyPairs   = keySelector(item);
            var searchKeys = keyPairs.Select(pair => new SearchKey(pair.Texts, pair.Weight, pinyinProvider)).ToArray();
            entries.Add(new SearchEntry(item, searchKeys));
            cache.Clear();
        }
        finally
        {
            lockSlim.ExitWriteLock();
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (defaultKeySelector == null)
            throw new InvalidOperationException("No default key selector was provided in the constructor.");
        AddRange(items, defaultKeySelector);
    }

    public void AddRange(IEnumerable<T> items, Func<T, IEnumerable<(IEnumerable<string?> Texts, FuzzySearchWeight Weight)>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(keySelector);

        var itemList = items.Where(static item => item != null).ToList();
        if (itemList.Count == 0) return;

        var builtEntries = new SearchEntry[itemList.Count];
        Parallel.For
        (
            0,
            itemList.Count,
            i =>
            {
                var item       = itemList[i];
                var keyPairs   = keySelector(item);
                var searchKeys = keyPairs.Select(pair => new SearchKey(pair.Texts, pair.Weight, pinyinProvider)).ToArray();
                builtEntries[i] = new SearchEntry(item, searchKeys);
            }
        );

        lockSlim.EnterWriteLock();

        try
        {
            entries.AddRange(builtEntries);
            cache.Clear();
        }
        finally
        {
            lockSlim.ExitWriteLock();
        }
    }

    public bool Remove(T item)
    {
        if (item == null) return false;
        lockSlim.EnterWriteLock();

        try
        {
            var index = entries.FindIndex(e => EqualityComparer<T>.Default.Equals(e.Item, item));

            if (index >= 0)
            {
                entries.RemoveAt(index);
                cache.Clear();
                return true;
            }

            return false;
        }
        finally
        {
            lockSlim.ExitWriteLock();
        }
    }

    public void Clear()
    {
        lockSlim.EnterWriteLock();

        try
        {
            entries.Clear();
            cache.Clear();
        }
        finally
        {
            lockSlim.ExitWriteLock();
        }
    }

    #endregion

    #region 核心评分逻辑

    private static bool TryScore(SearchEntry entry, IReadOnlyList<SearchTerm> terms, out int score)
    {
        score = 0;
        var bestKeyScore  = 0;
        var matchedAnyKey = false;

        foreach (var key in entry.Keys)
        {
            if (TryScoreKey(key, terms, out var keyScore))
            {
                matchedAnyKey = true;
                bestKeyScore  = Math.Max(bestKeyScore, keyScore);
            }
        }

        if (!matchedAnyKey)
            return false;

        score = bestKeyScore;
        return true;
    }

    private static bool TryScoreKey(SearchKey key, IReadOnlyList<SearchTerm> terms, out int keyScore)
    {
        keyScore = 0;

        // 1. 验证排除项 (Negative Terms) - 只要有任一 exclusion term 在当前 key 的 segments 中命中，则该 key 失败
        foreach (var term in terms)
        {
            if (term.IsNegative)
            {
                if (AnySegmentMatches(key.Segments, term, key.Weight))
                    return false;
            }
        }

        // 2. 评分正向项 (Positive Terms) - 遵循 AND 语义，即所有 positive term 都必须在当前 key 的某些 segments 中命中
        var positiveTermsCount = 0;
        var totalKeyScore      = 0;

        foreach (var term in terms)
        {
            if (term.IsNegative) continue;

            positiveTermsCount++;

            var bestTermScore = 0;

            foreach (var segment in key.Segments)
            {
                var termSegmentScore = ScoreSegment(segment, term, key.Weight);
                bestTermScore = Math.Max(bestTermScore, termSegmentScore);
            }

            if (bestTermScore == 0) return false;

            totalKeyScore += bestTermScore;
        }

        if (positiveTermsCount == 0)
        {
            // 如果仅有排除项且未触发排除，则给基础分
            keyScore = key.Weight.BaseScore;
            return true;
        }

        keyScore = totalKeyScore + (positiveTermsCount * 24);
        return true;
    }

    private static bool AnySegmentMatches(SegmentKey[] segments, SearchTerm term, FuzzySearchWeight weight)
    {
        foreach (var t in segments)
        {
            if (ScoreSegment(t, term, weight) > 0)
                return true;
        }

        return false;
    }

    private static int ScoreSegment(SegmentKey segment, SearchTerm term, FuzzySearchWeight weight)
    {
        // 0. 粗筛：如果字符掩码不包含查询掩码，必然无法通过子序列或等值匹配
        if (!term.Mask.IsSubsetOf(segment.Mask))
            return 0;

        // 双引号精确匹配路径
        if (term.IsExact)
        {
            if (segment.Original.Equals(term.Original, StringComparison.Ordinal))
                return (weight.BaseScore * 4) + (term.Original.Length * 10);

            if (segment.Original.Contains(term.Original, StringComparison.OrdinalIgnoreCase))
                return (weight.BaseScore * 2) + (term.Original.Length * 5);

            if (segment.Compact.Contains(term.Compact, StringComparison.Ordinal))
                return (weight.BaseScore * 2) + (term.Compact.Length * 5);

            return 0;
        }

        var bestScore = 0;

        // 通道 1：完全等值匹配 (Exact Match)
        if (segment.Original.Equals(term.Original, StringComparison.Ordinal))
        {
            var exactScore = (weight.BaseScore * 4) + (term.Original.Length * 10);
            bestScore = Math.Max(bestScore, exactScore);
        }

        // 通道 2：前缀匹配 (Prefix Match)
        if (segment.Original.StartsWith(term.Original, StringComparison.Ordinal))
        {
            var prefixScore = (weight.BaseScore * 2) + (term.Original.Length * 8);
            bestScore = Math.Max(bestScore, prefixScore);
        }

        // 通道 3：常规文本子序列模糊匹配 (Fuzzy Match on Original Text)
        var textFuzzy = FuzzyMatch(segment.Original, term.Original, weight);
        bestScore = Math.Max(bestScore, textFuzzy);

        // 通道 4：压缩后文本子序列模糊匹配 (Fuzzy Match on Compact Text)
        var compactFuzzy = FuzzyMatch(segment.Compact, term.Compact, weight);
        bestScore = Math.Max(bestScore, compactFuzzy);

        // 通道 5：英文首字母/Case边界缩写匹配 (Initialisms Match)
        foreach (var init in segment.Initialisms)
        {
            var initFuzzy = FuzzyMatch(init, term.Compact, weight with { BaseScore = weight.InitialismBase });
            bestScore = Math.Max(bestScore, initFuzzy);
        }

        // 通道 6：中文拼音子序列匹配 (Symmetrical Pinyin Match)
        foreach (var py in segment.PinyinCombinations)
        {
            var pyFuzzy = FuzzyMatch(py, term.Compact, weight);
            bestScore = Math.Max(bestScore, (int)(pyFuzzy * 0.9)); // 拼音权重微调，优先匹配汉字原文
        }

        // 通道 7：无序拆分组合匹配 (Unordered Match)
        if (bestScore == 0 && term.Compact.Length >= 6)
        {
            var unordered = TryUnorderedMatch(segment.Compact, term.Compact, weight);
            bestScore = Math.Max(bestScore, unordered);
        }

        // 通道 8：编辑距离容错匹配 (Approximate Match) - 最弱兜底
        if (bestScore == 0 && term.Compact.Length >= 3)
        {
            var approx = ApproximateMatch(segment.Compact, term.Compact, weight);
            bestScore = Math.Max(bestScore, approx);
        }

        return bestScore;
    }

    #endregion

    #region 子序列双向 greedy 匹配核心 (Zero Alloc)

    private static int FuzzyMatch(string source, string pattern, FuzzySearchWeight weight)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(pattern)) return 0;
        if (pattern.Length > source.Length) return 0;

        // 1. 左向贪心匹配
        int[]? rentedLeft = null;
        var posLeft = pattern.Length <= 256
                          ? stackalloc int[pattern.Length]
                          : rentedLeft = ArrayPool<int>.Shared.Rent(pattern.Length);

        var leftMatch = false;
        var siIndex   = 0;
        leftMatch = true;

        for (var pi = 0; pi < pattern.Length; pi++)
        {
            while (siIndex < source.Length && source[siIndex] != pattern[pi])
                siIndex++;

            if (siIndex >= source.Length)
            {
                leftMatch = false;
                break;
            }

            posLeft[pi] = siIndex;
            siIndex++;
        }

        if (!leftMatch)
        {
            if (rentedLeft != null) ArrayPool<int>.Shared.Return(rentedLeft);
            return 0;
        }

        // 2. 右向贪心匹配 (当左侧存在子序列时，右侧也必然存在，无需额外校验)
        int[]? rentedRight = null;
        var posRight = pattern.Length <= 256
                           ? stackalloc int[pattern.Length]
                           : rentedRight = ArrayPool<int>.Shared.Rent(pattern.Length);

        var si = source.Length - 1;

        for (var pi = pattern.Length - 1; pi >= 0; pi--)
        {
            while (si >= 0 && source[si] != pattern[pi])
                si--;
            posRight[pi] = si;
            si--;
        }

        // 评分比对，选取两种排列的较高分数 (解决旧匹配只用右侧覆写的问题)
        var scoreLeft  = CalculateAlignmentScore(source, posLeft,  pattern.Length, weight);
        var scoreRight = CalculateAlignmentScore(source, posRight, pattern.Length, weight);

        if (rentedLeft  != null) ArrayPool<int>.Shared.Return(rentedLeft);
        if (rentedRight != null) ArrayPool<int>.Shared.Return(rentedRight);

        return Math.Max(Math.Max(scoreLeft, scoreRight), 0);
    }

    private static int CalculateAlignmentScore(string source, Span<int> positions, int patternLength, FuzzySearchWeight weight)
    {
        var score = weight.BaseScore;

        for (var pi = 0; pi < patternLength; pi++)
        {
            score += weight.ConsecutiveBonus;
            score += PositionBonus(source, positions[pi], weight.BoundaryBonus);

            if (pi > 0)
            {
                if (positions[pi] == positions[pi - 1] + 1)
                    score += weight.ConsecutiveBonus * 2;

                var gap = positions[pi] - positions[pi - 1] - 1;
                score -= gap * weight.GapPenalty;
            }
        }

        score += Math.Max(0, weight.BoundaryBonus - positions[0]);
        return score;
    }

    private static int PositionBonus(string source, int pos, int boundaryBonus)
    {
        if (pos == 0) return boundaryBonus;

        var prev = source[pos - 1];

        // 动态以 boundaryBonus 比例衡量加分，解除硬编码量纲硬伤
        if (prev is ' ' or '_' or '-' or '.' or '\n' or '/')
            return (int)(boundaryBonus * 0.8);
        if (char.IsLower(prev) && char.IsUpper(source[pos]))
            return (int)(boundaryBonus * 0.6);
        if (char.IsDigit(prev) ^ char.IsDigit(source[pos]))
            return (int)(boundaryBonus * 0.6);

        return 0;
    }

    #endregion

    #region 编辑距离兜底算法 (Zero Alloc)

    private static int ApproximateMatch(string source, string pattern, FuzzySearchWeight weight)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(pattern)) return 0;
        if (pattern.Length < 3) return 0;

        var maxDistance  = pattern.Length <= 5 ? 1 : 2;
        var bestDistance = int.MaxValue;

        var lenDiff   = source.Length - pattern.Length;
        var lastStart = -1;

        for (var start = 0; start <= lenDiff + maxDistance; start++)
        {
            var actualStart = Math.Max(0, start - maxDistance);
            if (actualStart == lastStart) continue;
            lastStart = actualStart;

            var windowEnd = Math.Min(source.Length, actualStart + pattern.Length + (maxDistance * 2));
            if (actualStart >= windowEnd) continue;

            var window = source.AsSpan(actualStart, windowEnd - actualStart);

            var distance = DamerauLevenshteinDistance(window, pattern, maxDistance);
            if (distance < 0) continue;

            bestDistance = Math.Min(bestDistance, distance);
        }

        if (bestDistance > maxDistance) return 0;

        return Math.Max(weight.ApproximateBase + (pattern.Length * 4) - (bestDistance * 40), 0);
    }

    private static int DamerauLevenshteinDistance(ReadOnlySpan<char> source, string pattern, int maxDistance)
    {
        var slen = source.Length;
        var plen = pattern.Length;

        var    rowSize   = plen + 1;
        var    totalSize = rowSize * 3;
        int[]? rented    = null;
        var buffer = totalSize <= 256
                         ? stackalloc int[totalSize]
                         : rented = ArrayPool<int>.Shared.Rent(totalSize);

        try
        {
            var v0 = buffer[..rowSize];
            var v1 = buffer[rowSize..(rowSize * 2)];
            var v2 = buffer[(rowSize          * 2)..totalSize];

            for (var j = 0; j <= plen; j++)
                v0[j] = j;

            for (var i = 1; i <= slen; i++)
            {
                v2[0] = i;
                var rowMin = v2[0];

                for (var j = 1; j <= plen; j++)
                {
                    var cost = source[i - 1] == pattern[j - 1] ? 0 : 1;

                    var deletion     = v1[j]     + 1;
                    var insertion    = v2[j - 1] + 1;
                    var substitution = v1[j - 1] + cost;

                    var value = Math.Min(Math.Min(deletion, insertion), substitution);

                    if (i             > 1               &&
                        j             > 1               &&
                        source[i - 1] == pattern[j - 2] &&
                        source[i - 2] == pattern[j - 1]) value = Math.Min(value, v0[j - 2] + 1);

                    v2[j]  = value;
                    rowMin = Math.Min(rowMin, value);
                }

                if (rowMin > maxDistance) return -1;

                var temp = v0;
                v0 = v1;
                v1 = v2;
                v2 = temp;
            }

            var best = int.MaxValue;
            for (var j = 0; j <= plen; j++)
                best = Math.Min(best, v1[j]);

            return best <= maxDistance ? best : -1;
        }
        finally
        {
            if (rented != null)
                ArrayPool<int>.Shared.Return(rented);
        }
    }

    #endregion

    #region 无序拆分匹配

    private static int TryUnorderedMatch(string source, string query, FuzzySearchWeight weight)
    {
        var bestScore  = 0;
        var sourceSpan = source.AsSpan();

        for (var split = 3; split <= query.Length - 3; split++)
        {
            var part1 = query.AsSpan(0, split);
            var part2 = query.AsSpan(split);

            if (QuickSubsequence(sourceSpan,         part1, out var end1) &&
                QuickSubsequence(sourceSpan[end1..], part2, out _))
            {
                var s1 = FuzzyMatch(source, part1.ToString(), weight);
                var s2 = FuzzyMatch(source, part2.ToString(), weight);
                if (s1 > 0 && s2 > 0)
                    bestScore = Math.Max(bestScore, s1 + s2);
            }

            if (QuickSubsequence(sourceSpan,         part2, out var end2) &&
                QuickSubsequence(sourceSpan[end2..], part1, out _))
            {
                var s1 = FuzzyMatch(source, part2.ToString(), weight);
                var s2 = FuzzyMatch(source, part1.ToString(), weight);
                if (s1 > 0 && s2 > 0)
                    bestScore = Math.Max(bestScore, s1 + s2);
            }
        }

        return bestScore;
    }

    private static bool QuickSubsequence(ReadOnlySpan<char> source, ReadOnlySpan<char> pattern, out int endPos)
    {
        endPos = 0;
        var si = 0;

        foreach (var t in pattern)
        {
            while (si < source.Length && source[si] != t)
                si++;
            if (si >= source.Length) return false;
            si++;
        }

        endPos = si;
        return true;
    }

    #endregion

    #region 字符串处理与智能分词

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCjk(char ch) => ch >= 0x4e00 && ch <= 0x9fff;

    private static string NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var builder      = new StringBuilder(text.Length);
        var pendingSpace = false;

        foreach (var ch in text.Trim())
        {
            if (char.IsWhiteSpace(ch))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(char.ToLowerInvariant(ch));
        }

        return builder.ToString();
    }

    private static string Compact(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var builder = new StringBuilder(text.Length);

        foreach (var ch in text)
        {
            if (!char.IsLetterOrDigit(ch)) continue;
            builder.Append(char.ToLowerInvariant(ch));
        }

        return builder.ToString();
    }

    /// <summary>
    ///     支持 CJK 边界、数字边界、CamelCase、以及连续大写缩写词的智能分词器
    /// </summary>
    private static List<string> SplitByCase(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];
        var list  = new List<string>();
        var len   = text.Length;
        var start = 0;

        for (var i = 0; i < len; i++)
        {
            if (i == len - 1)
            {
                list.Add(text[start..len]);
                break;
            }

            var curr = text[i];
            var next = text[i + 1];

            var split = false;

            // 1. 中文与非中文边界
            if (IsCjk(curr) != IsCjk(next)) split = true;
            // 2. 数字与字母边界
            else if (char.IsDigit(curr) != char.IsDigit(next)) split = true;
            // 3. 小写与大写边界 (如 camelCase)
            else if (char.IsLower(curr) && char.IsUpper(next)) split = true;
            // 4. 连续大写缩写词在小写前的边界 (如 HTMLParser)
            else if (char.IsUpper(curr) && char.IsUpper(next) && i + 2 < len && char.IsLower(text[i + 2])) split = true;

            if (split)
            {
                list.Add(text[start..(i + 1)]);
                start = i + 1;
            }
        }

        return list.Select(static s => s.Trim()).Where(static s => s.Length > 0).ToList();
    }

    private static void AppendAsciiInitialism(StringBuilder builder, string text)
    {
        var tokens = SplitByCase(text);

        foreach (var token in tokens)
        {
            if (token.Length == 0) continue;

            var allUpper = true;

            foreach (var t in token)
            {
                if (char.IsLetter(t) && !char.IsUpper(t))
                {
                    allUpper = false;
                    break;
                }
            }

            // IBM 等纯大写缩写作为 acronym，保留其所有字母至首字母字段
            if (allUpper && token.Length > 1)
            {
                foreach (var ch in token)
                {
                    if (char.IsLetterOrDigit(ch))
                        builder.Append(char.ToLowerInvariant(ch));
                }
            }
            else
            {
                var first = token[0];
                if (char.IsLetterOrDigit(first))
                    builder.Append(char.ToLowerInvariant(first));
            }
        }
    }

    #endregion

    #region 查询词析构

    private static List<SearchTerm> ParseTerms(string rawQuery, IPinyinProvider pinyinProvider)
    {
        if (string.IsNullOrWhiteSpace(rawQuery)) return [];

        var terms     = new List<SearchTerm>();
        var remaining = rawQuery.Trim();

        // 1. 析构双引号精确搜索短语
        while (true)
        {
            var startIdx = remaining.IndexOf('"');
            if (startIdx == -1) break;

            var endIdx = remaining.IndexOf('"', startIdx + 1);
            if (endIdx == -1) break;

            var exactText = remaining.Substring(startIdx + 1, endIdx - startIdx - 1).Trim();
            if (exactText.Length > 0) terms.Add(CreateSearchTerm(exactText, true, false, pinyinProvider));

            remaining = (remaining[..startIdx] + " " + remaining[(endIdx + 1)..]).Trim();
        }

        // 2. 析构普通空格分词，支持以减号 '-' 标志的负向排除项
        var parts = remaining.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var isNegative = false;
            var cleanPart  = part;

            if (cleanPart.StartsWith('-') && cleanPart.Length > 1)
            {
                isNegative = true;
                cleanPart  = cleanPart[1..];
            }

            var subTokens = SplitByCase(cleanPart);
            foreach (var token in subTokens) terms.Add(CreateSearchTerm(token, false, isNegative, pinyinProvider));
        }

        return terms;
    }

    private static SearchTerm CreateSearchTerm(string text, bool isExact, bool isNegative, IPinyinProvider pinyinProvider)
    {
        var normalized = NormalizeText(text);
        var compact    = Compact(text);
        var isCjk      = text.Any(IsCjk);

        string[] pinyinCombinations   = [];
        if (isCjk) pinyinCombinations = pinyinProvider.GetPinyinCombinations(normalized);

        return new SearchTerm
        (
            normalized,
            compact,
            pinyinCombinations,
            isCjk,
            isExact,
            isNegative,
            new CharMask(compact + (isCjk ? "\u4e00" : ""))
        );
    }

    #endregion

    #region 内部数据模型

    private readonly record struct SearchEntry
    (
        T           Item,
        SearchKey[] Keys
    );

    private sealed class SearchKey
    {
        public SegmentKey[]      Segments { get; }
        public FuzzySearchWeight Weight   { get; }

        public SearchKey(IEnumerable<string?> primarySegments, FuzzySearchWeight weight, IPinyinProvider pinyinProvider)
        {
            Weight = weight;
            var segmentsList = new List<SegmentKey>();
            var uniqueTexts  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var text in primarySegments)
            {
                var normalized = NormalizeText(text);
                if (string.IsNullOrWhiteSpace(normalized) || !uniqueTexts.Add(normalized))
                    continue;

                segmentsList.Add(new SegmentKey(normalized, pinyinProvider));
            }

            Segments = [.. segmentsList];
        }
    }

    private sealed class SegmentKey
    {
        public string   Original           { get; }
        public string   Compact            { get; }
        public string[] PinyinCombinations { get; }
        public string[] PinyinInitialisms  { get; }
        public string[] Initialisms        { get; }
        public CharMask Mask               { get; }

        public SegmentKey(string original, IPinyinProvider pinyinProvider)
        {
            Original = original;
            Compact  = Compact(original);

            var isCjk = original.Any(IsCjk);
            PinyinCombinations = isCjk ? pinyinProvider.GetPinyinCombinations(original) : [];
            PinyinInitialisms  = isCjk ? ((DefaultPinyinProvider)pinyinProvider).GetPinyinInitialismCombinations(original) : [];

            var initialismBuilder = new StringBuilder();
            AppendAsciiInitialism(initialismBuilder, original);
            var asciiInit = initialismBuilder.ToString().ToLowerInvariant();

            var initialsList = new List<string>();
            if (!string.IsNullOrEmpty(asciiInit)) initialsList.Add(asciiInit);
            initialsList.AddRange(PinyinInitialisms);
            Initialisms = [.. initialsList.Distinct()];

            var maskBuilder = new StringBuilder();
            maskBuilder.Append(Original);
            maskBuilder.Append(Compact);
            foreach (var p in PinyinCombinations) maskBuilder.Append(p);
            foreach (var init in Initialisms) maskBuilder.Append(init);
            Mask = new CharMask(maskBuilder.ToString());
        }
    }

    private readonly record struct SearchTerm
    (
        string   Original,
        string   Compact,
        string[] PinyinCombinations,
        bool     IsCjk,
        bool     IsExact,
        bool     IsNegative,
        CharMask Mask
    );

    private readonly struct CharMask
    {
        private readonly ulong mask;

        public CharMask(string text)
        {
            ulong m = 0;

            foreach (var c in text)
            {
                var low = char.ToLowerInvariant(c);

                switch (low)
                {
                    case >= 'a' and <= 'z':
                        m |= 1UL << (low - 'a');
                        break;
                    case >= '0' and <= '9':
                        m |= 1UL << (26 + (low - '0'));
                        break;
                    default:
                    {
                        if (IsCjk(low))
                            m |= 1UL << 63;
                        break;
                    }
                }
            }

            mask = m;
        }

        public bool IsSubsetOf(CharMask other) => (mask & other.mask) == mask;
    }

    #endregion

    #region 拼音扩展抽象接口与实现

    private interface IPinyinProvider
    {
        string GetPinyin(string text, string separator);

        string[] GetPinyinCombinations(string segment);
    }

    private sealed class DefaultPinyinProvider : IPinyinProvider
    {
        public string GetPinyin(string text, string separator) =>
            PinyinHelper.GetPinyin(text, separator);

        public string[] GetPinyinCombinations(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment)) return [];

            var results = new List<StringBuilder> { new() };

            foreach (var ch in segment)
            {
                List<string> options = [];

                var p = PinyinHelper.GetPinyin(ch.ToString(), string.Empty).ToLowerInvariant();
                options.Add(!string.IsNullOrWhiteSpace(p) ? p : ch.ToString().ToLowerInvariant());

                var nextResults = new List<StringBuilder>();

                foreach (var res in results)
                {
                    foreach (var opt in options)
                    {
                        var sb = new StringBuilder(res.ToString());
                        sb.Append(opt);
                        nextResults.Add(sb);
                    }
                }

                results = nextResults;

                if (results.Count > 8) results = results.Take(8).ToList();
            }

            return results.Select(static sb => sb.ToString()).Distinct().ToArray();
        }

        public string[] GetPinyinInitialismCombinations(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment)) return [];

            var results = new List<StringBuilder> { new() };

            foreach (var ch in segment)
            {
                List<char> options = [];

                var p = PinyinHelper.GetPinyin(ch.ToString(), string.Empty).ToLowerInvariant();
                options.Add(!string.IsNullOrWhiteSpace(p) ? p[0] : char.ToLowerInvariant(ch));

                var nextResults = new List<StringBuilder>();

                foreach (var res in results)
                {
                    foreach (var opt in options.Distinct())
                    {
                        var sb = new StringBuilder(res.ToString());
                        sb.Append(opt);
                        nextResults.Add(sb);
                    }
                }

                results = nextResults;

                if (results.Count > 8) results = results.Take(8).ToList();
            }

            return results.Select(static sb => sb.ToString()).Distinct().ToArray();
        }
    }

    #endregion

    #region 线程安全 LRU 缓存

    private readonly struct CacheKey
    (
        string         query,
        int            limit,
        Comparison<T>? tieBreaker
    ) : IEquatable<CacheKey>
    {
        public string         Query      { get; } = query;
        public int            Limit      { get; } = limit;
        public Comparison<T>? TieBreaker { get; } = tieBreaker;

        public bool Equals(CacheKey other) =>
            string.Equals(Query, other.Query, StringComparison.Ordinal) &&
            Limit == other.Limit                                        &&
            Equals(TieBreaker, other.TieBreaker);

        public override bool Equals(object? obj) => obj is CacheKey other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Query, Limit, TieBreaker);
    }

    #endregion

    #region Heap Priority Key Helper

    private readonly struct MatchPriority
    (
        int            score,
        T              item,
        Comparison<T>? tieBreaker
    ) : IComparable<MatchPriority>
    {
        public int Score { get; } = score;
        public T   Item  { get; } = item;

        public int CompareTo(MatchPriority other)
        {
            var comp = Score.CompareTo(other.Score);
            if (comp != 0) return comp;

            if (tieBreaker != null)
                return -tieBreaker(Item, other.Item);

            return 0;
        }
    }

    #endregion

    public void Dispose()
    {
        if (disposed) return;
        cache.Dispose();
        lockSlim.Dispose();
        disposed = true;
    }
}
