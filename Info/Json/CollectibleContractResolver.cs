using System.Runtime.CompilerServices;
using Newtonsoft.Json.Serialization;
using OmenTools.Info.Json.Converters;

namespace OmenTools.Info.Json;

public sealed class CollectibleContractResolver : DefaultContractResolver
{
    private readonly ConditionalWeakTable<Type, JsonContract> collectibleContracts = new();

    public override JsonContract ResolveContract
    (
        Type type
    ) =>
        type.IsCollectible ?
            collectibleContracts.GetValue(type, CreateContract) :
            base.ResolveContract(type);

    protected override JsonContract CreateContract
    (
        Type objectType
    )
    {
        var type = Nullable.GetUnderlyingType(objectType) ?? objectType;

        if (type is { IsCollectible: true, IsConstructedGenericType: true } && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        {
            return new JsonObjectContract(objectType)
            {
                Converter = new CollectibleKeyValuePairConverter(type)
            };
        }

        return base.CreateContract(objectType);
    }
}
