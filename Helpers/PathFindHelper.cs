using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.Config;
using Dalamud.Hooking;

namespace OmenTools.Helpers;

public unsafe class PathFindHelper : IDisposable
{
    private const float DEFAULT_PRECISION = 0.01f;

    public bool Enabled
    {
        get => RMIWalkHook.IsEnabled;
        set
        {
            if (value)
            {
                RMIWalkHook.Enable();
                RMIFlyHook.Enable();
            }
            else
            {
                RMIWalkHook.Disable();
                RMIFlyHook.Disable();
            }
        }
    }

    public bool    IsAutoMove      { get; set; } = true;
    public Vector3 DesiredPosition { get; set; }
    public float   Precision       { get; set; } = DEFAULT_PRECISION;

    private delegate bool RMIWalkIsInputEnabledDelegate(void* self);

    private static readonly RMIWalkIsInputEnabledDelegate rmiWalkIsInputEnabled1 =
        new CompSig("E8 ?? ?? ?? ?? 84 C0 75 10 38 43 3C").GetDelegate<RMIWalkIsInputEnabledDelegate>();

    private static readonly RMIWalkIsInputEnabledDelegate rmiWalkIsInputEnabled2 =
        new CompSig("E8 ?? ?? ?? ?? 84 C0 75 03 88 47 3F").GetDelegate<RMIWalkIsInputEnabledDelegate>();

    private readonly CompSig RMIWalkSig = new("E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D");
    private delegate void RMIWalkDelegate(
        void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
    private readonly Hook<RMIWalkDelegate>? RMIWalkHook;
    
    private readonly CompSig               RMIFlySig = new("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? B8");
    private delegate void                  RMIFlyDelegate(void* self, PlayerMoveControllerFlyInput* result);
    private readonly Hook<RMIFlyDelegate>? RMIFlyHook;
    
    public PathFindHelper()
    {
        RMIWalkHook ??= RMIWalkSig.GetHook<RMIWalkDelegate>(RMIWalkDetour);
        RMIFlyHook  ??= RMIFlySig.GetHook<RMIFlyDelegate>(RMIFlyDetour);
    }

    public void Dispose()
    {
        RMIWalkHook?.Dispose();
        RMIFlyHook?.Dispose();
    }

    private void RMIWalkDetour(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    {
        RMIWalkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);

        var canMove = bAdditiveUnk == 0 && rmiWalkIsInputEnabled1(self) && rmiWalkIsInputEnabled2(self);
        if (!canMove) return;

        var shouldUpdateMovement = IsAutoMove || (*sumLeft == 0 && *sumForward == 0);
        if (!shouldUpdateMovement) return;

        if (Angle.TryGetDirectionToDestination(DesiredPosition, false, out var realDir, Precision))
        {
            var dir = realDir.H.ToDirection();
            *sumLeft    = dir.X;
            *sumForward = dir.Y;
        }
    }

    private void RMIFlyDetour(void* self, PlayerMoveControllerFlyInput* result)
    {
        RMIFlyHook.Original(self, result);

        var shouldUpdateMovement = IsAutoMove || result->Forward != 0 || result->Left != 0 || result->Up != 0;
        if (!shouldUpdateMovement) return;

        if (Angle.TryGetDirectionToDestination(DesiredPosition, true, out var realDir, Precision))
        {
            var dir = realDir.H.ToDirection();
            result->Forward = dir.Y;
            result->Left    = dir.X;
            result->Up      = realDir.V.Rad;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x18)]
    private struct PlayerMoveControllerFlyInput
    {
        [FieldOffset(0x0)] public float Forward;
        [FieldOffset(0x4)] public float Left;
        [FieldOffset(0x8)] public float Up;
    }
}
