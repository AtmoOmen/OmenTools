using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 0x2C0)]
public struct CameraEx
{
    [FieldOffset(320)]
    public float DirH; // 0 is north, increases CW

    [FieldOffset(324)]
    public float DirV; // 0 is horizontal, positive is looking up, negative looking down

    [FieldOffset(328)]
    public float InputDeltaHAdjusted;

    [FieldOffset(316 + 0x10)]
    public float InputDeltaVAdjusted;

    [FieldOffset(336)]
    public float InputDeltaH;

    [FieldOffset(340)]
    public float InputDeltaV;

    [FieldOffset(344)]
    public float DirVMin; // -85deg by default

    [FieldOffset(348)]
    public float DirVMax; // +45deg by default
}
