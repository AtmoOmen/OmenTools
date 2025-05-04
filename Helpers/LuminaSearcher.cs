using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Lumina.Excel;

namespace OmenTools.Helpers;

public class LuminaSearcher<T> where T : struct, IExcelRow<T>
{
    private readonly Throttler<Guid> SearchThrottler = new();

    public LuminaSearcher(
        IEnumerable<T> data,
        Func<T, string>[] searchFuncs,
        Func<T, string> orderFunc,
        int resultLimit = 100,
        int throttleInterval = 100,
        int retryInterval = 50,
        int maxRetries = 3)
    {
        Guid = Guid.NewGuid();
        Data = data
            .OrderBy(item => orderFunc(item).Length)
            .ThenBy(x => x.RowId)
            .ToList();

        SearchResult = Data.Take(resultLimit).ToList();
        this.resultLimit = resultLimit;
        this.throttleInterval = throttleInterval;
        this.retryInterval = retryInterval;
        this.maxRetries = maxRetries;
        preprocessedData = Data
            .Select(item => searchFuncs.Select(func => func(item)).ToList())
            .ToList();
    }

    public Guid Guid { get; init; }
    public IReadOnlyList<T> Data { get; init; }
    public List<T> SearchResult { get; private set; }

    private readonly int resultLimit;
    private readonly int throttleInterval;
    private readonly int retryInterval;
    private readonly int maxRetries;
    private readonly List<List<string>> preprocessedData;
    private readonly ConcurrentDictionary<string, List<int>> cache = [];

    public void Search(string keyword, bool isIgnoreCase = true, bool isRegex = true)
    {
        string cacheKey = $"{keyword}_{isIgnoreCase}_{isRegex}";
        
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

        Task.Delay(retryInterval).ContinueWith(_ =>
        {
            if (!SearchThrottler.Throttle(Guid, throttleInterval))
                RetrySearch(keyword, isIgnoreCase, isRegex, attempt + 1);
            else
                ExecuteSearch(keyword, isIgnoreCase, isRegex);
        });
    }

    private void ExecuteSearch(string keyword, bool isIgnoreCase, bool isRegex)
    {
        var comparison = isIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var partitioner = Partitioner.Create(0, preprocessedData.Count);
        var indexes = new ConcurrentBag<int>();
        
        RegexOptions regexOptions = isIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        Regex? regex = null;
        
        if (isRegex)
        {
            try
            {
                regex = new Regex(keyword, regexOptions);
            }
            catch
            {
                // 正则表达式无效，回退到普通搜索
                isRegex = false;
            }
        }

        Parallel.ForEach(partitioner, range =>
        {
            for (var i = range.Item1; i < range.Item2; i++)
            {
                var itemStrings = preprocessedData[i];
                if (isRegex && regex != null)
                {
                    if (itemStrings.Any(str => regex.IsMatch(str)))
                        indexes.Add(i);
                }
                else
                {
                    if (itemStrings.Any(str => str.Contains(keyword, comparison)))
                        indexes.Add(i);
                }
            }
        });

        var resultIndexes = indexes
            .OrderBy(i => i)
            .Take(resultLimit)
            .ToList();

        SearchResult = resultIndexes.Select(index => Data[index]).ToList();
        cache[$"{keyword}_{isIgnoreCase}_{isRegex}"] = resultIndexes;
    }
}

public class LuminaSearcherSubRow<T> where T : struct, IExcelSubrow<T>
{
    private readonly Throttler<Guid> SearchThrottler = new();

    public LuminaSearcherSubRow(
        IEnumerable<T> data,
        Func<T, string>[] searchFuncs,
        Func<T, string> orderFunc,
        int resultLimit = 100,
        int throttleInterval = 100,
        int retryInterval = 50,
        int maxRetries = 3)
    {
        Guid = Guid.NewGuid();
        Data = data
            .OrderBy(item => orderFunc(item).Length)
            .ThenBy(x => x.RowId)
            .ToList();

        SearchResult = Data.Take(resultLimit).ToList();
        this.resultLimit = resultLimit;
        this.throttleInterval = throttleInterval;
        this.retryInterval = retryInterval;
        this.maxRetries = maxRetries;
        preprocessedData = Data
            .Select(item => searchFuncs.Select(func => func(item)).ToList())
            .ToList();
    }

    public Guid Guid { get; init; }
    public IReadOnlyList<T> Data { get; init; }
    public List<T> SearchResult { get; private set; }

    private readonly int resultLimit;
    private readonly int throttleInterval;
    private readonly int retryInterval;
    private readonly int maxRetries;
    private readonly List<List<string>> preprocessedData;
    private readonly ConcurrentDictionary<string, List<int>> cache = [];

    public void Search(string keyword, bool isIgnoreCase = true, bool isRegex = true)
    {
        string cacheKey = $"{keyword}_{isIgnoreCase}_{isRegex}";
        
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

        Task.Delay(retryInterval).ContinueWith(_ =>
        {
            if (!SearchThrottler.Throttle(Guid, throttleInterval))
                RetrySearch(keyword, isIgnoreCase, isRegex, attempt + 1);
            else
                ExecuteSearch(keyword, isIgnoreCase, isRegex);
        });
    }

    private void ExecuteSearch(string keyword, bool isIgnoreCase, bool isRegex)
    {
        var comparison = isIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var partitioner = Partitioner.Create(0, preprocessedData.Count);
        var indexes = new ConcurrentBag<int>();
        
        RegexOptions regexOptions = isIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        Regex? regex = null;
        
        if (isRegex)
        {
            try
            {
                regex = new Regex(keyword, regexOptions);
            }
            catch
            {
                // 正则表达式无效，回退到普通搜索
                isRegex = false;
            }
        }

        Parallel.ForEach(partitioner, range =>
        {
            for (var i = range.Item1; i < range.Item2; i++)
            {
                var itemStrings = preprocessedData[i];
                if (isRegex && regex != null)
                {
                    if (itemStrings.Any(str => regex.IsMatch(str)))
                        indexes.Add(i);
                }
                else
                {
                    if (itemStrings.Any(str => str.Contains(keyword, comparison)))
                        indexes.Add(i);
                }
            }
        });

        var resultIndexes = indexes
            .OrderBy(i => i)
            .Take(resultLimit)
            .ToList();

        SearchResult = resultIndexes.Select(index => Data[index]).ToList();
        cache[$"{keyword}_{isIgnoreCase}_{isRegex}"] = resultIndexes;
    }
}
