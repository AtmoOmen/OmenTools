using System.Numerics;
using CSSceneObject = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object;
using ObjectType = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.ObjectType;

namespace OmenTools.Dalamud.Services.Graphics.Scene;

internal unsafe class SceneObject
(
    nint address
) : ISceneObject
{
    protected CSSceneObject* Struct => (CSSceneObject*)Address;

    public nint          Address               { get; } = address;
    public ISceneObject? ParentObject          => Create(Struct->ParentObject);
    public ISceneObject? PreviousSiblingObject => Create(Struct->PreviousSiblingObject);
    public ISceneObject? NextSiblingObject     => Create(Struct->NextSiblingObject);
    public ISceneObject? ChildObject           => Create(Struct->ChildObject);
    public ulong         ObjectFlags           => Struct->ObjectFlags;
    public bool          IsTransformChanged    => Struct->IsTransformChanged;
    public Vector3       Position              => Struct->Position;
    public Quaternion    Rotation              => Struct->Rotation;
    public Vector3       Scale                 => Struct->Scale;
    public ObjectType    ObjectType            => Struct->GetObjectType();

    public CSSceneObject* ToStruct() => Struct;

    private static ISceneObject? Create(CSSceneObject* sceneObject) => sceneObject                    == null              ? null
                                                                       : sceneObject->GetObjectType() == ObjectType.Object ? new SceneObject((nint)sceneObject)
                                                                                                                             : new DrawObject((nint)sceneObject);
}
