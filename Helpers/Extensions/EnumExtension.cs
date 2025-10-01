using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OmenTools.Helpers;

public static class EnumExtension
{
    private static readonly ConcurrentDictionary<Enum, string> DescriptionAttributeCache = [];

    public static string GetDescription(this Enum value) => DescriptionAttributeCache.GetOrAdd(value, v =>
    {
        var field     = v.GetType().GetField(v.ToString());
        if (field == null) return v.ToString();
            
        var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute?.Description ?? v.ToString();
    });

    public static bool HasAnyFlag<T>(this T value, params T[] flags) where T : struct, Enum
    {
        if (flags == null || flags.Length == 0) return false;
        
        var   v    = EnumUInt64<T>.ToUInt64(value);
        ulong mask = 0;
        
        foreach (var flag in flags)
            mask |= EnumUInt64<T>.ToUInt64(flag);

        return (v & mask) != 0;
    }

    public static bool HasAllFlag<T>(this T value, params T[] flags) where T : struct, Enum
    {
        if (flags == null || flags.Length == 0) return true;
        
        var   v    = EnumUInt64<T>.ToUInt64(value);
        ulong mask = 0;
        
        foreach (var flag in flags)
            mask |= EnumUInt64<T>.ToUInt64(flag);

        return (v & mask) == mask;
    }
    
    private static class EnumUInt64<T> where T : struct, Enum
    {
        public static readonly Func<T, ulong> ToUInt64 = Create();

        public static Func<T, ulong> Create()
        {
            var t = Enum.GetUnderlyingType(typeof(T));

            if (t == typeof(byte))
                return v => Unsafe.As<T, byte>(ref v);
            if (t == typeof(sbyte))
                return v => (ulong)Unsafe.As<T, sbyte>(ref v);
            if (t == typeof(ushort))
                return v => Unsafe.As<T, ushort>(ref v);
            if (t == typeof(short))
                return v => (ulong)Unsafe.As<T, short>(ref v);
            if (t == typeof(uint))
                return v => Unsafe.As<T, uint>(ref v);
            if (t == typeof(int))
                return v => (ulong)Unsafe.As<T, int>(ref v);
            if (t == typeof(ulong))
                return v => Unsafe.As<T, ulong>(ref v);

            return v => unchecked((ulong)Unsafe.As<T, long>(ref v));
        }
    }
}
