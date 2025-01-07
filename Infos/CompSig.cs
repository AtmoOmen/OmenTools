﻿using System.Runtime.InteropServices;
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
        signature = IsClientCN && !string.IsNullOrWhiteSpace(SignatureCN) ? SignatureCN : Signature;
        return !string.IsNullOrWhiteSpace(signature);
    }

    private bool TryGetValidSignature(out string sig)
        => TryGet(out sig!) && !string.IsNullOrWhiteSpace(sig);

    private int GetOffset() => IsClientCN && OffsetCN.HasValue ? OffsetCN.Value : Offset ?? 0;

    public nint ScanText()
        => TryGetValidSignature(out var sig) 
               ? DService.SigScanner.ScanText(sig) + GetOffset() 
               : nint.Zero;

    public unsafe T* ScanText<T>() where T : unmanaged
        => (T*)ScanText();

    public nint GetStatic()
        => TryGetValidSignature(out var sig)
               ? DService.SigScanner.GetStaticAddressFromSig(sig) + GetOffset()
               : nint.Zero;

    public unsafe T* GetStatic<T>() where T : unmanaged
        => (T*)GetStatic();

    public T GetDelegate<T>() where T : Delegate
        => Marshal.GetDelegateForFunctionPointer<T>(ScanText());

    public Hook<T> GetHook<T>(T detour) where T : Delegate
        => DService.Hook.HookFromSignature(Get() ?? string.Empty, detour);
}
