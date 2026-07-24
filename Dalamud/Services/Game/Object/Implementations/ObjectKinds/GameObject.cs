using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using OmenTools.OmenService;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using CSBattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;
using EventId = FFXIVClientStructs.FFXIV.Client.Game.Event.EventId;
using DrawObject = OmenTools.Dalamud.Services.Graphics.Scene.DrawObject;
using EventHandler = OmenTools.Dalamud.Services.Game.Event.EventHandler;
using LuaActor = OmenTools.Dalamud.Services.Game.Event.LuaActor;
using SharedGroupLayoutInstance = OmenTools.Dalamud.Services.LayoutEngine.Group.SharedGroupLayoutInstance;

namespace OmenTools.Dalamud.Services.Game.Object.ObjectKinds;

internal unsafe class GameObject
(
    nint address
) : IGameObject
{
    protected internal CSGameObject*    Struct              => (CSGameObject*)Address;
    public             string           Name                => Struct->NameString;
    public             ulong            GameObjectID        => Struct->GetGameObjectId();
    public             uint             EntityID            => Struct->EntityId;
    public             byte             EventState          => Struct->EventState;
    public             uint             LayoutID            => Struct->LayoutId;
    public             uint             GimmickID           => Struct->GimmickId;
    public             uint             DataID              => Struct->BaseId;
    public             uint             OwnerID             => Struct->OwnerId;
    public             ushort           ObjectIndex         => Struct->ObjectIndex;
    public             ObjectKind       ObjectKind          => (ObjectKind)Struct->ObjectKind;
    public             byte             SubKind             => Struct->SubKind;
    public             BattleNpcSubKind BattleNPCSubKind    => Struct->BattleNpcSubKind;
    public             byte             CurrentTargetStatus => Struct->CurrentTargetStatus;
    public             byte             CurrentDistance     => Struct->CurrentDistance;
    public             byte             NextTargetStatus    => Struct->NextTargetStatus;
    public             byte             NextDistance        => Struct->NextDistance;
    public             byte             Visibility          => Struct->Visibility;
    public             Vector3          Position            => new(Struct->Position.X, Struct->Position.Y, Struct->Position.Z);
    public             float            Rotation            => Struct->Rotation;
    public             float            HitboxRadius        => Struct->HitboxRadius;
    public             uint             NamePlateIconID     => Struct->NamePlateIconId;
    public             ushort           FateID              => Struct->FateId;
    public             EventId          EventID             => Struct->EventId;

    public IDrawObject? DrawObject => Struct->DrawObject == null ?
                                          null :
                                          new DrawObject((nint)Struct->DrawObject);

    public ISharedGroupLayoutInstance? SharedGroupLayoutInstance => Struct->SharedGroupLayoutInstance == null ?
                                                                        null :
                                                                        new SharedGroupLayoutInstance((nint)Struct->SharedGroupLayoutInstance);

    public ILuaActor? LuaActor => Struct->LuaActor == null ?
                                      null :
                                      new LuaActor((nint)Struct->LuaActor);

    public IEventHandler? EventHandler => Struct->EventHandler == null ?
                                              null :
                                              new EventHandler((nint)Struct->EventHandler);

    public float                 Scale                          => Struct->Scale;
    public float                 VfxScale                       => Struct->VfxScale;
    public ObjectTargetableFlags TargetableStatus               => Struct->TargetableStatus;
    public ObjectUpdateFlags     UpdateFlags                    => Struct->UpdateFlags;
    public byte                  TargetStatus                   => Struct->NextTargetStatus;
    public Vector3               DrawOffset                     => Struct->DrawOffset;
    public float                 Height                         => Struct->Height;
    public byte                  Sex                            => Struct->Sex;
    public VisibilityFlags       RenderFlags                    => Struct->RenderFlags;
    public Vector3               DefaultPosition                => Struct->DefaultPosition;
    public float                 DefaultRotation                => Struct->DefaultRotation;
    public Vector3               NameplateOffset                => Struct->NameplateOffset;
    public Vector3               CameraOffset                   => Struct->CameraOffset;
    public float                 NameplateOffsetScaleMultiplier => Struct->NameplateOffsetScaleMultiplier;
    public Vector3               NameplateOffsetTarget          => Struct->NameplateOffsetTarget;
    public Vector3               CameraOffsetTarget             => Struct->CameraOffsetTarget;
    public bool                  IsDead                         => Struct->IsDead();
    public bool                  IsTargetable                   => Struct->GetIsTargetable();
    public byte                  Distance                       => Struct->NextDistance;

    public virtual ulong        TargetObjectID => 0;
    public virtual IGameObject? TargetObject   => DService.Instance().ObjectTable.SearchByID(TargetObjectID);

    public bool IsValid() => IsValid(this);

    public CSGameObject* ToStruct() => Struct;

    public CSBattleChara* ToBCStruct() => (CSBattleChara*)Struct;

    public nint Address { get; internal set; } = address;

    bool IEquatable<IGameObject>.Equals(IGameObject? other) =>
        GameObjectID == other?.GameObjectID;

    public static implicit operator bool(GameObject? gameObject) => IsValid(gameObject);

    public static bool operator ==(GameObject? gameObject1, GameObject? gameObject2)
    {
        if (gameObject1 is null || gameObject2 is null)
            return Equals(gameObject1, gameObject2);

        return gameObject1.Equals(gameObject2);
    }

    public static bool operator !=(GameObject? actor1, GameObject? actor2) => !(actor1 == actor2);

    public static bool IsValid(IGameObject? actor) =>
        actor is not null                         &&
        LocalPlayerState.ContentID   != 0         &&
        actor.Address                != nint.Zero &&
        (CSGameObject*)actor.Address != null;

    public override bool Equals(object? obj) =>
        ((IEquatable<IGameObject>)this).Equals(obj as IGameObject);

    public override int GetHashCode() =>
        GameObjectID.GetHashCode();

    public override string ToString() =>
        $"{GameObjectID:X}({Name} - {ObjectKind}) Address: {Address:X}";
}
