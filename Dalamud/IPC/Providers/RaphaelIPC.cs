using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OmenTools.Dalamud.Abstractions;
using OmenTools.Dalamud.Attributes;

namespace OmenTools.Dalamud;

public static class RaphaelIPC
{
    public const string INTERNAL_NAME = "Raphael.Dalamud";

    [IPCSubscriber("Raphael.Dalamud.StartCalculation")]
    private static IPCSubscriber<uint> StartCalculationSubscriber;

    [IPCSubscriber("Raphael.Dalamud.StartCalculationWithRecipe")]
    private static IPCSubscriber<uint, uint> StartCalculationWithRecipeSubscriber;

    [IPCSubscriber("Raphael.Dalamud.StartCalculationWithConfig")]
    private static IPCSubscriber<uint, string, uint> StartCalculationWithConfigSubscriber;

    [IPCSubscriber("Raphael.Dalamud.GetCalculationStatus")]
    private static IPCSubscriber<uint, Tuple<uint, string, string, List<uint>>> GetCalculationStatusSubscriber;

    [IPCSubscriber("Raphael.Dalamud.GetCurrentRecipeID")]
    private static IPCSubscriber<uint> GetCurrentRecipeIDSubscriber;

    public static uint GetCurrentRecipeID() =>
        GetCurrentRecipeIDSubscriber.InvokeFunc();

    /// <summary>
    ///     开始计算当前所选配方
    /// </summary>
    /// <returns>请求 ID</returns>
    public static uint StartCalculation() =>
        StartCalculationSubscriber.InvokeFunc();

    public static uint StartCalculation(uint recipeID) =>
        StartCalculationWithRecipeSubscriber.InvokeFunc(recipeID);

    public static uint StartCalculation(uint recipeID, RaphaelCalculationConfig config) =>
        StartCalculationWithConfigSubscriber.InvokeFunc(recipeID, config.ToJSON());

    public static RaphaelCaculationResponse GetCalculationStatus(uint requestID)
    {
        var result = GetCalculationStatusSubscriber.InvokeFunc(requestID);
        return new
        (
            result.Item1,
            Enum.TryParse<RaphaelCalculationStatus>(result.Item2, out var statusResult) ? statusResult : RaphaelCalculationStatus.Failed,
            result.Item3,
            result.Item4
        );
    }
}

public sealed class RaphaelCalculationConfig
{
    public bool? EnsureReliability    { get; init; }
    public bool? BackloadProgress     { get; init; }
    public bool? AllowHeartAndSoul    { get; init; }
    public bool? AllowQuickInnovation { get; init; }
    public int?  MaxThreads           { get; init; }
    public int?  TimeoutSeconds       { get; init; }
    public int?  TargetQuality        { get; init; }
    public int?  InitialQuality       { get; init; }
    public uint? FoodItemID           { get; init; }
    public bool? FoodHQ               { get; init; }
    public uint? PotionItemID         { get; init; }
    public bool? PotionHQ             { get; init; }
    public int?  StellarSteadyHand    { get; init; }

    internal string ToJSON() =>
        JsonConvert.SerializeObject(this, JSONSettings);

    private static readonly JsonSerializerSettings JSONSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver  = new CamelCasePropertyNamesContractResolver()
    };
}

public class RaphaelCaculationResponse
(
    uint                     requestID,
    RaphaelCalculationStatus status,
    string                   error,
    List<uint>               actions
) : IEquatable<RaphaelCaculationResponse>
{
    public uint                     RequestID    { get; init; } = requestID;
    public RaphaelCalculationStatus Status       { get; init; } = status;
    public string                   ErrorMessage { get; init; } = error;
    public List<uint>               Actions      { get; init; } = actions;

    public bool Equals(RaphaelCaculationResponse? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return RequestID == other.RequestID;
    }

    public override bool Equals(object? obj) =>
        Equals(obj as RaphaelCaculationResponse);

    public override int GetHashCode() =>
        (int)RequestID;

    public static bool operator ==(RaphaelCaculationResponse? left, RaphaelCaculationResponse? right) =>
        Equals(left, right);

    public static bool operator !=(RaphaelCaculationResponse? left, RaphaelCaculationResponse? right) =>
        !Equals(left, right);
}

public enum RaphaelCalculationStatus
{
    Idle,
    Calculating,
    Success,
    Failed
}
