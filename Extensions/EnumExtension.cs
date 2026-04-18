using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OmenTools.Extensions;

public static class EnumExtension
{
    private static readonly ConcurrentDictionary<Enum, string> DescriptionAttributeCache = [];

    public static string GetDescription(this Enum value) =>
        DescriptionAttributeCache.GetOrAdd
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

    extension<T>(T value) where T : struct, Enum
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(T flag)
        {
            var v = ToUInt64(value);
            var f = ToUInt64(flag);
            return (v & f) == f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSetAny(params ReadOnlySpan<T> flags)
        {
            if (flags.IsEmpty) return false;

            var   v    = ToUInt64(value);
            ulong mask = 0;

            foreach (var flag in flags)
                mask |= ToUInt64(flag);

            return (v & mask) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSetAll(params ReadOnlySpan<T> flags)
        {
            if (flags.IsEmpty) return true;

            var   v    = ToUInt64(value);
            ulong mask = 0;

            foreach (var flag in flags)
                mask |= ToUInt64(flag);

            return (v & mask) == mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Add(T flag) =>
            FromUInt64<T>(ToUInt64(value) | ToUInt64(flag));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Add(params ReadOnlySpan<T> flags)
        {
            if (flags.IsEmpty) return value;

            var v = ToUInt64(value);

            foreach (var flag in flags)
                v |= ToUInt64(flag);

            return FromUInt64<T>(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Remove(T flag) =>
            FromUInt64<T>(ToUInt64(value) & ~ToUInt64(flag));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Remove(params ReadOnlySpan<T> flags)
        {
            if (flags.IsEmpty) return value;

            var   v    = ToUInt64(value);
            ulong mask = 0;

            foreach (var flag in flags)
                mask |= ToUInt64(flag);

            return FromUInt64<T>(v & ~mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ToUInt64(T origValue)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return Unsafe.As<T, byte>(ref origValue);

            if (Unsafe.SizeOf<T>() == 2)
                return Unsafe.As<T, ushort>(ref origValue);

            if (Unsafe.SizeOf<T>() == 4)
                return Unsafe.As<T, uint>(ref origValue);

            if (Unsafe.SizeOf<T>() == 8)
                return Unsafe.As<T, ulong>(ref origValue);

            throw new UnreachableException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T FromUInt64(ulong rawValue)
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
}
