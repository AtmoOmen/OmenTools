using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using OmenTools.OmenService;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using CSBattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;

namespace OmenTools.Dalamud.Services.ObjectTable.ObjectKinds;

internal unsafe class GameObject
(
    nint address
) : IGameObject
{
    protected internal CSGameObject*         Struct           => (CSGameObject*)Address;
    public             SeString              Name             => SeString.Parse(Struct->Name);
    public             ulong                 GameObjectID     => Struct->GetGameObjectId();
    public             uint                  EntityID         => Struct->EntityId;
    public             uint                  DataID           => Struct->BaseId;
    public             uint                  OwnerID          => Struct->OwnerId;
    public             ushort                ObjectIndex      => Struct->ObjectIndex;
    public             ObjectKind            ObjectKind       => (ObjectKind)Struct->ObjectKind;
    public             byte                  SubKind          => Struct->SubKind;
    public             byte                  YalmDistanceX    => Struct->YalmDistanceFromPlayerX;
    public             byte                  YalmDistanceZ    => Struct->YalmDistanceFromPlayerZ;
    public             Vector3               Position         => new(Struct->Position.X, Struct->Position.Y, Struct->Position.Z);
    public             float                 Rotation         => Struct->Rotation;
    public             float                 HitboxRadius     => Struct->HitboxRadius;
    public             uint                  NamePlateIconID  => Struct->NamePlateIconId;
    public             ushort                FateID           => Struct->FateId;
    public             float                 Scale            => Struct->Scale;
    public             float                 VfxScale         => Struct->VfxScale;
    public             ObjectTargetableFlags TargetableStatus => Struct->TargetableStatus;
    public             byte                  TargetStatus     => Struct->TargetStatus;
    public             Vector3               DrawOffset       => Struct->DrawOffset;
    public             float                 Height           => Struct->Height;
    public             byte                  Sex              => Struct->Sex;
    public             VisibilityFlags       RenderFlags      => Struct->RenderFlags;
    public             Vector3               DefaultPosition  => Struct->DefaultPosition;
    public             float                 DefaultRotation  => Struct->DefaultRotation;
    public             bool                  IsDead           => Struct->IsDead();
    public             bool                  IsTargetable     => Struct->GetIsTargetable();

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
        $"{GameObjectID:X}({Name.TextValue} - {ObjectKind}) Address: {Address:X}";
}
