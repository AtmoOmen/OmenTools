using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 0x2B0)]
public struct CameraEx
{
    [FieldOffset(304)]
    public float DirH; // 0 is north, increases CW

    [FieldOffset(308)]
    public float DirV; // 0 is horizontal, positive is looking up, negative looking down

    [FieldOffset(312)]
    public float InputDeltaHAdjusted;

    [FieldOffset(316)]
    public float InputDeltaVAdjusted;

    [FieldOffset(320)]
    public float InputDeltaH;

    [FieldOffset(324)]
    public float InputDeltaV;

    [FieldOffset(328)]
    public float DirVMin; // -85deg by default

    [FieldOffset(332)]
    public float DirVMax; // +45deg by default
}
