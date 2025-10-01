using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace OmenTools.Helpers;

public static class EnumerableExtension
{
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
    {
        if (dictionary.TryGetValue(key, out var value)) return value;
        value           = valueFactory(key);
        dictionary[key] = value;
        return value;
    }

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
            collection.Add(item);
    }
    
    public static void AddRange<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items)
        where TKey : notnull
    {
        if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
        if (items      == null) throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
            dictionary.TryAdd(item.Key, item.Value);
    }
    
    public static void AddRange<T>(this ConcurrentBag<T> bag, IEnumerable<T> items)
    {
        if (bag   == null) throw new ArgumentNullException(nameof(bag));
        if (items == null) throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
            bag.Add(item);
    }

    public static ConcurrentBag<T> ToConcurrentBag<T>(this IEnumerable<T> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        return new ConcurrentBag<T>(source);
    }

    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey>       keySelector,
        Func<TSource, TValue>     valueSelector) where TKey : notnull
    {
        if (source        == null) throw new ArgumentNullException(nameof(source));
        if (keySelector   == null) throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));

        return new ConcurrentDictionary<TKey, TValue>(
            source.ToDictionary(keySelector, valueSelector));
    }

    public static ConcurrentDictionary<TKey, TSource> ToConcurrentDictionary<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey>       keySelector) where TKey : notnull =>
        source.ToConcurrentDictionary(keySelector, item => item);

    public static void ForEach<T>(this ConcurrentBag<T> bag, Action<T> action)
    {
        if (bag    == null) throw new ArgumentNullException(nameof(bag));
        if (action == null) throw new ArgumentNullException(nameof(action));

        foreach (var item in bag)
            action(item);
    }

    public static void ForEach<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, Action<TKey, TValue> action) where TKey : notnull
    {
        if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
        if (action     == null) throw new ArgumentNullException(nameof(action));

        foreach (var kvp in dictionary)
            action(kvp.Key, kvp.Value);
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (action == null) throw new ArgumentNullException(nameof(action));

        foreach (var item in source)
            action(item);
    }

    public static bool Toggle<T>(this HashSet<T> hashSet, T value)
    {
        if (!hashSet.Add(value))
        {
            hashSet.Remove(value);
            return false;
        }

        return true;
    }

    public static bool Toggle<T>(this List<T> list, T value)
    {
        if (list.Contains(value))
        {
            list.RemoveAll(x => x != null && x.Equals(value));
            return false;
        }

        list.Add(value);
        return true;
    }

    public static T FirstOr0<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        var enumerable = collection as T[] ?? collection.ToArray();
        foreach (var x in enumerable)
            if (predicate(x))
                return x;

        return enumerable.First();
    }

    public static IEnumerable<R> SelectMulti<T, R>(this IEnumerable<T> values, params Func<T, R>[] funcs)
        => from v in values from x in funcs select x(v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
        => values.All(source.Contains);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny<T>(this IEnumerable<T> obj, params T[] values)
    {
        var enumerable = obj as T[] ?? obj.ToArray();
        return values.Any(x => enumerable.Contains(x));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny<T>(this IEnumerable<T> obj, IEnumerable<T> values)
    {
        var enumerable = obj as T[] ?? obj.ToArray();
        return values.Any(x => enumerable.Contains(x));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAny<T>(this T obj, IEnumerable<T> values) => 
        values.Any(x => x != null && x.Equals(obj));

    public static IEnumerable<K> FindKeysByValue<K, V>(this IDictionary<K, V> dictionary, V value) => 
        from x in dictionary where value != null && value.Equals(x.Value) select x.Key;

    public static bool TryGetFirst<K, V>(this IDictionary<K, V> dictionary, Func<KeyValuePair<K, V>, bool> predicate, out KeyValuePair<K, V> keyValuePair)
    {
        try
        {
            keyValuePair = dictionary.First(predicate);
            return true;
        }
        catch
        {
            keyValuePair = default;
            return false;
        }
    }

    public static bool TryGetFirst<TSource>(this IEnumerable<TSource>? source, out TSource? value)
    {
        switch (source)
        {
            case null:
                break;
            
            case IList<TSource> list:
            {
                if (list.Count > 0)
                {
                    value = list[0];
                    return true;
                }

                break;
            }
            
            default:
            {
                using IEnumerator<TSource?> e = source.GetEnumerator();
                if (e.MoveNext())
                {
                    value = e.Current;
                    return true;
                }

                break;
            }
        }

        value = default;
        return false;
    }

    public static bool TryGetFirst<TSource>(this IEnumerable<TSource?>? source, Func<TSource, bool>? predicate, out TSource? value)
    {
        if (source == null || predicate == null)
        {
            value = default;
            return false;
        }

        foreach (var element in source)
        {
            if (element != null && predicate(element))
            {
                value = element;
                return true;
            }
        }

        value = default;
        return false;
    }

    public static bool TryGetLast<K>(this IEnumerable<K?> enumerable, Func<K?, bool> predicate, out K? value)
    {
        try
        {
            value = enumerable.Last(predicate);
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    public static void Swap<T>(this IList<T> list, int index1, int index2)
    {
        if (index1 < 0 || index1 >= list.Count || index2 < 0 || index2 >= list.Count)
            throw new IndexOutOfRangeException("无法交换元素, 因为其中一个索引无效");

        (list[index1], list[index2]) = (list[index2], list[index1]);
    }

    public static bool TryDequeue<T>(this IList<T> List, out T? result)
    {
        if (List.Count > 0)
        {
            result = List[0];
            List.RemoveAt(0);
            return true;
        }

        result = default;
        return false;
    }

    public static T? Dequeue<T>(this IList<T> List)
    {
        if (List.TryDequeue(out var ret)) return ret;

        throw new InvalidOperationException("Sequence contains no elements");
    }

    public static void AddRange<T>(this ICollection<T> collection, params T[] values)
    {
        foreach (var x in values)
            collection.Add(x);
    }

    public static void RemoveRange<T>(this ICollection<T> collection, params T[] values)
    {
        foreach (var x in values)
            collection.Remove(x);
    }
    
    public static void Add<T>(this ICollection<T> collection, params T[] values) => 
        collection.AddRange(values);

    public static void Remove<T>(this ICollection<T> collection, params T[] values) => 
        collection.RemoveRange(values);
    
    public static bool TryDequeueLast<T>(this IList<T> List, out T? result)
    {
        if (List.Count > 0)
        {
            result = List[^1];
            List.RemoveAt(List.Count - 1);
            return true;
        }

        result = default;
        return false;
    }

    public static T? DequeueLast<T>(this IList<T> List)
    {
        if (List.TryDequeueLast(out var ret)) 
            return ret;

        throw new InvalidOperationException("序列中未包含任何有效元素");
    }

    public static T? DequeueOrDefault<T>(this IList<T> List)
    {
        if (List.Count <= 0) return default;

        var ret = List[0];
        List.RemoveAt(0);
        return ret;
    }

    public static T? DequeueOrDefault<T>(this Queue<T?> Queue) => Queue.Count > 0 ? Queue.Dequeue() : default;

    public static int IndexOf<T>(this IEnumerable<T> values, Predicate<T> predicate)
    {
        var ret = -1;
        foreach (var v in values)
        {
            ret++;
            if (predicate(v))
                return ret;
        }

        return -1;
    }

    public static T[] Together<T>(this T[] array, params T[] additionalValues) => array.Union(additionalValues).ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] CreateArray<T>(this T o, uint repeatCount)
    {
        var arr = new T[repeatCount];
        for (var i = 0; i < arr.Length; i++)
            arr[i] = o;

        return arr;
    }

    public static V? GetOrDefault<K, V>(this IDictionary<K, V> dic, K key) =>
        dic.TryGetValue(key, out var value) ? value : default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IncrementOrSet<K>(this IDictionary<K, int> dic, K key, int increment = 1)
    {
        if (!dic.TryAdd(key, increment)) 
            dic[key] += increment;

        return dic[key];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static V? GetSafe<K, V>(this IDictionary<K, V>? dic, K key, V? Default = default) => 
        dic?.TryGetValue(key, out var value) == true ? value : Default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static V GetOrCreate<K, V>(this IDictionary<K, V> dictionary, K key) where V : new()
    {
        if (dictionary.TryGetValue(key, out var result)) return result;

        var newValue = new V();
        dictionary.Add(key, newValue);
        return newValue;
    }
}
