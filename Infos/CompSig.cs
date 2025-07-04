using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Hooking;

namespace OmenTools.Infos;

/// <summary>
/// Composite Signatures 复合签名
/// </summary>
public record CompSig
{
    public string Signature { get; init; }

    public CompSig(string signature) => 
        Signature = signature.Trim() ?? throw new ArgumentNullException(nameof(signature));

    public string Get() => Signature;
    
    public nint ScanText() => 
        DService.SigScanner.TryScanText(Signature, out var ptr) ? ptr : nint.Zero;

    public unsafe T* ScanText<T>() where T : unmanaged => 
        (T*)ScanText();

    public nint GetStatic(int offset = 0) => 
        DService.SigScanner.TryGetStaticAddressFromSig(Signature, out var ptr, offset) ? ptr : nint.Zero;

    public unsafe T* GetStatic<T>(int offset = 0) where T : unmanaged => 
        (T*)GetStatic(offset);

    public T GetDelegate<T>() where T : Delegate => 
        Marshal.GetDelegateForFunctionPointer<T>(ScanText());

    public Hook<T> GetHook<T>(T detour) where T : Delegate => 
        DService.Hook.HookFromSignature(Signature, detour);
}
