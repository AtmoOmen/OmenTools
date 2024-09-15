using Dalamud.Game;

namespace OmenTools.Infos;

/// <summary>
/// Composite Signatures 复合签名
/// </summary>
public record CompSig(string Signature, string? SignatureCN = null)
{
    public static bool IsClientCN => DService.ClientState.ClientLanguage == (ClientLanguage)4;

    public string? Get() => TryGet(out var sig) ? sig : null;

    public bool TryGet(out string? signature)
    {
        signature = !string.IsNullOrWhiteSpace(Signature) && IsClientCN && !string.IsNullOrWhiteSpace(SignatureCN)
            ? SignatureCN
            : Signature;

        return !string.IsNullOrWhiteSpace(signature);
    }
}
