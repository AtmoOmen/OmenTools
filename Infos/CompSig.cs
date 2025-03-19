using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Hooking;

namespace OmenTools.Infos;

/// <summary>
/// Composite Signatures 复合签名
/// </summary>
public record CompSig(string Signature, string? SignatureCN = null)
{
    public nint Address { get; private set; }

    public static bool IsClientCN => DService.ClientState.ClientLanguage == (ClientLanguage)4;

    public string? Get() => TryGet(out var sig) ? sig : null;

    public bool TryGet(out string? signature)
    {
        signature = IsClientCN && !string.IsNullOrWhiteSpace(SignatureCN) ? SignatureCN : Signature;
        return !string.IsNullOrWhiteSpace(signature);
    }

    private bool TryGetValidSignature(out string sig)
        => TryGet(out sig!) && !string.IsNullOrWhiteSpace(sig);

    public nint ScanText()
    {
        if (TryGetValidSignature(out var sig) && DService.SigScanner.TryScanText(sig, out var address))
            Address = address;
        
        return Address;
    }

    public unsafe T* ScanText<T>() where T : unmanaged
    {
        if (TryGetValidSignature(out var sig) && DService.SigScanner.TryScanText(sig, out var address))
            Address = address;
        
        return (T*)Address;
    }

    public nint GetStatic(int offset = 0)
        => TryGetValidSignature(out var sig) ? DService.SigScanner.GetStaticAddressFromSig(sig, offset) : nint.Zero;

    public unsafe T* GetStatic<T>(int offset = 0) where T : unmanaged
        => TryGetValidSignature(out var sig) ? (T*)DService.SigScanner.GetStaticAddressFromSig(sig, offset) : null;

    public T GetDelegate<T>() where T : Delegate
        => Marshal.GetDelegateForFunctionPointer<T>(ScanText());

    public Hook<T> GetHook<T>(T detour) where T : Delegate
        => DService.Hook.HookFromSignature(Get() ?? string.Empty, detour);
}
