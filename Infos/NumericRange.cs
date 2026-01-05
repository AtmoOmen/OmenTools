using System.Numerics;
using System.Runtime.CompilerServices;

namespace OmenTools.Infos;

public readonly record struct NumericRange<T> where T : INumber<T>
{
    public NumericRange(T min, T max)
    {
        if (min >= max)
            throw new ArgumentOutOfRangeException(nameof(min), $"最小值 ({nameof(min)}) 必须小于等于最大值 ({nameof(max)})");
        
        this.Min = min;
        this.Max = max;
    }

    public T Min { get; init; }
    public T Max { get; init; }

    public void Deconstruct(out T min, out T max)
    {
        min = this.Min;
        max = this.Max;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T? value) => 
        value >= Min && value <= Max;
}

public static class NumericRange
{
    public static NumericRange<ushort> UShortRange { get; } = new(ushort.MinValue, ushort.MaxValue);
    public static NumericRange<short>  ShortRange  { get; } = new(short.MinValue, short.MaxValue);
}
