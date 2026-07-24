using CSDrawObject = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.DrawObject;
using ObjectHighlightColor = FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectHighlightColor;

namespace OmenTools.Dalamud.Services.Graphics.Scene.Abstractions;

public interface IDrawObject : ISceneObject
{
    byte                 Flags             { get; }
    byte                 OutlineFlags      { get; }
    bool                 IsCoveredFromRain { get; }
    byte                 LoadState         { get; }
    ObjectHighlightColor OutlineColor      { get; }
    bool                 IsVisible         { get; }
    float                Transparency      { get; }
    int                  TargetStatus      { get; }

    new unsafe CSDrawObject* ToStruct();

    new static IDrawObject Create(nint address) => new DrawObject(address);
}
