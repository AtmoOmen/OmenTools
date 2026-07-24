using System.Numerics;
using CSSceneObject = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object;
using ObjectType = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.ObjectType;

namespace OmenTools.Dalamud.Services.Graphics.Scene.Abstractions;

public interface ISceneObject
{
    nint          Address               { get; }
    ISceneObject? ParentObject          { get; }
    ISceneObject? PreviousSiblingObject { get; }
    ISceneObject? NextSiblingObject     { get; }
    ISceneObject? ChildObject           { get; }
    ulong         ObjectFlags           { get; }
    bool          IsTransformChanged    { get; }
    Vector3       Position              { get; }
    Quaternion    Rotation              { get; }
    Vector3       Scale                 { get; }
    ObjectType    ObjectType            { get; }

    unsafe CSSceneObject* ToStruct();

    static ISceneObject Create(nint address) => new SceneObject(address);
}
