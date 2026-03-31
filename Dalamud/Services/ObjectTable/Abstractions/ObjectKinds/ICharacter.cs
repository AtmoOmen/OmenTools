using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using OmenTools.Dalamud.Services.ObjectTable.Enums;
using Character = OmenTools.Dalamud.Services.ObjectTable.ObjectKinds.Character;
using Companion = Lumina.Excel.Sheets.Companion;
using Ornament = Lumina.Excel.Sheets.Ornament;

namespace OmenTools.Dalamud.Services.ObjectTable.Abstractions.ObjectKinds;

public interface ICharacter : IGameObject
{
    short                TransformationID    { get; }
    float                ModelScale          { get; }
    int                  ModelCharaID        { get; }
    int                  ModelSkeletonID     { get; }
    uint                 CurrentHp           { get; }
    uint                 MaxHp               { get; }
    uint                 CurrentMp           { get; }
    uint                 MaxMp               { get; }
    uint                 CurrentGp           { get; }
    uint                 MaxGp               { get; }
    uint                 CurrentCp           { get; }
    uint                 MaxCp               { get; }
    ushort               TitleID             { get; }
    byte                 Icon                { get; }
    byte                 ENPCMap             { get; }
    BattalionFlags       Battalion           { get; }
    byte                 ShieldPercentage    { get; }
    RowRef<ClassJob>     ClassJob            { get; }
    byte                 Level               { get; }
    byte[]               Customize           { get; }
    SeString             CompanyTag          { get; }
    float                Alpha               { get; }
    uint                 NameID              { get; }
    ulong                AccountID           { get; }
    ulong                ContentID           { get; }
    CharacterModes       Mode                { get; }
    byte                 ModeParam           { get; }
    RowRef<OnlineStatus> OnlineStatus        { get; }
    StatusFlags          StatusFlags         { get; }
    RowRef<Emote>?       CurrentEmote        { get; }
    ulong                EmoteTargetObjectID { get; }
    IGameObject?         EmoteTargetObject   { get; }
    bool                 IsWanderer          { get; }
    bool                 IsTraveler          { get; }
    bool                 IsVoyager           { get; }
    RowRef<World>        CurrentWorld        { get; }
    RowRef<World>        HomeWorld           { get; }
    RowRef<Mount>?       CurrentMount        { get; }
    RowRef<Ornament>?    CurrentOrnament     { get; }
    RowRef<Companion>?   CurrentMinion       { get; }

    new unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.Character* ToStruct();

    new static ICharacter Create(nint address) => new Character(address);
}
