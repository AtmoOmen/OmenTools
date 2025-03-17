using System.Collections.Immutable;
using Timer = System.Threading.Timer;

namespace OmenTools.Helpers;

public class LRUCache<TKey, TValue> : IDisposable where TKey : notnull
{
    private readonly int                                         capacity;
    private readonly ReaderWriterLockSlim                        lockSlim = new();
    private readonly Dictionary<TKey, LinkedListNode<CacheItem>> cache    = new();
    private readonly LinkedList<CacheItem>                       lruList  = new();
    private readonly TimeSpan                                    defaultExpiration;
    private readonly Timer                                       cleanupTimer;
    private readonly TimeSpan                                    cleanupInterval;
    private          bool                                        isDisposed;

    private          long   totalRequests;
    private          long   cacheHits;
    private readonly object statsLock = new();

    public LRUCache(int capacity, TimeSpan? defaultExpiration = null, TimeSpan? cleanupInterval = null)
    {
        this.capacity          = capacity > 0 ? capacity : throw new ArgumentException("容量必须大于0", nameof(capacity));
        this.defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(30);
        this.cleanupInterval   = cleanupInterval   ?? TimeSpan.FromMinutes(5);

        cleanupTimer = new Timer(CleanupExpiredItems, null, this.cleanupInterval, this.cleanupInterval);

        RegisterMemoryPressureNotification();
    }

    public int Count
    {
        get
        {
            lockSlim.EnterReadLock();
            try
            {
                return cache.Count;
            } 
            finally
            {
                lockSlim.ExitReadLock();
            }
        }
    }

    public double HitRate
    {
        get
        {
            lock (statsLock)
            {
                return totalRequests > 0 ? (double)cacheHits / totalRequests : 0;
            }
        }
    }

    public long TotalRequests
    {
        get
        {
            lock (statsLock)
            {
                return totalRequests;
            }
        }
    }

    public long CacheHits
    {
        get
        {
            lock (statsLock)
            {
                return cacheHits;
            }
        }
    }

    public ImmutableList<KeyValuePair<TKey, TValue>> GetItems()
    {
        lockSlim.EnterReadLock();
        try
        {
            return cache.Select(pair => new KeyValuePair<TKey, TValue>(pair.Key, pair.Value.Value.Value))
                        .ToImmutableList();
        } 
        finally
        {
            lockSlim.ExitReadLock();
        }
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) => GetOrAdd(key, valueFactory, defaultExpiration);

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory, TimeSpan expiration)
    {
        IncrementTotalRequests();

        if (TryGet(key, out var value))
        {
            IncrementCacheHits();
            return value;
        }

        lockSlim.EnterWriteLock();
        try
        {
            if (cache.TryGetValue(key, out var node))
            {
                IncrementCacheHits();
                lruList.Remove(node);
                lruList.AddFirst(node);
                return node.Value.Value;
            }

            value = valueFactory(key);

            if (cache.Count >= capacity)
                RemoveOldest();

            var cacheItem = new CacheItem(key, value, DateTime.UtcNow.Add(expiration));
            var newNode   = lruList.AddFirst(cacheItem);
            cache.Add(key, newNode);

            return value;
        } finally { lockSlim.ExitWriteLock(); }
    }

    public bool TryGet(TKey key, out TValue value)
    {
        IncrementTotalRequests();

        lockSlim.EnterUpgradeableReadLock();
        try
        {
            if (cache.TryGetValue(key, out var node))
            {
                if (DateTime.UtcNow > node.Value.ExpirationTime)
                {
                    lockSlim.EnterWriteLock();
                    try
                    {
                        lruList.Remove(node);
                        cache.Remove(key);
                    } finally { lockSlim.ExitWriteLock(); }

                    value = default!;
                    return false;
                }

                lockSlim.EnterWriteLock();
                try
                {
                    lruList.Remove(node);
                    lruList.AddFirst(node);
                } finally { lockSlim.ExitWriteLock(); }

                IncrementCacheHits();
                value = node.Value.Value;
                return true;
            }

            value = default!;
            return false;
        } finally { lockSlim.ExitUpgradeableReadLock(); }
    }

    public bool TryUpdate(TKey key, TValue newValue, TimeSpan? expiration = null)
    {
        lockSlim.EnterWriteLock();
        try
        {
            if (cache.TryGetValue(key, out var node))
            {
                var newExpiration = expiration.HasValue
                                        ? DateTime.UtcNow.Add(expiration.Value)
                                        : DateTime.UtcNow.Add(defaultExpiration);

                var newCacheItem = new CacheItem(key, newValue, newExpiration);

                lruList.Remove(node);
                var newNode = lruList.AddFirst(newCacheItem);
                cache[key] = newNode;

                return true;
            }

            return false;
        } finally { lockSlim.ExitWriteLock(); }
    }

    public bool TryRemove(TKey key, out TValue value)
    {
        lockSlim.EnterWriteLock();
        try
        {
            if (cache.TryGetValue(key, out var node))
            {
                value = node.Value.Value;
                lruList.Remove(node);
                cache.Remove(key);
                return true;
            }

            value = default!;
            return false;
        } finally { lockSlim.ExitWriteLock(); }
    }

    public void Clear()
    {
        lockSlim.EnterWriteLock();
        try
        {
            cache.Clear();
            lruList.Clear();

            ResetStats();
        } finally { lockSlim.ExitWriteLock(); }
    }

    public void RemoveAll(IEnumerable<TKey> keys)
    {
        if (keys == null) return;

        lockSlim.EnterWriteLock();
        try
        {
            foreach (var key in keys)
                if (cache.TryGetValue(key, out var node))
                {
                    lruList.Remove(node);
                    cache.Remove(key);
                }
        } 
        finally
        {
            lockSlim.ExitWriteLock();
        }
    }

    public void AddOrUpdateAll(IEnumerable<KeyValuePair<TKey, TValue>> items, TimeSpan? expiration = null)
    {
        if (items == null) return;

        lockSlim.EnterWriteLock();
        try
        {
            foreach (var item in items)
            {
                var expirationTime = DateTime.UtcNow.Add(expiration ?? defaultExpiration);

                if (cache.TryGetValue(item.Key, out var existingNode))
                {
                    lruList.Remove(existingNode);
                    var newCacheItem = new CacheItem(item.Key, item.Value, expirationTime);
                    var newNode      = lruList.AddFirst(newCacheItem);
                    cache[item.Key] = newNode;
                }
                else
                {
                    if (cache.Count >= capacity)
                        RemoveOldest();

                    var cacheItem = new CacheItem(item.Key, item.Value, expirationTime);
                    var newNode   = lruList.AddFirst(cacheItem);
                    cache.Add(item.Key, newNode);
                }
            }
        } finally { lockSlim.ExitWriteLock(); }
    }

    public void ResetStats()
    {
        lock (statsLock)
        {
            totalRequests = 0;
            cacheHits     = 0;
        }
    }

    private void RemoveOldest()
    {
        if (lruList.Last != null)
        {
            cache.Remove(lruList.Last.Value.Key);
            lruList.RemoveLast();
        }
    }

    private void CleanupExpiredItems(object? state)
    {
        try
        {
            if (!lockSlim.TryEnterWriteLock(100))
                return;

            try
            {
                var now     = DateTime.UtcNow;
                var current = lruList.Last;

                while (current != null)
                {
                    var next = current.Previous;

                    if (current.Value.ExpirationTime < now)
                    {
                        cache.Remove(current.Value.Key);
                        lruList.Remove(current);
                    }

                    current = next;
                }
            } finally { lockSlim.ExitWriteLock(); }
        }
        catch (ObjectDisposedException)
        {
            // ignored
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void IncrementTotalRequests()
    {
        lock (statsLock) { totalRequests++; }
    }

    private void IncrementCacheHits()
    {
        lock (statsLock) { cacheHits++; }
    }

    private void RegisterMemoryPressureNotification()
    {
        Task.Run(async () =>
        {
            while (!isDisposed)
            {
                try
                {
                    // 超过 100mb 时
                    if (GC.GetTotalMemory(false) > 1024 * 1024 * 100)
                    {
                        TrimCache(0.5);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        });
    }

    private void TrimCache(double percentage)
    {
        if (percentage <= 0 || percentage > 1)
            throw new ArgumentOutOfRangeException(nameof(percentage), "百分比必须在0到1之间");

        lockSlim.EnterWriteLock();
        try
        {
            var itemsToRemove = (int)(cache.Count * percentage);
            for (var i = 0; i < itemsToRemove && lruList.Last != null; i++)
            {
                cache.Remove(lruList.Last.Value.Key);
                lruList.RemoveLast();
            }
        } finally { lockSlim.ExitWriteLock(); }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed) return;

        if (disposing)
        {
            cleanupTimer.Dispose();
            lockSlim.Dispose();
            isDisposed = true;
        }
    }

    ~LRUCache()
    {
        Dispose(false);
    }

    private class CacheItem
    {
        public TKey     Key            { get; }
        public TValue   Value          { get; }
        public DateTime ExpirationTime { get; }

        public CacheItem(TKey key, TValue value, DateTime expirationTime)
        {
            Key            = key;
            Value          = value;
            ExpirationTime = expirationTime;
        }
    }
}
