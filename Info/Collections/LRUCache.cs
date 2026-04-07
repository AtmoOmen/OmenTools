using System.Collections.Immutable;
using StandardTimeManager = OmenTools.OmenService.StandardTimeManager;
using Timer = System.Threading.Timer;

namespace OmenTools.Info.Collections;

public class LRUCache<TKey, TValue> : IDisposable where TKey : notnull
{
    private readonly Dictionary<TKey, LinkedListNode<CacheItem>> cache   = [];
    private readonly LinkedList<CacheItem>                       lruList = [];
    private readonly Lock                                        gate    = new();

    private readonly int      capacity;
    private readonly TimeSpan defaultExpiration;
    private readonly Timer    cleanupTimer;
    private readonly TimeSpan cleanupInterval;

    private bool isDisposed;

    private long totalRequests;
    private long cacheHits;

    public LRUCache(int capacity, TimeSpan? defaultExpiration = null, TimeSpan? cleanupInterval = null)
    {
        this.capacity          = capacity > 0 ? capacity : throw new ArgumentException("容量必须大于 0", nameof(capacity));
        this.defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(30);
        this.cleanupInterval   = cleanupInterval   ?? TimeSpan.FromMinutes(5);

        cleanupTimer = new Timer(CleanupExpiredItems, null, this.cleanupInterval, this.cleanupInterval);
        RegisterMemoryPressureNotification();
    }

    public int Count
    {
        get
        {
            lock (gate)
            {
                return cache.Count;
            }
        }
    }

    public double HitRate
    {
        get
        {
            var requests = Interlocked.Read(ref totalRequests);
            return requests > 0 ? (double)Interlocked.Read(ref cacheHits) / requests : 0;
        }
    }

    public long TotalRequests => Interlocked.Read(ref totalRequests);

    public long CacheHits => Interlocked.Read(ref cacheHits);

    public ImmutableList<KeyValuePair<TKey, TValue>> GetItems() =>
        GetSnapshotItems().ToImmutableList();

    public KeyValuePair<TKey, TValue>[] GetSnapshotItems()
    {
        lock (gate)
        {
            var snapshot = new KeyValuePair<TKey, TValue>[cache.Count];
            var index    = 0;

            foreach (var pair in cache)
                snapshot[index++] = new(pair.Key, pair.Value.Value.Value);

            return snapshot;
        }
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) =>
        GetOrAdd(key, valueFactory, defaultExpiration);

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory, TimeSpan expiration)
    {
        Interlocked.Increment(ref totalRequests);

        lock (gate)
        {
            var now = StandardTimeManager.Instance().UTCNow;

            if (TryGetNodeValueNoLock(key, now, out var node, out var value))
            {
                MoveToFrontNoLock(node);
                Interlocked.Increment(ref cacheHits);
                return value;
            }

            value = valueFactory(key);
            AddNewNoLock(key, value, now.Add(expiration), false);
            return value;
        }
    }

    public TValue GetOrAddPermanent(TKey key, Func<TKey, TValue> valueFactory)
    {
        Interlocked.Increment(ref totalRequests);

        lock (gate)
        {
            var now = StandardTimeManager.Instance().UTCNow;

            if (TryGetNodeValueNoLock(key, now, out var node, out var value))
            {
                MoveToFrontNoLock(node);
                Interlocked.Increment(ref cacheHits);
                return value;
            }

            value = valueFactory(key);
            AddNewNoLock(key, value, DateTime.MaxValue, true);
            return value;
        }
    }

    public bool TryGet(TKey key, out TValue value)
    {
        Interlocked.Increment(ref totalRequests);

        lock (gate)
        {
            if (!TryGetNodeValueNoLock(key, StandardTimeManager.Instance().UTCNow, out var node, out value))
                return false;

            MoveToFrontNoLock(node);
            Interlocked.Increment(ref cacheHits);
            return true;
        }
    }

    public bool TryUpdate(TKey key, TValue newValue, TimeSpan? expiration = null, bool? isPermanent = null)
    {
        lock (gate)
        {
            if (!cache.TryGetValue(key, out var node))
                return false;

            var permanent = isPermanent ?? node.Value.IsPermanent;
            var expiresAt = permanent
                                ? DateTime.MaxValue
                                : StandardTimeManager.Instance().UTCNow.Add(expiration ?? defaultExpiration);

            node.Value = new CacheItem(key, newValue, expiresAt, permanent);
            MoveToFrontNoLock(node);
            return true;
        }
    }

    public bool TryRemove(TKey key, out TValue value)
    {
        lock (gate)
        {
            if (!cache.TryGetValue(key, out var node))
            {
                value = default!;
                return false;
            }

            value = node.Value.Value;
            RemoveNodeNoLock(node);
            return true;
        }
    }

    public void Clear()
    {
        lock (gate)
        {
            if (cache.Count == 0)
            {
                ResetStats();
                return;
            }

            var permanentItems = new List<CacheItem>();

            foreach (var item in lruList)
            {
                if (item.IsPermanent)
                    permanentItems.Add(item);
            }

            cache.Clear();
            lruList.Clear();

            foreach (var item in permanentItems)
            {
                var node = lruList.AddFirst(item);
                cache.Add(item.Key, node);
            }

            ResetStats();
        }
    }

    public void ClearAll(bool includePermanentItems = false)
    {
        if (includePermanentItems)
        {
            lock (gate)
            {
                cache.Clear();
                lruList.Clear();
                ResetStats();
            }

            return;
        }

        Clear();
    }

    public void RemoveAll(IEnumerable<TKey> keys)
    {
        lock (gate)
        {
            foreach (var key in keys)
            {
                if (cache.TryGetValue(key, out var node))
                    RemoveNodeNoLock(node);
            }
        }
    }

    public void AddOrUpdateAll(IEnumerable<KeyValuePair<TKey, TValue>> items, TimeSpan? expiration = null, bool isPermanent = false)
    {
        lock (gate)
        {
            var now       = StandardTimeManager.Instance().UTCNow;
            var expiresAt = isPermanent ? DateTime.MaxValue : now.Add(expiration ?? defaultExpiration);

            foreach (var item in items)
            {
                if (cache.TryGetValue(item.Key, out var node))
                {
                    node.Value = new CacheItem(item.Key, item.Value, expiresAt, isPermanent);
                    MoveToFrontNoLock(node);
                    continue;
                }

                AddNewNoLock(item.Key, item.Value, expiresAt, isPermanent);
            }
        }
    }

    public void AddOrUpdatePermanent(TKey key, TValue value)
    {
        lock (gate)
        {
            if (cache.TryGetValue(key, out var node))
            {
                node.Value = new CacheItem(key, value, DateTime.MaxValue, true);
                MoveToFrontNoLock(node);
                return;
            }

            AddNewNoLock(key, value, DateTime.MaxValue, true);
        }
    }

    public void ResetStats()
    {
        Interlocked.Exchange(ref totalRequests, 0);
        Interlocked.Exchange(ref cacheHits,     0);
    }

    public void Dispose()
    {
        if (isDisposed)
            return;

        isDisposed = true;

        ClearAll(true);
        cleanupTimer.Dispose();
    }

    private bool TryGetNodeValueNoLock(TKey key, DateTime utcNow, out LinkedListNode<CacheItem> node, out TValue value)
    {
        if (!cache.TryGetValue(key, out node!))
        {
            value = default!;
            return false;
        }

        if (!node.Value.IsPermanent && utcNow > node.Value.ExpirationTime)
        {
            RemoveNodeNoLock(node);
            value = default!;
            return false;
        }

        value = node.Value.Value;
        return true;
    }

    private void AddNewNoLock(TKey key, TValue value, DateTime expirationTime, bool isPermanent)
    {
        if (cache.Count >= capacity)
            RemoveOldestNoLock();

        var node = lruList.AddFirst(new CacheItem(key, value, expirationTime, isPermanent));
        cache[key] = node;
    }

    private void RemoveNodeNoLock(LinkedListNode<CacheItem> node)
    {
        lruList.Remove(node);
        cache.Remove(node.Value.Key);
    }

    private void MoveToFrontNoLock(LinkedListNode<CacheItem> node)
    {
        if (ReferenceEquals(lruList.First, node))
            return;

        lruList.Remove(node);
        lruList.AddFirst(node);
    }

    private void RemoveOldestNoLock()
    {
        var node = lruList.Last;

        while (node is { Value.IsPermanent: true })
            node = node.Previous;

        if (node != null)
        {
            RemoveNodeNoLock(node);
            return;
        }

        if (lruList.Last != null)
            RemoveNodeNoLock(lruList.Last);
    }

    private void CleanupExpiredItems(object? state)
    {
        if (isDisposed)
            return;

        try
        {
            lock (gate)
            {
                var now  = StandardTimeManager.Instance().UTCNow;
                var node = lruList.Last;

                while (node != null)
                {
                    var previous = node.Previous;

                    if (!node.Value.IsPermanent && node.Value.ExpirationTime < now)
                        RemoveNodeNoLock(node);

                    node = previous;
                }
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void RegisterMemoryPressureNotification()
    {
        Task.Run
        (async () =>
            {
                while (!isDisposed)
                {
                    try
                    {
                        if (GC.GetTotalMemory(false) > 1024 * 1024 * 100)
                            TrimCache(0.5);

                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        );
    }

    private void TrimCache(double percentage)
    {
        if (percentage is <= 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(percentage), "百分比必须在 0 到 1 之间");

        lock (gate)
        {
            var nonPermanentCount = 0;

            foreach (var item in lruList)
            {
                if (!item.IsPermanent)
                    nonPermanentCount++;
            }

            var itemsToRemove = (int)(nonPermanentCount * percentage);
            var node          = lruList.Last;

            for (var removed = 0; removed < itemsToRemove && node != null;)
            {
                var previous = node.Previous;

                if (!node.Value.IsPermanent)
                {
                    RemoveNodeNoLock(node);
                    removed++;
                }

                node = previous;
            }
        }
    }

    private readonly record struct CacheItem
    (
        TKey     Key,
        TValue   Value,
        DateTime ExpirationTime,
        bool     IsPermanent = false
    );
}
