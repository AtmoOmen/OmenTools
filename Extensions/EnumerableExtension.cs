using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace OmenTools.Extensions;

public static class EnumerableExtension
{
    #region Queue

    public static T? DequeueOrDefault<T>(this Queue<T?> queue) => queue.Count > 0 ? queue.Dequeue() : default;

    #endregion

    #region HashSet

    public static bool Toggle<T>(this HashSet<T> hashSet, T value)
    {
        if (!hashSet.Add(value))
        {
            hashSet.Remove(value);
            return false;
        }

        return true;
    }

    #endregion

    #region ICollection<T>

    extension<T>(ICollection<T> collection)
    {
        public void AddRange(params T[] values)
        {
            foreach (var x in values)
                collection.Add(x);
        }

        public void RemoveRange(params T[] values)
        {
            foreach (var x in values)
                collection.Remove(x);
        }

        public void Add(params T[] values) =>
            collection.AddRange(values);

        public void Remove(params T[] values) =>
            collection.RemoveRange(values);

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Add(item);
        }
    }

    #endregion

    #region ConcurrentBag

    extension<T>(ConcurrentBag<T> bag)
    {
        public void AddRange(IEnumerable<T> items)
        {
            if (bag == null)
                throw new ArgumentNullException(nameof(bag));
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
                bag.Add(item);
        }

        public void ForEach(Action<T> action)
        {
            if (bag == null)
                throw new ArgumentNullException(nameof(bag));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in bag)
                action(item);
        }
    }

    #endregion

    #region IEnumerable<T>

    extension<T>(IEnumerable<T> source)
    {
        public ConcurrentBag<T> ToConcurrentBag()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new ConcurrentBag<T>(source);
        }

        public ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>
        (
            Func<T, TKey>   keySelector,
            Func<T, TValue> valueSelector
        ) where TKey : notnull
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));

            return new ConcurrentDictionary<TKey, TValue>(source.ToDictionary(keySelector, valueSelector));
        }

        public ConcurrentDictionary<TKey, T> ToConcurrentDictionary<TKey>(Func<T, TKey> keySelector) where TKey : notnull =>
            source.ToConcurrentDictionary(keySelector, item => item);

        public void ForEach(Action<T> action)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in source)
                action(item);
        }

        public T FirstOr0(Func<T, bool> predicate)
        {
            var enumerable = source as T[] ?? source.ToArray();

            foreach (var x in enumerable)
            {
                if (predicate(x))
                    return x;
            }

            return enumerable.First();
        }

        public IEnumerable<TR> SelectMulti<TR>(params Func<T, TR>[] funcs)
            => from v in source from x in funcs select x(v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsAll(IEnumerable<T> values)
            => values.All(source.Contains);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsAny(params T[] values)
        {
            var enumerable = source as T[] ?? source.ToArray();
            return values.Any(x => enumerable.Contains(x));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsAny(IEnumerable<T> values)
        {
            var enumerable = source as T[] ?? source.ToArray();
            return values.Any(x => enumerable.Contains(x));
        }

        public int IndexOf(Predicate<T> predicate)
        {
            var ret = -1;

            foreach (var v in source)
            {
                ret++;
                if (predicate(v))
                    return ret;
            }

            return -1;
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

    public static bool TryGetLast<TK>(this IEnumerable<TK?> enumerable, Func<TK?, bool> predicate, out TK? value)
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

    #endregion

    #region IList<T>

    extension<T>(IList<T> list)
    {
        public void Swap(int index1, int index2)
        {
            if (index1 < 0 || index1 >= list.Count || index2 < 0 || index2 >= list.Count)
                throw new IndexOutOfRangeException("无法交换元素, 因为其中一个索引无效");

            (list[index1], list[index2]) = (list[index2], list[index1]);
        }

        public bool TryDequeue(out T? result)
        {
            if (list.Count > 0)
            {
                result = list[0];
                list.RemoveAt(0);
                return true;
            }

            result = default;
            return false;
        }

        public T? Dequeue()
        {
            if (list.TryDequeue(out var ret)) return ret;

            throw new InvalidOperationException("Sequence contains no elements");
        }

        public bool TryDequeueLast(out T? result)
        {
            if (list.Count > 0)
            {
                result = list[^1];
                list.RemoveAt(list.Count - 1);
                return true;
            }

            result = default;
            return false;
        }

        public T? DequeueLast()
        {
            if (list.TryDequeueLast(out var ret))
                return ret;

            throw new InvalidOperationException("序列中未包含任何有效元素");
        }

        public T? DequeueOrDefault()
        {
            if (list.Count <= 0) return default;

            var ret = list[0];
            list.RemoveAt(0);
            return ret;
        }
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

    #endregion

    #region IDictionary / ConcurrentDictionary

    extension<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
                dictionary.TryAdd(item.Key, item.Value);
        }

        public void ForEach(Action<TKey, TValue> action)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var kvp in dictionary)
                action(kvp.Key, kvp.Value);
        }
    }

    extension<TK, TV>(IDictionary<TK, TV> dictionary)
    {
        public IEnumerable<TK> FindKeysByValue(TV value) =>
            from x in dictionary where value != null && value.Equals(x.Value) select x.Key;

        public bool TryGetFirst(Func<KeyValuePair<TK, TV>, bool> predicate, out KeyValuePair<TK, TV> keyValuePair)
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

        public TV? GetOrDefault(TK key) =>
            dictionary.TryGetValue(key, out var value) ? value : default;

        public TV GetOrAdd(TK key, Func<TK, TV> valueFactory)
        {
            if (dictionary.TryGetValue(key, out var value)) return value;
            value           = valueFactory(key);
            dictionary[key] = value;
            return value;
            // Note: This implementation is not thread-safe for standard Dictionary, unlike ConcurrentDictionary.GetOrAdd
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IncrementOrSet<TK>(this IDictionary<TK, int> dic, TK key, int increment = 1)
    {
        if (!dic.TryAdd(key, increment))
            dic[key] += increment;

        return dic[key];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TV? GetSafe<TK, TV>(this IDictionary<TK, TV>? dic, TK key, TV? defaultValue = default) =>
        dic?.TryGetValue(key, out var value) == true ? value : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TV GetOrCreate<TK, TV>(this IDictionary<TK, TV> dictionary, TK key) where TV : new()
    {
        if (dictionary.TryGetValue(key, out var result)) return result;

        var newValue = new TV();
        dictionary.Add(key, newValue);
        return newValue;
    }

    #endregion

    #region Object / Array

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAny<T>(this T obj, IEnumerable<T> values) =>
        values.Any(x => x != null && x.Equals(obj));

    public static T[] Together<T>(this T[] array, params T[] additionalValues) => array.Union(additionalValues).ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] CreateArray<T>(this T o, uint repeatCount)
    {
        var arr = new T[repeatCount];
        for (var i = 0; i < arr.Length; i++)
            arr[i] = o;

        return arr;
    }

    #endregion
}
