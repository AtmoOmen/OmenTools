using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using OmenTools.Dalamud.Services.Game.Object.Enums;
using Character = OmenTools.Dalamud.Services.Game.Object.ObjectKinds.Character;
using Companion = Lumina.Excel.Sheets.Companion;
using Ornament = Lumina.Excel.Sheets.Ornament;
using CharacterModes = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterModes;
using CSCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using ObjectType = FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectType;
using BalloonState = FFXIVClientStructs.FFXIV.Client.Game.BalloonState;
using BalloonType = FFXIVClientStructs.FFXIV.Client.Game.BalloonType;
using PoseType = FFXIVClientStructs.FFXIV.Client.Game.Control.EmoteController.PoseType;
using BannerCameraFollowFlags = FFXIVClientStructs.FFXIV.Client.Game.Character.LookAtContainer.BannerCameraFollowFlags;
using ShroudFlags = FFXIVClientStructs.FFXIV.Client.Game.Character.ReaperShroudContainer.ShroudFlags;
using StatusEffect = FFXIVClientStructs.FFXIV.Client.Game.Character.EffectContainer.StatusEffect;
using TiltFlags = FFXIVClientStructs.FFXIV.Client.Game.Character.EffectContainer.TiltFlags;
using TiltOrigin = FFXIVClientStructs.FFXIV.Client.Game.Character.EffectContainer.TiltOrigin;
using NPCYellBalloonCloseType = FFXIVClientStructs.FFXIV.Client.Game.NpcYellBalloonCloseType;
using NPCYellBalloonFlags = FFXIVClientStructs.FFXIV.Client.Game.NpcYellBalloonFlags;
using NPCYellBalloonState = FFXIVClientStructs.FFXIV.Client.Game.NpcYellBalloonState;

namespace OmenTools.Dalamud.Services.Game.Object.Abstractions.ObjectKinds;

public interface ICharacter : IGameObject
{
    IBattleChara?           ContainerOwner                       { get; }
    short                   TransformationID                     { get; }
    ushort                  StatusLoopVfxID                      { get; }
    byte                    SEPack                               { get; }
    float                   ModelScale                           { get; }
    int                     ModelCharaID                         { get; }
    int                     ModelSkeletonID                      { get; }
    uint                    CurrentHp                            { get; }
    uint                    MaxHp                                { get; }
    uint                    CurrentMp                            { get; }
    uint                    MaxMp                                { get; }
    uint                    CurrentGp                            { get; }
    uint                    MaxGp                                { get; }
    uint                    CurrentCp                            { get; }
    uint                    MaxCp                                { get; }
    ushort                  TitleID                              { get; }
    byte                    Icon                                 { get; }
    byte                    ENPCMap                              { get; }
    BattalionFlags          Battalion                            { get; }
    byte                    ShieldPercentage                     { get; }
    byte                    CharacterFlags                       { get; }
    byte                    CombatTagType                        { get; }
    ulong                   CombatTaggerObjectID                 { get; }
    RowRef<ClassJob>        ClassJob                             { get; }
    byte                    Level                                { get; }
    byte[]                  Customize                            { get; }
    string                  CompanyTag                           { get; }
    float                   Alpha                                { get; }
    uint                    NameID                               { get; }
    ulong                   AccountID                            { get; }
    ulong                   ContentID                            { get; }
    ulong                   SoftTargetObjectID                   { get; }
    IGameObject?            SoftTargetObject                     { get; }
    CharacterModes          Mode                                 { get; }
    byte                    ModeParam                            { get; }
    byte                    WeaponFlags                          { get; }
    bool                    IsWeaponDrawn                        { get; }
    bool                    IsSwimming                           { get; }
    bool                    IsMounted                            { get; }
    bool                    IsInPvP                              { get; }
    uint                    EventNPCInstanceID                   { get; }
    byte                    RelationFlags                        { get; }
    byte                    ActorControlFlags                    { get; }
    float                   CastRotation                         { get; }
    ICharacter?             ChildObject                          { get; }
    uint                    CompanionOwnerID                     { get; }
    ObjectType              ObjectType                           { get; }
    byte                    GMRank                               { get; }
    byte                    SoundVolumeCategory                  { get; }
    byte                    SoundVolumeCategoryOverride          { get; }
    ushort                  MountID                              { get; }
    float                   DismountTimer                        { get; }
    byte                    MountFlags                           { get; }
    ICharacter?             MountObject                          { get; }
    ushort                  CompanionID                          { get; }
    ushort                  FollowMountID                        { get; }
    ICharacter?             CompanionObject                      { get; }
    ushort                  OrnamentID                           { get; }
    ICharacter?             OrnamentObject                       { get; }
    int                     ModelCharaID2                        { get; }
    int                     ModelSkeletonID2                     { get; }
    byte                    ModelScaleID                         { get; }
    byte                    ModelAttributeFlags                  { get; }
    float                   UnscaledRadius                       { get; }
    byte                    RepresentationNameType               { get; }
    string                  NameOverride                         { get; }
    float                   RepresentationUpdateTimer            { get; }
    ushort                  VoiceID                              { get; }
    byte                    TimelineModelState                   { get; }
    float                   OverallSpeed                         { get; }
    ushort                  BaseOverride                         { get; }
    ushort                  LipsOverride                         { get; }
    ushort                  BannerTimelineRowID                  { get; }
    byte                    BannerFacialRowID                    { get; }
    uint                    BannerTimelineNameOffset             { get; }
    uint                    BannerTimelineAdditionalData         { get; }
    int                     BannerTimelineIcon                   { get; }
    ushort                  BannerTimelineUnlockCondition        { get; }
    ushort                  BannerTimelineSortKey                { get; }
    byte                    BannerTimelineType                   { get; }
    byte                    BannerTimelineAcceptClassJobCategory { get; }
    byte                    BannerTimelineCategory               { get; }
    float                   BannerRequestStartTimestamp          { get; }
    Vector3                 CameraVector                         { get; }
    bool                    IsFacingCamera                       { get; }
    Vector2                 BannerHeadDirection                  { get; }
    Vector2                 BannerEyeDirection                   { get; }
    BannerCameraFollowFlags BannerCameraFollowFlag               { get; }
    bool                    IsHatHidden                          { get; }
    bool                    IsWeaponHidden                       { get; }
    bool                    IsVisorToggled                       { get; }
    bool                    VieraEarsHidden                      { get; }
    byte                    FreeCompanyCrestBitfield             { get; }
    ulong                   FreeCompanyCrestDataValue            { get; }
    byte                    FreeCompanyCrestCharge               { get; }
    byte                    FreeCompanyCrestOrdinaryTinctures    { get; }
    byte                    ReaperStanceChangeID                 { get; }
    uint                    ReaperStanceChangeState              { get; }
    float                   ReaperTimer                          { get; }
    ICharacter?             ReaperCopyObject                     { get; }
    ShroudFlags             ReaperFlags                          { get; }
    ushort                  ReaperNPCEquipID                     { get; }
    float                   CurrentFloatHeight                   { get; }
    float                   TargetFloatHeight                    { get; }
    float                   FloatHeightChangeSpeed               { get; }
    StatusEffect            StatusEffects                        { get; }
    int                     MountTiltSetupState1                 { get; }
    int                     MountTiltSetupState2                 { get; }
    TiltOrigin              MountGroundTiltOrigin                { get; }
    float                   MountGroundTiltAngle                 { get; }
    float                   MountGroundTiltSpeed                 { get; }
    TiltFlags               MountGroundTiltFlags                 { get; }
    TiltOrigin              MountFlightSwimTiltOrigin            { get; }
    float                   MountFlightSwimTiltAngle             { get; }
    float                   MountFlightSwimTiltSpeed             { get; }
    TiltFlags               MountFlightSwimTiltFlags             { get; }
    TiltOrigin              RiderGroundTiltOrigin                { get; }
    float                   RiderGroundTiltAngle                 { get; }
    float                   RiderGroundTiltSpeed                 { get; }
    TiltFlags               RiderGroundTiltFlags                 { get; }
    TiltOrigin              RiderFlightSwimTiltOrigin            { get; }
    float                   RiderFlightSwimTiltAngle             { get; }
    float                   RiderFlightSwimTiltSpeed             { get; }
    TiltFlags               RiderFlightSwimReverseTilt           { get; }
    ushort                  BalloonDefaultID                     { get; }
    ushort                  BalloonCurrentID                     { get; }
    float                   BalloonPlayTimer                     { get; }
    BalloonType             BalloonType                          { get; }
    BalloonState            BalloonState                         { get; }
    float                   NPCYellPlayTimer                     { get; }
    float                   NPCYellDelayTime                     { get; }
    NPCYellBalloonState     NPCYellState                         { get; }
    NPCYellBalloonCloseType NPCYellCloseType                     { get; }
    byte                    NPCYellParentBone                    { get; }
    NPCYellBalloonFlags     NPCYellFlags                         { get; }
    ICharacter?             NPCYellCharacter                     { get; }
    RowRef<OnlineStatus>    OnlineStatus                         { get; }
    StatusFlags             StatusFlags                          { get; }
    RowRef<Emote>?          CurrentEmote                         { get; }
    ulong                   EmoteTargetObjectID                  { get; }
    IGameObject?            EmoteTargetObject                    { get; }
    IBattleChara?           EmoteOwnerObject                     { get; }
    byte                    EmoteStance                          { get; }
    PoseType                CurrentPoseType                      { get; }
    byte                    CPoseState                           { get; }
    bool                    IsEmoting                            { get; }
    bool                    IsInEmoteLoop                        { get; }
    bool                    IsWanderer                           { get; }
    bool                    IsTraveler                           { get; }
    bool                    IsVoyager                            { get; }
    RowRef<World>           CurrentWorld                         { get; }
    RowRef<World>           HomeWorld                            { get; }
    RowRef<Mount>?          CurrentMount                         { get; }
    RowRef<Ornament>?       CurrentOrnament                      { get; }
    RowRef<Companion>?      CurrentMinion                        { get; }

    new unsafe CSCharacter* ToStruct();

    new static ICharacter Create(nint address) => new Character(address);
}
