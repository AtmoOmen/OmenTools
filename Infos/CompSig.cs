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

    public nint ScanModule()
    {
        if (!TryGet(out var sig) || string.IsNullOrWhiteSpace(sig)) return nint.Zero;
        return DService.SigScanner.ScanModule(sig);
    }

    public unsafe T* ScanModule<T>() where T : unmanaged
    {
        if (!TryGet(out var sig) || string.IsNullOrWhiteSpace(sig)) return null;
        return (T*)DService.SigScanner.ScanModule(sig);
    }

    public nint GetStatic()
    {
        if (!TryGet(out var sig) || string.IsNullOrWhiteSpace(sig)) return nint.Zero;
        return DService.SigScanner.GetStaticAddressFromSig(sig);
    }

    public unsafe T* GetStatic<T>() where T : unmanaged
    {
        if (!TryGet(out var sig) || string.IsNullOrWhiteSpace(sig)) return null;
        return (T*)DService.SigScanner.GetStaticAddressFromSig(sig);
    }
}
