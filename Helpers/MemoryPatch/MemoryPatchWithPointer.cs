using Dalamud;
using Dalamud.Memory;

namespace OmenTools.Helpers;

public class MemoryPatchWithPointer<T> : MemoryPatch
    where T : unmanaged
{
    public nint PointerAddress { get; }
    public T    OriginalValue { get; private set; }
    public T    CurrentValue  { get; private set; }

    private bool isPatched;

    public void Set(T value)
    {
        if (!isPatched)
        {
            if (!SafeMemory.Read<T>(PointerAddress, out var result)) return;
            
            OriginalValue = result;
            isPatched     = true;
        }

        if (!SafeMemory.Write(PointerAddress, value)) return;
        CurrentValue = value;
    }

    public void Reset()
    {
        if (!isPatched) return;
        if (!SafeMemory.Write(PointerAddress, OriginalValue)) return;
        
        CurrentValue = OriginalValue;
        isPatched    = false;
    }

    public override void Dispose()
    {
        Reset();
        base.Dispose();
    }
    
    public MemoryPatchWithPointer(nint address, IReadOnlyCollection<byte?> bytes, nint pointerOffset = 0, bool startEnabled = false) 
        : base(address, bytes, startEnabled) => 
        PointerAddress = address + pointerOffset;

    public MemoryPatchWithPointer(nint address, string bytesString, nint pointerOffset = 0, bool startEnabled = false) 
        : base(address, bytesString, startEnabled) => 
        PointerAddress = address + pointerOffset;

    public MemoryPatchWithPointer(string sig, IReadOnlyCollection<byte?> bytes, nint scanOffset = 0, nint pointerOffset = 0, bool startEnabled = false) 
        : base(sig, bytes, scanOffset, startEnabled) => 
        PointerAddress = Address + pointerOffset;

    public MemoryPatchWithPointer(string sig, string bytesString, nint scanOffset = 0, nint pointerOffset = 0, bool startEnabled = false) 
        : base(sig, bytesString, scanOffset, startEnabled) => 
        PointerAddress = Address + pointerOffset;
}
