namespace OmenTools.Dalamud.DataShare.Attributes;

/// <remarks>
///     被标记的数据中只能使用共享类型，任何自声明的类型全部都不可以。
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public class DataShareTagAttribute : Attribute;
