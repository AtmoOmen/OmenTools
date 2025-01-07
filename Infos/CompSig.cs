using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Hooking;

namespace OmenTools.Infos;

/// <summary>
/// Composite Signatures 复合签名
/// </summary>
public record CompSig(string Signature, int? Offset = null, string? SignatureCN = null, int? OffsetCN = null)
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

    public int GetOffset() => (OffsetCN != null && IsClientCN ? OffsetCN : Offset) ?? 0;

    public nint ScanText()
    {
        if (!TryGet(out var sig) || string.IsNullOrWhiteSpace(sig)) return nint.Zero;
        return DService.SigScanner.ScanText(sig) + GetOffset();
    }

    public unsafe T* ScanText<T>() where T : unmanaged => (T*)ScanText();

    public nint GetStatic()
    {
        if (!TryGet(out var sig) || string.IsNullOrWhiteSpace(sig)) return nint.Zero;
        return DService.SigScanner.GetStaticAddressFromSig(sig) + GetOffset();
    }

    public unsafe T* GetStatic<T>() where T : unmanaged => (T*)GetStatic();

    public T GetDelegate<T>() where T : Delegate
        => Marshal.GetDelegateForFunctionPointer<T>(ScanText());

    public Hook<T> GetHook<T>(T detour) where T : Delegate
        => DService.Hook.HookFromAddress(ScanText(), detour);
}
