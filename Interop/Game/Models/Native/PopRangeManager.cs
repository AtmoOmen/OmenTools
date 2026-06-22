using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Layer;
using OmenTools.OmenService;

namespace OmenTools.Interop.Game.Models.Native;

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

    public void PopRange(ILayoutInstance* exit, Vector3? recoveredPosition = null)
    {
        var pop = ((ExitRangeLayoutInstance*)exit)->ReturnInstance;

        if (recoveredPosition == null)
        {
            Position          = LocalPlayerState.Object?.Position ?? default;
            RecoveredPosition = *pop->Base.GetTranslationImpl();
        }
        else
        {
            Position          = new(float.MaxValue);
            RecoveredPosition = recoveredPosition.Value;
        }
        
        ExitLayoutInstance = (ExitRangeLayoutInstance*)exit;
        State              = 2;
    }
}

// TODO: FFCS
[StructLayout(LayoutKind.Explicit, Size = 160)]
public unsafe struct ExitRangeLayoutInstance
{
    [FieldOffset(0)]   public TriggerBoxLayoutInstance Base;
    [FieldOffset(128)] public ExitRangeType            ExitType;
    [FieldOffset(132)] public ushort                   ZoneID;
    [FieldOffset(134)] public ushort                   TerritoryType;
    [FieldOffset(136)] public int                      Index;
    [FieldOffset(140)] public uint                     DestInstanceID;
    [FieldOffset(144)] public uint                     ReturnInstanceID;
    [FieldOffset(148)] public float                    PlayerRunningDirection;

    public PopRangeLayoutInstance* ReturnInstance =>
        (PopRangeLayoutInstance*)LayoutWorld.GetLayoutInstanceStatic(InstanceType.PopRange, ReturnInstanceID);
    
    public enum ExitRangeType 
    {
        ZoneLine  = 1,
        Invisible = 2, // used for doors etc.
    }
}

[StructLayout(LayoutKind.Explicit, Size = 160)]
public unsafe struct PopRangeLayoutInstance
{
    [FieldOffset(0)]
    public TriggerBoxLayoutInstance Base;

    [FieldOffset(128)]
    public Vector3* AddPos;
}
