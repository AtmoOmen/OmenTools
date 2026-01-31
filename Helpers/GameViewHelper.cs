using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using Control = FFXIVClientStructs.FFXIV.Client.Game.Control.Control;

namespace OmenTools.Helpers;

public static class GameViewHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool WorldToScreen(Vector3 worldPos, out Vector2 screenPos, out bool inView)
    {
        var dev     = Device.Instance();
        var viewPos = ImGuiHelpers.MainViewport.Pos;

        var mat = Control.Instance()->ViewProjectionMatrix;

        var wx = worldPos.X;
        var wy = worldPos.Y;
        var wz = worldPos.Z;

        var w = wx * mat.M14 + wy * mat.M24 + wz * mat.M34 + mat.M44;

        if (w < 0.001)
        {
            screenPos = default;
            inView    = false;
            return false;
        }

        var invW = 1.0                                                    / w;
        var x    = (wx * mat.M11 + wy * mat.M21 + wz * mat.M31 + mat.M41) * invW;
        var y    = (wx * mat.M12 + wy * mat.M22 + wz * mat.M32 + mat.M42) * invW;

        var width  = dev->Width;
        var height = dev->Height;
        var halfW  = width  * 0.5f;
        var halfH  = height * 0.5f;

        var sx = (float)(x * halfW)         + halfW + viewPos.X;
        var sy = halfH - (float)(y * halfH) + viewPos.Y;

        screenPos = new Vector2(sx, sy);

        inView = sx >= viewPos.X         && 
                 sx <= viewPos.X + width && 
                 sy >= viewPos.Y         && 
                 sy <= viewPos.Y + height;

        return true;
    }
}
