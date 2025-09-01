using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Layer;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit, Size = 256)]
public unsafe struct PopRangeManager
{
    [FieldOffset(0)]
    public int State;

    [FieldOffset(4)]
    public float Time;

    // Set if LocalPlayer is inside ExitRangeLayoutInstance's collider
    [FieldOffset(16)]
    public ExitRangeLayoutInstance* ExitLayoutInstance;

    [FieldOffset(32)]
    public Vector3 Position;

    // When check position failure, recovered position
    [FieldOffset(48)]
    public Vector3 RecoveredPosition;

    private static readonly CompSig          PopRangManagerSig = new("83 3D ?? ?? ?? ?? ?? 77 ??");
    private static          PopRangeManager* PopRangeManagerPtr;

    public static PopRangeManager* Instance()
    {
        if (PopRangeManagerPtr != null)
            return PopRangeManagerPtr;

        PopRangeManagerPtr = PopRangManagerSig.GetStatic<PopRangeManager>();
        return PopRangeManagerPtr;
    }

    public void PopRange(ILayoutInstance* exit)
    {
        ExitLayoutInstance = (ExitRangeLayoutInstance*)exit;
        var pop = ExitLayoutInstance->PopRangeLayoutInstance;
        
        Position = *pop->Base.GetTranslationImpl();
        var recovered = Position + *pop->AddPos;
        
        RecoveredPosition = recovered;
        State             = 2;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 160)]
public unsafe struct ExitRangeLayoutInstance
{
    [FieldOffset(0x0)]
    public TriggerBoxLayoutInstance Base;

    [FieldOffset(134)]
    public ushort TerritoryType;

    [FieldOffset(144)]
    public uint PopRangeLayoutInstanceId;

    public PopRangeLayoutInstance* PopRangeLayoutInstance => 
        (PopRangeLayoutInstance*)LayoutWorld.GetLayoutInstance(InstanceType.PopRange, PopRangeLayoutInstanceId);
}

[StructLayout(LayoutKind.Explicit, Size = 160)]
public unsafe struct PopRangeLayoutInstance
{
    [FieldOffset(0)]
    public TriggerBoxLayoutInstance Base;

    [FieldOffset(128)]
    public Vector3* AddPos;
}
