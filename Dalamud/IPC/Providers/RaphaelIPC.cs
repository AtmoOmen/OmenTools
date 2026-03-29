using OmenTools.Dalamud.Abstractions;
using OmenTools.Dalamud.Attributes;

namespace OmenTools.Dalamud;

public static class RaphaelIPC
{
    public const string InternalName = "Raphael.Dalamud";

    [IPCSubscriber("Raphael.Dalamud.StartCalculation")]
    private static IPCSubscriber<uint> StartCalculationSubscriber;

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
