using CSDrawObject = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.DrawObject;
using ObjectHighlightColor = FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectHighlightColor;

namespace OmenTools.Dalamud.Services.Graphics.Scene;

internal unsafe class DrawObject
(
    nint address
) : SceneObject(address), IDrawObject
{
    private new CSDrawObject* Struct => (CSDrawObject*)Address;

    public byte                 Flags             => Struct->Flags;
    public byte                 OutlineFlags      => Struct->OutlineFlags;
    public bool                 IsCoveredFromRain => Struct->IsCoveredFromRain;
    public byte                 LoadState         => Struct->LoadState;
    public ObjectHighlightColor OutlineColor      => Struct->OutlineColor;
    public bool                 IsVisible         => Struct->IsVisible;
    public float                Transparency      => Struct->GetTransparency();
    public int                  TargetStatus      => Struct->GetTargetStatus();

    public new CSDrawObject* ToStruct() => Struct;
}
