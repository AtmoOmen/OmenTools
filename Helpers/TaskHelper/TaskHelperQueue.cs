namespace OmenTools.Helpers;

public class TaskHelperQueue(uint weight) : IEquatable<TaskHelperQueue>, IComparable<TaskHelperQueue>
{
    public uint                 Weight { get; } = weight;
    public List<TaskHelperTask> Tasks  { get; } = [];

    public bool Equals(TaskHelperQueue? other) => 
        other is not null && Weight == other.Weight;

    public override bool Equals(object? obj) => 
        obj is TaskHelperQueue other && Equals(other);

    public override int GetHashCode() => 
        (int)Weight;

    public int CompareTo(TaskHelperQueue? other) => 
        other is null ? 1 : other.Weight.CompareTo(Weight);

    public static bool operator ==(TaskHelperQueue? left, TaskHelperQueue? right) => 
        Equals(left, right);

    public static bool operator !=(TaskHelperQueue? left, TaskHelperQueue? right) => 
        !Equals(left, right);

    public static bool operator <(TaskHelperQueue left, TaskHelperQueue right) => 
        left.CompareTo(right) < 0;

    public static bool operator <=(TaskHelperQueue left, TaskHelperQueue right) => 
        left.CompareTo(right) <= 0;

    public static bool operator >(TaskHelperQueue left, TaskHelperQueue right) => 
        left.CompareTo(right) > 0;

    public static bool operator >=(TaskHelperQueue left, TaskHelperQueue right) => 
        left.CompareTo(right) >= 0;
}
