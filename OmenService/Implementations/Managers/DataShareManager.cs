using System.Reflection;
using OmenTools.Dalamud;
using OmenTools.Dalamud.DataShare.Attributes;
using OmenTools.OmenService.Abstractions;

namespace OmenTools.OmenService;

public sealed class DataShareManager : OmenServiceBase<DataShareManager>
{
    private string[] dataShareTags = [];

    protected override void Init() =>
        dataShareTags = DiscoverDataShareTags();

    protected override void Uninit()
    {
        foreach (var tag in dataShareTags)
            DService.Instance().PI.RelinquishData(tag);

        dataShareTags = [];
    }

    private static string[] DiscoverDataShareTags()
    {
        HashSet<string> discoveredTags = new(StringComparer.Ordinal);

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                if (!field.IsDefined(typeof(DataShareTagAttribute), false))
                    continue;

                ValidateDataShareTagField(field);

                discoveredTags.Add
                (
                    field.GetRawConstantValue() as string ??
                    throw new InvalidOperationException($"字段 {GetFieldDisplayName(field)} 的 DataShare 标签值不能为空")
                );
            }
        }

        return [.. discoveredTags.Order(StringComparer.Ordinal)];
    }

    private static void ValidateDataShareTagField(FieldInfo field)
    {
        if (!field.IsLiteral || field.IsInitOnly)
            throw new InvalidOperationException($"字段 {GetFieldDisplayName(field)} 标记了 [DataShareTag]，但它不是 const string");

        if (field.FieldType != typeof(string))
            throw new InvalidOperationException($"字段 {GetFieldDisplayName(field)} 标记了 [DataShareTag]，但它不是 const string");
    }

    private static string GetFieldDisplayName(FieldInfo field) =>
        $"{field.DeclaringType?.FullName ?? "<未知类型>"}.{field.Name}";
}
