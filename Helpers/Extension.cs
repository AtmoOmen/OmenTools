using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.System.String;
using SeString = Lumina.Text.SeString;
using Timer = System.Timers.Timer;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using Dalamud.Game.ClientState.Party;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using Lumina.Excel.Sheets;
using Aetheryte = Lumina.Excel.Sheets.Aetheryte;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    private static readonly CompareInfo    s_compareInfo    = CultureInfo.InvariantCulture.CompareInfo;
    private const           CompareOptions s_compareOptions = CompareOptions.IgnoreCase;

    public const BindingFlags AllFlags      = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    public const BindingFlags StaticFlags   = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    public const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly ConcurrentDictionary<Enum, string> DescriptionAttributeCache = [];

    public static string GetTexturePath(this Map map) 
    {
        var mapKey = map.Id.ExtractText();
        var rawKey = mapKey.Replace("/", "");
        return $"ui/map/{mapKey}/{rawKey}_m.tex";
    }
    
    public static string GetDescription(this Enum value) =>
        DescriptionAttributeCache.GetOrAdd(value, v =>
        {
            var field     = v.GetType().GetField(v.ToString());
            if (field == null) return v.ToString();
            
            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? v.ToString();
        });
    
    public static char ToSEChar(this uint integer) =>
        integer switch
        {
            1 => '\ue0b1',
            2 => '\ue0b2',
            3 => '\ue0b3',
            4 => '\ue0b4',
            5 => '\ue0b5',
            6 => '\ue0b6',
            7 => '\ue0b7',
            8 => '\ue0b8',
            9 => '\ue0b9',
            _ => char.MinValue,
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFullWidth(this char c) => c is >= '\uFF01' and <= '\uFF5E';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToHalfWidth(this char c) => (char)(c - 0xFEE0);
    
    public static string ToLowerAndHalfWidth(this string input)
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
    
    public static object? GetFoP(this object obj, string name) =>
        obj.GetType().GetField(name, AllFlags)?.GetValue(obj)
        ?? obj.GetType().GetProperty(name, AllFlags)?.GetValue(obj);

    public static T? GetFoP<T>(this object obj, string name) 
        => (T?)GetFoP(obj, name);

    public static void SetFoP(this object obj, string name, object value)
    {
        var field = obj.GetType().GetField(name, AllFlags);
        if (field != null)
            field.SetValue(obj, value);
        else
            obj.GetType().GetProperty(name, AllFlags)?.SetValue(obj, value);
    }

    public static object? GetStaticFoP(this object obj, string type, string name) =>
        obj.GetType().Assembly.GetType(type)?.GetField(name, StaticFlags)?.GetValue(null)
        ?? obj.GetType().Assembly.GetType(type)?.GetProperty(name, StaticFlags)?.GetValue(null);

    public static T? GetStaticFoP<T>(this object obj, string type, string name) 
        => (T?)GetStaticFoP(obj, type, name);

    public static void SetStaticFoP(this object obj, string type, string name, object value)
    {
        var field = obj.GetType().Assembly.GetType(type)?.GetField(name, StaticFlags);
        if (field != null)
            field.SetValue(null, value);
        else
            obj.GetType().Assembly.GetType(type)?.GetProperty(name, StaticFlags)?.SetValue(null, value);
    }

    /// <returns>Object returned by the target method</returns>
    public static object? Call(this object obj, string name, object[] @params, bool matchExactArgumentTypes = false)
    {
        var info = !matchExactArgumentTypes
                       ? obj.GetType().GetMethod(name, AllFlags)
                       : obj.GetType().GetMethod(name, AllFlags, @params.Select(x => x.GetType()).ToArray());
        return info?.Invoke(obj, @params);
    }

    public static T? Call<T>(this object obj, string name, object[] @params, bool matchExactArgumentTypes = false) 
        => (T?)Call(obj, name, @params, matchExactArgumentTypes);

    public static Vector2 GetPositionWorld(this Aetheryte aetheryte)
    {
        var mapRow = aetheryte.Territory.ValueNullable?.Map.ValueNullable;
        if (mapRow == null) return new();

        return MapToWorld(GetPositionMap(aetheryte), (Map)mapRow);
    }

    public static Vector2 GetPositionMap(this Aetheryte aetheryte)
    {
        var mapRow = aetheryte.Territory.ValueNullable?.Map.ValueNullable;
        if (mapRow == null) return new();

        var mapRowNotNull = (Map)mapRow;

        var result = LuminaGetter.GetSub<MapMarker>()
            .SelectMany(x => x)
            .Where(x => x.DataType == 3 && x.RowId == mapRowNotNull.MapMarkerRange && x.DataKey.RowId == aetheryte.RowId)
            .Select(x => TextureToMap(x.X, x.Y, mapRowNotNull.SizeFactor))
            .FirstOrDefault();

        return result;
    }

    public static PartyMember* ToStruct(this IPartyMember partyMember) => (PartyMember*)partyMember.Address;

    public static ConcurrentBag<T> ToConcurrentBag<T>(this IEnumerable<T> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        return new ConcurrentBag<T>(source);
    }

    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector) where TKey : notnull
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));

        return new ConcurrentDictionary<TKey, TValue>(
            source.ToDictionary(keySelector, valueSelector));
    }

    public static ConcurrentDictionary<TKey, TSource> ToConcurrentDictionary<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) where TKey : notnull
    {
        return source.ToConcurrentDictionary(keySelector, item => item);
    }

    public static void AddRange<T>(this ConcurrentBag<T> bag, IEnumerable<T> items)
    {
        if (bag == null) throw new ArgumentNullException(nameof(bag));
        if (items == null) throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
            bag.Add(item);
    }

    public static void ForEach<T>(this ConcurrentBag<T> bag, Action<T> action)
    {
        if (bag == null) throw new ArgumentNullException(nameof(bag));
        if (action == null) throw new ArgumentNullException(nameof(action));

        foreach (var item in bag)
            action(item);
    }

    public static void AddRange<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items) 
        where TKey : notnull
    {
        if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
        if (items == null) throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
            dictionary.TryAdd(item.Key, item.Value);
    }

    public static void ForEach<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, Action<TKey, TValue> action) where TKey : notnull
    {
        if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
        if (action == null) throw new ArgumentNullException(nameof(action));

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

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items) 
            collection.Add(item);
    }

    public static TValue GetOrAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
    {
        if(dictionary.TryGetValue(key, out var value)) return value;
        value = valueFactory(key);
        dictionary[key] = value;
        return value;
    }

    public static List<nint> SearchSimpleNodesByType(this AtkUldManager manager, NodeType type)
    {
        var result = new List<nint>();
        for (var i = 0; i < manager.NodeListCount; i++)
        {
            var node = manager.NodeList[i];
            // 非 SimpleNode
            if (node == null || (int)node->Type > 1000) continue;
            if (node->Type != type) continue;

            result.Add((nint)node);
        }

        return result;
    }

    public static T* SearchSimpleNodeByType<T>(this AtkUldManager manager, NodeType type) where T : unmanaged
    {
        for (var i = 0; i < manager.NodeListCount; i++)
        {
            var node = manager.NodeList[i];
            // 非 SimpleNode
            if (node == null || (int)node->Type > 1000) continue;
            if (node->Type != type) continue;

            return (T*)node;
        }

        return null;
    }

    public static nint SearchSimpleNodeByType(this AtkUldManager manager, NodeType type)
    {
        for (var i = 0; i < manager.NodeListCount; i++)
        {
            var node = manager.NodeList[i];
            // 非 SimpleNode
            if (node == null || (int)node->Type > 1000) continue;
            if (node->Type != type) continue;

            return (nint)node;
        }

        return nint.Zero;
    }

    public static List<nint> SearchComponentNodesByType(this AtkUldManager manager, ComponentType type)
    {
        var result = new List<nint>();
        for (var i = 0; i < manager.NodeListCount; i++)
        {
            var node = manager.NodeList[i];
            // 非 ComponentNode
            if (node == null || (int)node->Type < 1000) continue;

            var componentNode = (AtkComponentNode*)node;
            var componentInfo = componentNode->Component->UldManager;
            var objectInfo = (AtkUldComponentInfo*)componentInfo.Objects;
            if (objectInfo == null || objectInfo->ComponentType != type) continue;

            result.Add((nint)componentNode->Component);
        }

        return result;
    }

    public static T* SearchComponentNodeByType<T>(this AtkUldManager manager, ComponentType type) where T : unmanaged
    {
        for (var i = 0; i < manager.NodeListCount; i++)
        {
            var node = manager.NodeList[i];
            // 非 ComponentNode
            if (node == null || (int)node->Type < 1000) continue;

            var componentNode = (AtkComponentNode*)node;
            var componentInfo = componentNode->Component->UldManager;
            var objectInfo = (AtkUldComponentInfo*)componentInfo.Objects;
            if (objectInfo == null || objectInfo->ComponentType != type) continue;

            return (T*)componentNode->Component;
        }

        return null;
    }

    public static nint SearchComponentNodeByType(this AtkUldManager manager, ComponentType type)
    {
        for (var i = 0; i < manager.NodeListCount; i++)
        {
            var node = manager.NodeList[i];
            // 非 ComponentNode
            if (node == null || (int)node->Type < 1000) continue;

            var componentNode = (AtkComponentNode*)node;
            var componentInfo = componentNode->Component->UldManager;
            var objectInfo = (AtkUldComponentInfo*)componentInfo.Objects;
            if (objectInfo == null || objectInfo->ComponentType != type) continue;

            return (nint)componentNode->Component;
        }

        return nint.Zero;
    }

    public static AtkUnitBase* ToAtkUnitBase(this nint ptr) => (AtkUnitBase*)ptr;

    public static void ClickAddonRadioButton(this AtkComponentRadioButton target, AtkUnitBase* addon, uint which, EventType type = EventType.CHANGE)
        => ClickAddonComponent(addon, (&target)->OwnerNode, which, type);

    public static void ClickAddonCheckBox(this AtkComponentCheckBox target, AtkUnitBase* addon, uint which, EventType type = EventType.CHANGE)
        => ClickAddonComponent(addon, (&target)->AtkComponentButton.AtkComponentBase.OwnerNode, which, type);

    public static void ClickAddonDragDrop(this AtkComponentDragDrop target, AtkUnitBase* addon, uint which, EventType type = EventType.ICON_TEXT_ROLL_OUT)
        => ClickAddonComponent(addon, (&target)->AtkComponentBase.OwnerNode, which, type);

    public static void ClickAddonButton(this AtkComponentButton target, AtkUnitBase* addon, AtkEvent* eventData)
        => Listener!.Invoke((nint)addon, eventData->State.EventType, eventData->Param, eventData);

    public static void ClickAddonButton(this AtkCollisionNode target, AtkUnitBase* addon, AtkEvent* eventData)
        => Listener!.Invoke((nint)addon, eventData->State.EventType, eventData->Param, eventData);

    public static void ClickAddonButton(this AtkComponentButton target, AtkUnitBase* addon)
    {
        var btnRes = target.AtkComponentBase.OwnerNode->AtkResNode;
        var evt = btnRes.AtkEventManager.Event;

        addon->ReceiveEvent(evt->State.EventType, (int)evt->Param, btnRes.AtkEventManager.Event);
    }

    public static void ClickAddonButton(this AtkCollisionNode target, AtkUnitBase* addon)
    {
        var btnRes = target.AtkResNode;
        var evt = btnRes.AtkEventManager.Event;

        while (evt->State.EventType != AtkEventType.MouseClick)
            evt = evt->NextEvent;

        addon->ReceiveEvent(evt->State.EventType, (int)evt->Param, btnRes.AtkEventManager.Event);
    }

    public static void ClickRadioButton(this AtkComponentRadioButton target, AtkUnitBase* addon)
    {
        var btnRes = target.OwnerNode->AtkResNode;
        var evt = btnRes.AtkEventManager.Event;

        addon->ReceiveEvent(evt->State.EventType, (int)evt->Param, btnRes.AtkEventManager.Event);
    }

    public static List<MapMarker> GetMapMarkers(this Map map) =>
        LuminaGetter.GetSub<MapMarker>()
            .SelectMany(x => x)
            .Where(x => x.RowId == map.MapMarkerRange)
            .ToList();

    private static string GetMarkerPlaceName(this MapMarker marker)
    {
        var placeName = marker.GetMarkerLabel();
        if (placeName != string.Empty) return placeName;

        if (!LuminaGetter.TryGetRow<MapSymbol>(marker.Icon, out var symbol)) return string.Empty;
        return symbol.PlaceName.ValueNullable?.Name.ExtractText() ?? string.Empty;
    }

    public static string GetMarkerLabel(this MapMarker marker)
        => marker.PlaceNameSubtext.ValueNullable?.Name.ExtractText() ?? string.Empty;

    public static Vector2 GetPosition(this MapMarker marker) => new(marker.X, marker.Y);

    public static Vector3 ToVector3(this Vector2 vector2) 
        => vector2.ToVector3(DService.ObjectTable.LocalPlayer?.Position.Y ?? 0);
    
    public static bool TargetInteract(this IGameObject? gameObject)
    {
        if (gameObject == null) return false;
        TargetSystem.Instance()->Target = gameObject.ToStruct();
        return TargetSystem.Instance()->InteractWithObject(gameObject.ToStruct()) != 0;
    }
    
    public static void SaveToBinaryFile(this WebResponse response, string filePath)
    {
        var       buffer = new byte[1024];
        using var rs     = response.GetResponseStream();
        using var fileStream = new FileStream(
            filePath,
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.ReadWrite
        );

        while (true)
        {
            var count = rs.Read(buffer, 0, buffer.Length);
            if (count <= 0)
            {
                break;
            }

            fileStream.Write(buffer, 0, count);
        }
    }

    public static bool Interact(this IGameObject? gameObject) 
        => gameObject != null && TargetSystem.Instance()->InteractWithObject(gameObject.ToStruct()) != 0;

    public static IGameObject? FindNearest(this IEnumerable<IGameObject> gameObjects, 
        Vector3                                                          source,
        Func<IGameObject, bool>                                          predicate) =>
        gameObjects.Where(predicate).MinBy(x => Vector3.Distance(source, x.Position));

    public static void Toggle<T>(this Hook<T>? hook, bool? isEnabled = null) where T : Delegate
    {
        if (hook == null || hook.IsDisposed) return;

        if (isEnabled == null)
        {
            if (hook.IsEnabled) hook.Disable();
            else hook.Enable();
        }
        else
        {
            if (isEnabled.Value) hook.Enable();
            else hook.Disable();
        }
    }
    
    public static bool TryReplaceIgnoreCase(this string origText, string input, string replacement, out string? result)
    {
        result = null;
        if (string.IsNullOrEmpty(origText) || string.IsNullOrEmpty(input))
            return false;

        var index = s_compareInfo.IndexOf(origText, input, s_compareOptions);
        if (index == -1)
            return false;

        result = ReplaceAll(origText, input, replacement);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ReplaceAll(string origText, string input, string replacement)
    {
        var inputLength = input.Length;
        var replacementLength = replacement.Length;
        var capacityMultiplier = Math.Max(1, replacementLength / inputLength);
        
        char[]? rentedArray = null;
        var buffer = origText.Length <= 256
            ? stackalloc char[256]
            : (rentedArray = ArrayPool<char>.Shared.Rent(origText.Length * capacityMultiplier));

        try
        {
            var writePos = 0;
            var startIndex = 0;
            while (true)
            {
                var index = s_compareInfo.IndexOf(origText, input, startIndex, origText.Length - startIndex, s_compareOptions);
                if (index == -1)
                {
                    origText.AsSpan(startIndex).CopyTo(buffer.Slice(writePos));
                    writePos += origText.Length - startIndex;
                    break;
                }

                var count = index - startIndex;
                origText.AsSpan(startIndex, count).CopyTo(buffer.Slice(writePos));
                writePos += count;

                replacement.AsSpan().CopyTo(buffer.Slice(writePos));
                writePos += replacementLength;

                startIndex = index + inputLength;
            }

            return new string(buffer[..writePos]);
        }
        finally
        {
            if (rentedArray != null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
    }
    
    public static bool TryReplaceIgnoreCase(this ReadOnlySpan<char> origText, ReadOnlySpan<char> input, ReadOnlySpan<char> replacement, out string? result)
    {
        result = null;
        if (origText.IsEmpty || input.IsEmpty)
            return false;

        var index = s_compareInfo.IndexOf(origText, input, s_compareOptions);
        if (index == -1)
            return false;

        result = ReplaceAll(origText, input, replacement);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ReplaceAll(ReadOnlySpan<char> origText, ReadOnlySpan<char> input, ReadOnlySpan<char> replacement)
    {
        var inputLength        = input.Length;
        var replacementLength  = replacement.Length;
        var capacityMultiplier = Math.Max(1, replacementLength / inputLength);
    
        char[]? rentedArray = null;
        Span<char> buffer = origText.Length <= 256
            ? stackalloc char[256]
            : (rentedArray = ArrayPool<char>.Shared.Rent(origText.Length * capacityMultiplier));

        try
        {
            var writePos   = 0;
            var startIndex = 0;
            while (true)
            {
                var remainingText = origText.Slice(startIndex);
                var index         = s_compareInfo.IndexOf(remainingText, input, s_compareOptions);
                if (index == -1)
                {
                    remainingText.CopyTo(buffer.Slice(writePos));
                    writePos += remainingText.Length;
                    break;
                }

                origText.Slice(startIndex, index).CopyTo(buffer.Slice(writePos));
                writePos += index;

                replacement.CopyTo(buffer.Slice(writePos));
                writePos += replacementLength;

                startIndex += index + inputLength;
            }

            return new string(buffer.Slice(0, writePos));
        }
        finally
        {
            if (rentedArray != null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    public static BitmapFontIcon ToBitmapFontIcon(this ClassJob? job)
    {
        if (job == null || job.Value.RowId == 0) return BitmapFontIcon.NewAdventurer;

        return job.Value.RowId switch
        {
            < 1 => BitmapFontIcon.NewAdventurer,
            < 41 => (BitmapFontIcon)job.Value.RowId + 127,
            41 or 42 => (BitmapFontIcon)job.Value.RowId + 129,
            _ => BitmapFontIcon.NewAdventurer
        };
    }

    public static BitmapFontIcon ToBitmapFontIcon(this ClassJob job)
    {
        if (job.RowId == 0) return BitmapFontIcon.NewAdventurer;

        return job.RowId switch
        {
            < 1 => BitmapFontIcon.NewAdventurer,
            < 41 => (BitmapFontIcon)job.RowId + 127,
            41 or 42 => (BitmapFontIcon)job.RowId + 129,
            _ => BitmapFontIcon.NewAdventurer
        };
    }

    public static string ExtractPlaceName(this TerritoryType row)
        => row.PlaceName.ValueNullable?.Name.ExtractText() ?? string.Empty;

    public static Vector2 ToVector2(this Vector3 vector3)
        => new(vector3.X, vector3.Z);

    public static Vector3 ToVector3(this Vector2 vector2, float Y)
        => new(vector2.X, Y, vector2.Y);

    public static Vector3 ToPosition(this Level level) => new(level.X, level.Y, level.Z);

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

    public static V? GetOrDefault<K, V>(this IDictionary<K, V> dic, K key) =>
        dic.TryGetValue(key, out var value) ? value : default;

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
        => from x in dictionary where value != null && value.Equals(x.Value) select x.Key;

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

    public static void Swap<T>(this IList<T> list, int index1, int index2)
    {
        if (index1 < 0 || index1 >= list.Count || index2 < 0 || index2 >= list.Count)
        {
            throw new IndexOutOfRangeException("无法交换元素，因为其中一个索引无效");
        }
        (list[index1], list[index2]) = (list[index2], list[index1]);
    }
}
