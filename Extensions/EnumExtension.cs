using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OmenTools.Extensions;

public static class EnumExtension
{
    private static readonly ConditionalWeakTable<Type, ConcurrentDictionary<Enum, string>> DescriptionAttributeCache = [];

    /// <summary>
    ///     获取枚举成员上通过 <see cref="DescriptionAttribute" /> 声明的描述文本。
    /// </summary>
    /// <param name="value">要获取描述文本的枚举值。</param>
    /// <returns>枚举成员的描述文本；未声明特性或找不到对应成员时返回其名称。</returns>
    public static string GetDescription
    (
        this Enum value
    ) =>
        DescriptionAttributeCache.GetValue(value.GetType(), static _ => []).GetOrAdd
        (
            value,
            v =>
            {
                var field = v.GetType().GetField(v.ToString());
                if (field == null) return v.ToString();

                var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                return attribute?.Description ?? v.ToString();
            }
        );

    extension<T>
    (
        T value
    ) where T : struct, Enum
    {
        /// <summary>
        ///     判断当前枚举值是否完整包含指定的标志位。
        /// </summary>
        /// <param name="flag">要检查的标志位。</param>
        /// <returns>当前值完整包含该标志位时返回 <see langword="true" />，否则返回 <see langword="false" />。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet
        (
            T flag
        )
        {
            var v = ToUInt64(value);
            var f = ToUInt64(flag);
            return (v & f) == f;
        }

        /// <summary>
        ///     判断当前枚举值是否包含其中任意一个指定标志位。
        /// </summary>
        /// <param name="flags">要检查的候选标志位集合。</param>
        /// <returns>包含任意一个标志位时返回 <see langword="true" />；集合为空时返回 <see langword="false" />。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSetAny
        (
            params ReadOnlySpan<T> flags
        )
        {
            if (flags.IsEmpty) return false;

            var   v    = ToUInt64(value);
            ulong mask = 0;

            foreach (var flag in flags)
                mask |= ToUInt64(flag);

            return (v & mask) != 0;
        }

        /// <summary>
        ///     判断当前枚举值是否完整包含所有指定标志位。
        /// </summary>
        /// <param name="flags">要检查的标志位集合。</param>
        /// <returns>完整包含全部标志位时返回 <see langword="true" />；集合为空时返回 <see langword="true" />。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSetAll
        (
            params ReadOnlySpan<T> flags
        )
        {
            if (flags.IsEmpty) return true;

            var   v    = ToUInt64(value);
            ulong mask = 0;

            foreach (var flag in flags)
                mask |= ToUInt64(flag);

            return (v & mask) == mask;
        }
    }

    extension<T>
    (
        ref T value
    ) where T : struct, Enum
    {
        /// <summary>
        ///     将指定标志位置位，并直接改写调用方持有的枚举变量。
        /// </summary>
        /// <param name="flag">要置位的标志位。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add
        (
            T flag
        ) =>
            value = FromUInt64<T>(ToUInt64(value) | ToUInt64(flag));

        /// <summary>
        ///     将指定的一组标志位同时置位，并直接改写调用方持有的枚举变量。
        /// </summary>
        /// <param name="flags">要置位的标志位集合。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add
        (
            params ReadOnlySpan<T> flags
        )
        {
            if (flags.IsEmpty) return;

            var v = ToUInt64(value);

            foreach (var flag in flags)
                v |= ToUInt64(flag);

            value = FromUInt64<T>(v);
        }

        /// <summary>
        ///     将指定标志位清零，并直接改写调用方持有的枚举变量。
        /// </summary>
        /// <param name="flag">要清零的标志位。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove
        (
            T flag
        ) =>
            value = FromUInt64<T>(ToUInt64(value) & ~ToUInt64(flag));

        /// <summary>
        ///     将指定的一组标志位同时清零，并直接改写调用方持有的枚举变量。
        /// </summary>
        /// <param name="flags">要清零的标志位集合。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove
        (
            params ReadOnlySpan<T> flags
        )
        {
            if (flags.IsEmpty) return;

            var   v    = ToUInt64(value);
            ulong mask = 0;

            foreach (var flag in flags)
                mask |= ToUInt64(flag);

            value = FromUInt64<T>(v & ~mask);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ToUInt64<T>
    (
        T origValue
    ) where T : struct, Enum
    {
        if (Unsafe.SizeOf<T>() == 1)
            return Unsafe.As<T, byte>(ref origValue);

        if (Unsafe.SizeOf<T>() == 2)
            return Unsafe.As<T, ushort>(ref origValue);

        if (Unsafe.SizeOf<T>() == 4)
            return Unsafe.As<T, uint>(ref origValue);

        return Unsafe.SizeOf<T>() == 8 ?
                   Unsafe.As<T, ulong>(ref origValue) :
                   throw new UnreachableException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T FromUInt64<T>
    (
        ulong rawValue
    ) where T : struct, Enum
    {
        if (Unsafe.SizeOf<T>() == 1)
        {
            var result = (byte)rawValue;
            return Unsafe.As<byte, T>(ref result);
        }

        if (Unsafe.SizeOf<T>() == 2)
        {
            var result = (ushort)rawValue;
            return Unsafe.As<ushort, T>(ref result);
        }

        if (Unsafe.SizeOf<T>() == 4)
        {
            var result = (uint)rawValue;
            return Unsafe.As<uint, T>(ref result);
        }

        if (Unsafe.SizeOf<T>() == 8)
        {
            var result = rawValue;
            return Unsafe.As<ulong, T>(ref result);
        }

        throw new UnreachableException();
    }
}
