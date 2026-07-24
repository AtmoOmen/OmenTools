using System.Numerics;
using CSBattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using CSBattleNPCSubKind = FFXIVClientStructs.FFXIV.Client.Game.Object.BattleNpcSubKind;
using EventId = FFXIVClientStructs.FFXIV.Client.Game.Event.EventId;
using ObjectTargetableFlags = FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectTargetableFlags;
using ObjectUpdateFlags = FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectUpdateFlags;
using VisibilityFlags = FFXIVClientStructs.FFXIV.Client.Game.Object.VisibilityFlags;
using GameObject = OmenTools.Dalamud.Services.Game.Object.ObjectKinds.GameObject;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace OmenTools.Dalamud.Services.Game.Object.Abstractions.ObjectKinds;

public interface IGameObject : IEquatable<IGameObject>
{
    string                      Name                           { get; }
    ulong                       GameObjectID                   { get; }
    uint                        EntityID                       { get; }
    byte                        EventState                     { get; }
    uint                        LayoutID                       { get; }
    uint                        GimmickID                      { get; }
    uint                        DataID                         { get; }
    uint                        OwnerID                        { get; }
    ushort                      ObjectIndex                    { get; }
    ObjectKind                  ObjectKind                     { get; }
    byte                        SubKind                        { get; }
    CSBattleNPCSubKind          BattleNPCSubKind               { get; }
    byte                        CurrentTargetStatus            { get; }
    byte                        CurrentDistance                { get; }
    byte                        NextTargetStatus               { get; }
    byte                        NextDistance                   { get; }
    byte                        Visibility                     { get; }
    byte                        Distance                       { get; }
    bool                        IsDead                         { get; }
    bool                        IsTargetable                   { get; }
    Vector3                     Position                       { get; }
    float                       Rotation                       { get; }
    float                       HitboxRadius                   { get; }
    ulong                       TargetObjectID                 { get; }
    IGameObject?                TargetObject                   { get; }
    uint                        NamePlateIconID                { get; }
    ushort                      FateID                         { get; }
    EventId                     EventID                        { get; }
    IDrawObject?                DrawObject                     { get; }
    ISharedGroupLayoutInstance? SharedGroupLayoutInstance      { get; }
    ILuaActor?                  LuaActor                       { get; }
    IEventHandler?              EventHandler                   { get; }
    float                       Scale                          { get; }
    float                       VfxScale                       { get; }
    ObjectTargetableFlags       TargetableStatus               { get; }
    ObjectUpdateFlags           UpdateFlags                    { get; }
    byte                        TargetStatus                   { get; }
    nint                        Address                        { get; }
    Vector3                     DrawOffset                     { get; }
    float                       Height                         { get; }
    byte                        Sex                            { get; }
    VisibilityFlags             RenderFlags                    { get; }
    Vector3                     DefaultPosition                { get; }
    float                       DefaultRotation                { get; }
    Vector3                     NameplateOffset                { get; }
    Vector3                     CameraOffset                   { get; }
    float                       NameplateOffsetScaleMultiplier { get; }
    Vector3                     NameplateOffsetTarget          { get; }
    Vector3                     CameraOffsetTarget             { get; }

    bool IsValid();

    unsafe CSGameObject* ToStruct();

    unsafe CSBattleChara* ToBCStruct();

    static IGameObject Create(nint address) => new GameObject(address);
}
