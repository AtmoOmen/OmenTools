using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Lumina.Excel;
using TinyPinyin;

namespace OmenTools.Helpers;

public class LuminaSearcher<T> where T : struct, IExcelRow<T>
{
    private readonly Throttler<Guid> SearchThrottler = new();

    public LuminaSearcher
    (
        IEnumerable<T>                             data,
        Func<T, string>[]                          searchFuncs,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderFunc        = null,
        int                                        resultLimit      = 100,
        uint                                       throttleInterval = 100,
        int                                        retryInterval    = 50,
        int                                        maxRetries       = 3
    )
    {
        Guid = Guid.NewGuid();

        if (orderFunc != null)
        {
            var query = data.AsQueryable();
            Data = orderFunc(query)
                   .ThenBy(x => x.RowId)
                   .ToList();
        }
        else
        {
            Data = data.OrderBy(x => x.RowId)
                       .ToList();
        }

        SearchResult          = Data.Take(resultLimit).ToList();
        this.resultLimit      = resultLimit;
        this.throttleInterval = throttleInterval;
        this.retryInterval    = retryInterval;
        this.maxRetries       = maxRetries;

        var dataCount = Data.Count;
        preprocessedData = new string[dataCount][];

        Parallel.For
        (
            0,
            dataCount,
            i =>
            {
                var item          = Data[i];
                var searchStrings = new string[searchFuncs.Length];

                for (var j = 0; j < searchFuncs.Length; j++)
                {
                    var val = searchFuncs[j](item);
                    searchStrings[j] = val + "_" + PinyinHelper.GetPinyin(val, string.Empty);
                }

                preprocessedData[i] = searchStrings;
            }
        );
    }

    public Guid             Guid         { get; init; }
    public IReadOnlyList<T> Data         { get; init; }
    public List<T>          SearchResult { get; private set; }

    private readonly int                                     resultLimit;
    private readonly uint                                    throttleInterval;
    private readonly int                                     retryInterval;
    private readonly int                                     maxRetries;
    private readonly string[][]                              preprocessedData;
    private readonly ConcurrentDictionary<string, List<int>> cache = [];

    public void Search(string keyword, bool isIgnoreCase = true, bool isRegex = true)
    {
        var cacheKey = keyword + "_" + isIgnoreCase + "_" + isRegex;

        if (cache.TryGetValue(cacheKey, out var cachedIndexes))
        {
            SearchResult = cachedIndexes.Take(resultLimit).Select(index => Data[index]).ToList();
            return;
        }

        if (!SearchThrottler.Throttle(Guid, throttleInterval))
        {
            RetrySearch(keyword, isIgnoreCase, isRegex, 0);
            return;
        }

        ExecuteSearch(keyword, isIgnoreCase, isRegex);
    }

    private void RetrySearch(string keyword, bool isIgnoreCase, bool isRegex, int attempt)
    {
        if (attempt >= maxRetries) return;

        Task.Delay(retryInterval).ContinueWith
        (_ =>
            {
                if (!SearchThrottler.Throttle(Guid, throttleInterval))
                    RetrySearch(keyword, isIgnoreCase, isRegex, attempt + 1);
                else
                    ExecuteSearch(keyword, isIgnoreCase, isRegex);
            }
        );
    }

    private void ExecuteSearch(string keyword, bool isIgnoreCase, bool isRegex)
    {
        var comparison   = isIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var finalResults = new List<int>();
        var syncLock     = new Lock();

        var    regexOptions = isIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        Regex? regex        = null;

        if (isRegex)
        {
            try
            {
                regex = new Regex(keyword, regexOptions);
            }
            catch
            {
                isRegex = false;
            }
        }

        Parallel.For
        (
            0,
            preprocessedData.Length,
            () => new List<int>(),
            (i, _, localList) =>
            {
                var itemStrings = preprocessedData[i];
                var isMatch     = false;

                if (isRegex && regex != null)
                {
                    foreach (var str in itemStrings)
                    {
                        if (regex.IsMatch(str))
                        {
                            isMatch = true;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var str in itemStrings)
                    {
                        if (str.Contains(keyword, comparison))
                        {
                            isMatch = true;
                            break;
                        }
                    }
                }

                if (isMatch)
                    localList.Add(i);

                return localList;
            },
            localList =>
            {
                if (localList.Count > 0)
                {
                    lock (syncLock)
                        finalResults.AddRange(localList);
                }
            }
        );

        finalResults.Sort();
        var resultIndexes = finalResults.Take(resultLimit).ToList();

        SearchResult                                        = resultIndexes.Select(index => Data[index]).ToList();
        cache[keyword + "_" + isIgnoreCase + "_" + isRegex] = resultIndexes;
    }
}

public class LuminaSearcherSubRow<T> where T : struct, IExcelSubrow<T>
{
    private readonly Throttler<Guid> SearchThrottler = new();

    public LuminaSearcherSubRow
    (
        IEnumerable<T>                             data,
        Func<T, string>[]                          searchFuncs,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderFunc        = null,
        int                                        resultLimit      = 100,
        uint                                       throttleInterval = 100,
        int                                        retryInterval    = 50,
        int                                        maxRetries       = 3
    )
    {
        Guid = Guid.NewGuid();

        if (orderFunc != null)
        {
            var query = data.AsQueryable();
            Data = orderFunc(query)
                   .ThenBy(x => x.RowId)
                   .ToList();
        }
        else
        {
            Data = data.OrderBy(x => x.RowId)
                       .ToList();
        }

        SearchResult          = Data.Take(resultLimit).ToList();
        this.resultLimit      = resultLimit;
        this.throttleInterval = throttleInterval;
        this.retryInterval    = retryInterval;
        this.maxRetries       = maxRetries;

        var dataCount = Data.Count;
        preprocessedData = new string[dataCount][];

        Parallel.For
        (
            0,
            dataCount,
            i =>
            {
                var item          = Data[i];
                var searchStrings = new string[searchFuncs.Length];

                for (var j = 0; j < searchFuncs.Length; j++)
                {
                    var val = searchFuncs[j](item);
                    searchStrings[j] = val + "_" + PinyinHelper.GetPinyin(val, string.Empty);
                }

                preprocessedData[i] = searchStrings;
            }
        );
    }

    public Guid             Guid         { get; init; }
    public IReadOnlyList<T> Data         { get; init; }
    public List<T>          SearchResult { get; private set; }

    private readonly int                                     resultLimit;
    private readonly uint                                    throttleInterval;
    private readonly int                                     retryInterval;
    private readonly int                                     maxRetries;
    private readonly string[][]                              preprocessedData;
    private readonly ConcurrentDictionary<string, List<int>> cache = [];

    public void Search(string keyword, bool isIgnoreCase = true, bool isRegex = true)
    {
        var cacheKey = keyword + "_" + isIgnoreCase + "_" + isRegex;

        if (cache.TryGetValue(cacheKey, out var cachedIndexes))
        {
            SearchResult = cachedIndexes.Take(resultLimit).Select(index => Data[index]).ToList();
            return;
        }

        if (!SearchThrottler.Throttle(Guid, throttleInterval))
        {
            RetrySearch(keyword, isIgnoreCase, isRegex, 0);
            return;
        }

        ExecuteSearch(keyword, isIgnoreCase, isRegex);
    }

    private void RetrySearch(string keyword, bool isIgnoreCase, bool isRegex, int attempt)
    {
        if (attempt >= maxRetries) return;

        Task.Delay(retryInterval).ContinueWith
        (_ =>
            {
                if (!SearchThrottler.Throttle(Guid, throttleInterval))
                    RetrySearch(keyword, isIgnoreCase, isRegex, attempt + 1);
                else
                    ExecuteSearch(keyword, isIgnoreCase, isRegex);
            }
        );
    }

    private void ExecuteSearch(string keyword, bool isIgnoreCase, bool isRegex)
    {
        var comparison   = isIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var finalResults = new List<int>();
        var syncLock     = new Lock();

        var    regexOptions = isIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        Regex? regex        = null;

        if (isRegex)
        {
            try
            {
                regex = new Regex(keyword, regexOptions);
            }
            catch
            {
                isRegex = false;
            }
        }

        Parallel.For
        (
            0,
            preprocessedData.Length,
            () => new List<int>(),
            (i, _, localList) =>
            {
                var itemStrings = preprocessedData[i];
                var isMatch     = false;

                if (isRegex && regex != null)
                {
                    foreach (var str in itemStrings)
                    {
                        if (regex.IsMatch(str))
                        {
                            isMatch = true;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var str in itemStrings)
                    {
                        if (str.Contains(keyword, comparison))
                        {
                            isMatch = true;
                            break;
                        }
                    }
                }

                if (isMatch)
                    localList.Add(i);

                return localList;
            },
            localList =>
            {
                if (localList.Count > 0)
                {
                    lock (syncLock)
                        finalResults.AddRange(localList);
                }
            }
        );

        finalResults.Sort();
        var resultIndexes = finalResults.Take(resultLimit).ToList();

        SearchResult                                        = resultIndexes.Select(index => Data[index]).ToList();
        cache[keyword + "_" + isIgnoreCase + "_" + isRegex] = resultIndexes;
    }
}
