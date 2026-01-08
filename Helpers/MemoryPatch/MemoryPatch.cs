using System.Globalization;
using Dalamud;

namespace OmenTools.Helpers;

public class MemoryPatch : IDisposable
{
    public nint   Address   { get; }
    public string Signature { get; } = null!;
    public byte[] NewBytes  { get; } = null!;
    public byte[] OldBytes  { get; } = null!;
    public bool   IsEnabled { get; private set; }

    public bool IsValid => Address != nint.Zero;

    private static readonly List<MemoryPatch> MemoryPatches = [];

    public MemoryPatch(nint address, IReadOnlyCollection<byte?> bytes, bool startEnabled = false)
    {
        if (address == nint.Zero) return;

        var (trimmedBytes, skip) = TrimBytes(bytes);
        Address = address + skip;
        SafeMemory.ReadBytes(Address, trimmedBytes.Length, out var oldBytes);
        OldBytes = oldBytes;
        NewBytes = MergeBytes(trimmedBytes, OldBytes);

        MemoryPatches.Add(this);
        if (startEnabled) 
            Enable();
    }

    public MemoryPatch(nint address, string bytesString, bool startEnabled = false) : 
        this(address, ParseByteString(bytesString), startEnabled) 
    { }

    public MemoryPatch(string sig, IReadOnlyCollection<byte?> bytes, nint offset = 0, bool startEnabled = false) :
        this(Scan(sig) + offset, bytes, startEnabled) =>
        Signature = sig;

    public MemoryPatch(string sig, string bytesString, nint offset = 0, bool startEnabled = false) : 
        this(sig, ParseByteString(bytesString), offset, startEnabled)
    { }

    public void Enable()
    {
        if (IsEnabled || !IsValid) return;
        SafeMemory.WriteBytes(Address, NewBytes);
        IsEnabled = true;
    }

    public void Disable()
    {
        if (!IsEnabled || !IsValid) return;
        SafeMemory.WriteBytes(Address, OldBytes);
        IsEnabled = false;
    }

    public void Toggle() => 
        Set(!IsEnabled);

    public void Set(bool enable) => 
        (enable ? (Action)Enable : Disable)();
    
    public virtual void Dispose()
    {
        Disable();
        GC.SuppressFinalize(this);
    }

    internal static void DisposeAll() => 
        MemoryPatches.ForEach(patch => patch.Dispose());

    #region 私有

    private static nint Scan(string sig)
    {
        if (DService.Instance().SigScanner.TryScanModule(sig, out var address))
            return address;

        DService.Instance().Log.Warning($"未能为 MemoryPatch 搜寻到签名 {sig}");
        return nint.Zero;
    }

    private static byte?[] ParseByteString(string bytesString) =>
        bytesString.Replace(" ", "")
                   .Chunk(2)
                   .Select(chunk => chunk.SequenceEqual("??") || chunk.SequenceEqual("**")
                                        ? (byte?)null
                                        : byte.Parse(new string(chunk), NumberStyles.HexNumber))
                   .ToArray();

    private static (byte?[] TrimmedBytes, int Skip) TrimBytes(IReadOnlyCollection<byte?> bytes)
    {
        var trimmedBytes = bytes.SkipWhile(b => !b.HasValue).ToArray();
        return (trimmedBytes, bytes.Count - trimmedBytes.Length);
    }

    private static byte[] MergeBytes(byte?[] newBytes, byte[] oldBytes) =>
        newBytes.Select((b, i) => b ?? oldBytes[i]).ToArray();

    #endregion
}
