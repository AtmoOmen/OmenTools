using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using SeString = Lumina.Text.SeString;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    public static string ExtractText(this SeString s, bool onlyFirst = false) => s.ToDalamudString().ExtractText(onlyFirst);

    public static string ExtractText(this Utf8String s)
    {
        var str = MemoryHelper.ReadSeString(&s);
        return str.ExtractText();
    }

    public static string ExtractText(this Dalamud.Game.Text.SeStringHandling.SeString seStr, bool onlyFirst = false)
    {
        StringBuilder sb = new();
        foreach (var x in seStr.Payloads)
        {
            if (x is not TextPayload tp) continue;
            sb.Append(tp.Text);
            if (onlyFirst) break;
        }

        return sb.ToString();
    }

    public static void Restart(this Timer timer)
    {
        timer.Stop();
        timer.Start();
    }

    public static string ToHexString(this IEnumerable<byte> bytes, char separator = ' ')
    {
        var first = true;
        var sb = new StringBuilder();
        foreach (var x in bytes)
        {
            if (first)
                first = false;
            else
                sb.Append(separator);

            sb.Append($"{x:X2}");
        }

        return sb.ToString();
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
        if (List.TryDequeueLast(out var ret)) return ret;

        throw new InvalidOperationException("Sequence contains no elements");
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

    public static bool ContainsIgnoreCase(this IEnumerable<string> haystack, string needle) 
        => haystack.Any(x => x.EqualsIgnoreCase(needle));

    public static T[] Together<T>(this T[] array, params T[] additionalValues) => array.Union(additionalValues).ToArray();

    public static string? NullWhenFalse(this string? s, bool b) => b ? s : null;

    public static uint AsUInt32(this float f) { return *(uint*)&f; }

    public static float AsFloat(this uint u) { return *(float*)&u; }

    public static void Add<T>(this ICollection<T> collection, params T[] values)
    {
        foreach (var x in values)
            collection.Add(x);
    }

    public static void Remove<T>(this ICollection<T> collection, params T[] values)
    {
        foreach (var x in values)
            collection.Remove(x);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOr<T>(this T source, Predicate<T> testFunction) => source == null || testFunction(source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] CreateArray<T>(this T o, uint num)
    {
        var arr = new T[num];
        for (var i = 0; i < arr.Length; i++)
            arr[i] = o;

        return arr;
    }

    public static V? GetOrDefault<K, V>(this IDictionary<K, V> dic, K key) => dic.TryGetValue(key, out var value) ? value : default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IncrementOrSet<K>(this IDictionary<K, int> dic, K key, int increment = 1)
    {
        if (!dic.TryAdd(key, increment)) dic[key] += increment;

        return dic[key];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int? ParseInt(this string number)
    {
        if (int.TryParse(number, out var result))
            return result;

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static V? GetSafe<K, V>(this IDictionary<K, V>? dic, K key, V? Default = default)
        => dic?.TryGetValue(key, out var value) == true ? value : Default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static V GetOrCreate<K, V>(this IDictionary<K, V> dictionary, K key) where V : new()
    {
        if (dictionary.TryGetValue(key, out var result)) return result;

        var newValue = new V();
        dictionary.Add(key, newValue);
        return newValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Each<T>(this IEnumerable<T> collection, Action<T> function)
    {
        foreach (var x in collection) function(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool If<T>(this T obj, Func<T, bool> func) { return func(obj); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NotNull<T>(this T obj, [NotNullWhen(true)] out T outobj)
    {
        outobj = obj;
        return obj != null;
    }

    public static string ReplaceFirst(this string text, string search, string replace)
    {
        var pos = text.IndexOf(search, StringComparison.Ordinal);
        return pos < 0 ? text : string.Concat(text.AsSpan(0, pos), replace, text.AsSpan(pos + search.Length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithAny(this string source, params string[] values)
        => source.StartsWithAny(values, StringComparison.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithAny(this string source, StringComparison stringComparison = StringComparison.Ordinal,
                                     params string[] values)
        => source.StartsWithAny(values, stringComparison);

    public static bool StartsWithAny(
        this string source, IEnumerable<string> compareTo, StringComparison stringComparison = StringComparison.Ordinal)
    {
        foreach (var x in compareTo)
            if (source.StartsWith(x, stringComparison))
                return true;

        return false;
    }

    public static SeStringBuilder Add(this SeStringBuilder b, IEnumerable<Payload> payloads)
    {
        foreach (var x in payloads)
            b = b.Add(x);

        return b;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<string> Split(this string str, int chunkSize)
    {
        return Enumerable.Range(0, str.Length / chunkSize)
                         .Select(i => str.Substring(i * chunkSize, chunkSize));
    }

    public static T FirstOr0<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        var enumerable = collection as T[] ?? collection.ToArray();
        foreach (var x in enumerable)
            if (predicate(x))
                return x;

        return enumerable.First();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Default(this string s, string defaultValue)
        => string.IsNullOrEmpty(s) ? defaultValue : s;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCase(this string s, string other)
        => s.Equals(other, StringComparison.OrdinalIgnoreCase);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? NullWhenEmpty(this string? s)
        => s == string.Empty ? null : s;

    public static IEnumerable<R> SelectMulti<T, R>(this IEnumerable<T> values, params Func<T, R>[] funcs) 
        => from v in values from x in funcs select x(v);

    public static Vector4 Invert(this Vector4 v)
        => v with { X = 1f - v.X, Y = 1f - v.Y, Z = 1f - v.Z };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUint(this Vector4 color)
        => ImGui.ColorConvertFloat4ToU32(color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToVector4(this uint color)
        => ImGui.ColorConvertU32ToFloat4(color);

    public static ref int ValidateRange(this ref int i, int min, int max)
    {
        if (i > max) i = max;
        if (i < min) i = min;
        return ref i;
    }

    public static ref float ValidateRange(this ref float i, float min, float max)
    {
        if (i > max) i = max;
        if (i < min) i = min;
        return ref i;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Invert(this bool b, bool invert) { return invert ? !b : b; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values) 
        => values.All(source.Contains);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Cut(this string s, int num)
    {
        if (s.Length <= num) return s;
        return s[..num] + "...";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Repeat(this string s, int num)
    {
        StringBuilder str = new();
        for (var i = 0; i < num; i++)
            str.Append(s);

        return str.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(this IEnumerable<string> e, string separator)
        => string.Join(separator, e);

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
    public static bool ContainsAny(this string obj, IEnumerable<string> values) 
        => values.Any(obj.Contains);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(this string obj, params string[] values) 
        => values.Any(obj.Contains);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(this string obj, StringComparison comp, params string[] values) 
        => values.Any(x => obj.Contains(x, comp));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAny<T>(this T obj, params T[] values) 
        => values.Any(x => x != null && x.Equals(obj));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCaseAny(this string obj, params string[] values) 
        => values.Any(x => x.Equals(obj, StringComparison.OrdinalIgnoreCase));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCaseAny(this string obj, IEnumerable<string> values) 
        => values.Any(x => x.Equals(obj, StringComparison.OrdinalIgnoreCase));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAny<T>(this T obj, IEnumerable<T> values) 
        => values.Any(x => x != null && x.Equals(obj));

    public static IEnumerable<K> FindKeysByValue<K, V>(this IDictionary<K, V> dictionary, V value) 
        => from x in dictionary where value.Equals(x.Value) select x.Key;

    public static bool TryGetFirst<K, V>(
        this IDictionary<K, V> dictionary, Func<KeyValuePair<K, V>, bool> predicate, out KeyValuePair<K, V> keyValuePair)
    {
        try
        {
            keyValuePair = dictionary.First(predicate);
            return true;
        }
        catch (Exception)
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
                value = default;
                return false;
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

    public static bool TryGetFirst<TSource>(
        this IEnumerable<TSource?>? source, Func<TSource, bool>? predicate, out TSource? value)
    {
        if (source == null)
        {
            value = default;
            return false;
        }

        if (predicate == null)
        {
            value = default;
            return false;
        }

        foreach (var element in source)
            if (element != null && predicate(element))
            {
                value = element;
                return true;
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
        catch (Exception)
        {
            value = default;
            return false;
        }
    }
}