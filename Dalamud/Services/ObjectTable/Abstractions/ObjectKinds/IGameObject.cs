using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using GameObject = OmenTools.Dalamud.Services.ObjectTable.ObjectKinds.GameObject;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace OmenTools.Dalamud.Services.ObjectTable.Abstractions.ObjectKinds;

public interface IGameObject : IEquatable<IGameObject>
{
    SeString              Name             { get; }
    ulong                 GameObjectID     { get; }
    uint                  EntityID         { get; }
    uint                  DataID           { get; }
    uint                  OwnerID          { get; }
    ushort                ObjectIndex      { get; }
    ObjectKind            ObjectKind       { get; }
    byte                  SubKind          { get; }
    byte                  YalmDistanceX    { get; }
    byte                  YalmDistanceZ    { get; }
    bool                  IsDead           { get; }
    bool                  IsTargetable     { get; }
    Vector3               Position         { get; }
    float                 Rotation         { get; }
    float                 HitboxRadius     { get; }
    ulong                 TargetObjectID   { get; }
    IGameObject?          TargetObject     { get; }
    uint                  NamePlateIconID  { get; }
    ushort                FateID           { get; }
    float                 Scale            { get; }
    float                 VfxScale         { get; }
    ObjectTargetableFlags TargetableStatus { get; }
    byte                  TargetStatus     { get; }
    nint                  Address          { get; }
    Vector3               DrawOffset       { get; }
    float                 Height           { get; }
    byte                  Sex              { get; }
    VisibilityFlags       RenderFlags      { get; }
    Vector3               DefaultPosition  { get; }
    float                 DefaultRotation  { get; }

    bool IsValid();

    unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* ToStruct();

    unsafe BattleChara* ToBCStruct();

    static IGameObject Create(nint address) => new GameObject(address);
}
