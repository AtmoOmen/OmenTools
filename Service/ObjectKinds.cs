using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Companion = Lumina.Excel.Sheets.Companion;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using CSCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using CSBattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;
using Ornament = Lumina.Excel.Sheets.Ornament;

namespace OmenTools.Service;

public unsafe class GameObject(nint address) : IGameObject
{
    public SeString              Name             => SeString.Parse(Struct->Name);
    public ulong                 GameObjectId     => Struct->GetGameObjectId();
    public uint                  EntityId         => Struct->EntityId;
    public uint                  DataId           => Struct->BaseId;
    public uint                  OwnerId          => Struct->OwnerId;
    public ushort                ObjectIndex      => Struct->ObjectIndex;
    public ObjectKind            ObjectKind       => (ObjectKind)Struct->ObjectKind;
    public byte                  SubKind          => Struct->SubKind;
    public byte                  YalmDistanceX    => Struct->YalmDistanceFromPlayerX;
    public byte                  YalmDistanceZ    => Struct->YalmDistanceFromPlayerZ;
    public Vector3               Position         => new(Struct->Position.X, Struct->Position.Y, Struct->Position.Z);
    public float                 Rotation         => Struct->Rotation;
    public float                 HitboxRadius     => Struct->HitboxRadius;
    public uint                  NamePlateIconId  => Struct->NamePlateIconId;
    public ushort                FateId           => Struct->FateId;
    public float                 Scale            => Struct->Scale;
    public float                 VfxScale         => Struct->VfxScale;
    public ObjectTargetableFlags TargetableStatus => Struct->TargetableStatus;
    public byte                  TargetStatus     => Struct->TargetStatus;
    public Vector3               DrawOffset       => Struct->DrawOffset;
    public float                 Height           => Struct->Height;
    public byte                  Sex              => Struct->Sex;
    public int                   RenderFlags      => Struct->RenderFlags;
    public Vector3               DefaultPosition  => Struct->DefaultPosition;
    public float                 DefaultRotation  => Struct->DefaultRotation;
    public bool                  IsDead           => Struct->IsDead();
    public bool                  IsTargetable     => Struct->GetIsTargetable();
    
    public virtual ulong        TargetObjectId => 0;
    public virtual IGameObject? TargetObject   => DService.ObjectTable.SearchById(TargetObjectId);
    
    public bool          IsValid()  => IsValid(this);
    public CSGameObject* ToStruct() => Struct;
    
    public nint Address { get; internal set; } = address;

    public static implicit operator bool(GameObject? gameObject) => IsValid(gameObject);

    public static bool operator ==(GameObject? gameObject1, GameObject? gameObject2)
    {
        if (gameObject1 is null || gameObject2 is null)
            return Equals(gameObject1, gameObject2);

        return gameObject1.Equals(gameObject2);
    }

    public static bool operator !=(GameObject? actor1, GameObject? actor2) => !(actor1 == actor2);

    public static bool IsValid(IGameObject? actor) =>
        actor is not null && DService.ClientState.LocalContentId != 0;
    
    bool IEquatable<IGameObject>.Equals(IGameObject? other) => GameObjectId == other?.GameObjectId;

    public override bool Equals(object? obj) => ((IEquatable<IGameObject>)this).Equals(obj as IGameObject);

    public override int GetHashCode() => GameObjectId.GetHashCode();
    
    public override string ToString() => $"{GameObjectId:X}({Name.TextValue} - {ObjectKind}) Address: {Address:X}";
    
    protected internal CSGameObject* Struct => (CSGameObject*)Address;
}

public unsafe class Character(nint address) : GameObject(address), ICharacter
{
    public float                ModelScale          => Struct->CharacterData.ModelScale;
    public int                  ModelCharaId        => Struct->ModelContainer.ModelCharaId;
    public int                  ModelSkeletonId     => Struct->ModelContainer.ModelSkeletonId;
    public uint                 CurrentHp           => Struct->CharacterData.Health;
    public uint                 MaxHp               => Struct->CharacterData.MaxHealth;
    public uint                 CurrentMp           => Struct->CharacterData.Mana;
    public uint                 MaxMp               => Struct->CharacterData.MaxMana;
    public uint                 CurrentGp           => Struct->CharacterData.GatheringPoints;
    public uint                 MaxGp               => Struct->CharacterData.MaxGatheringPoints;
    public uint                 CurrentCp           => Struct->CharacterData.CraftingPoints;
    public uint                 MaxCp               => Struct->CharacterData.MaxCraftingPoints;
    public ushort               TitleId             => Struct->CharacterData.TitleId;
    public byte                 Icon                => Struct->CharacterData.Icon;
    public byte                 ENpcMap             => Struct->CharacterData.Map;
    public BattalionFlags       Battalion           => (BattalionFlags)Struct->CharacterData.Battalion;
    public byte                 ShieldPercentage    => Struct->CharacterData.ShieldValue;
    public RowRef<ClassJob>     ClassJob            => LuminaCreateRef<ClassJob>(Struct->CharacterData.ClassJob);
    public byte                 Level               => Struct->CharacterData.Level;
    public byte[]               Customize           => Struct->DrawData.CustomizeData.Data.ToArray();
    public SeString             CompanyTag          => SeString.Parse(Struct->FreeCompanyTag);
    public float                Alpha               => Struct->Alpha;
    public uint                 NameId              => Struct->NameId;
    public ulong                AccountId           => Struct->AccountId;
    public ulong                ContentId           => Struct->ContentId;
    public CharacterModes       Mode                => Struct->Mode;
    public byte                 ModeParam           => Struct->ModeParam;
    public RowRef<OnlineStatus> OnlineStatus        => LuminaCreateRef<OnlineStatus>(Struct->CharacterData.OnlineStatus);
    public ulong                EmoteTargetObjectId => Struct->EmoteController.Target;
    public IGameObject?         EmoteTargetObject   => DService.ObjectTable.SearchById(EmoteTargetObjectId);
    public bool                 IsWanderer          => Struct->IsWanderer();
    public bool                 IsTraveler          => Struct->IsTraveler();
    public bool                 IsVoyager           => Struct->IsVoyager();
    public RowRef<World>        CurrentWorld        => LuminaCreateRef<World>(Struct->CurrentWorld);
    public RowRef<World>        HomeWorld           => LuminaCreateRef<World>(Struct->HomeWorld);

    public override ulong TargetObjectId => Struct->TargetId;

    public StatusFlags StatusFlags =>
        (Struct->IsHostile ? StatusFlags.Hostile : StatusFlags.None)               |
        (Struct->InCombat ? StatusFlags.InCombat : StatusFlags.None)               |
        (Struct->IsWeaponDrawn ? StatusFlags.WeaponOut : StatusFlags.None)         |
        (Struct->IsOffhandDrawn ? StatusFlags.OffhandOut : StatusFlags.None)       |
        (Struct->IsPartyMember ? StatusFlags.PartyMember : StatusFlags.None)       |
        (Struct->IsAllianceMember ? StatusFlags.AllianceMember : StatusFlags.None) |
        (Struct->IsFriend ? StatusFlags.Friend : StatusFlags.None)                 |
        (Struct->IsCasting ? StatusFlags.IsCasting : StatusFlags.None);

    public RowRef<Emote>? CurrentEmote
    {
        get
        {
            var emoteID = Struct->EmoteController.EmoteId;
            return emoteID == 0 ? null : LuminaCreateRef<Emote>(emoteID);
        }
    }

    public RowRef<Mount>? CurrentMount
    {
        get
        {
            if (Struct->IsNotMounted()) return null;

            var mountId = Struct->Mount.MountId;
            return mountId == 0 ? null : LuminaCreateRef<Mount>(mountId);
        }
    }

    public RowRef<Ornament>? CurrentOrnament
    {
        get
        {
            var ornamentId = Struct->OrnamentData.OrnamentId;
            return ornamentId == 0 ? null : LuminaCreateRef<Ornament>(ornamentId);
        }
    }

    public RowRef<Companion>? CurrentMinion
    {
        get
        {
            if (Struct->CompanionObject != null)
                return LuminaCreateRef<Companion>(Struct->CompanionObject->BaseId);

            var hiddenCompanionId = Struct->CompanionData.CompanionId;
            return hiddenCompanionId == 0 ? null : LuminaCreateRef<Companion>(hiddenCompanionId);
        }
    }

    public new CSCharacter* ToStruct() => Struct;
    
    protected internal new CSCharacter* Struct => (CSCharacter*)Address;
}

public unsafe class BattleChara(nint address) : Character(address), IBattleChara
{
    public StatusList   StatusList          => new(this.Struct->GetStatusManager());
    public bool         IsCasting           => CastInfo.IsCasting     > 0;
    public bool         IsCastInterruptible => CastInfo.Interruptible > 0;
    public ActionType   CastActionType      => CastInfo.ActionType;
    public uint         CastActionId        => CastInfo.ActionId;
    public ulong        CastTargetObjectId  => CastInfo.TargetId;
    public IGameObject? CastTargetObject    => DService.ObjectTable.SearchById(CastTargetObjectId);
    public float        CurrentCastTime     => CastInfo.CurrentCastTime != 0 ? CastInfo.CurrentCastTime : -1;
    public float        BaseCastTime        => CastInfo.BaseCastTime    != 0 ? CastInfo.BaseCastTime : -1;
    public float        TotalCastTime       => CastInfo.TotalCastTime   != 0 ? CastInfo.TotalCastTime : -1;

    public new CSBattleChara* ToStruct() => Struct;

    private       CastInfo       CastInfo => Struct->CastInfo;
    protected new CSBattleChara* Struct   => (CSBattleChara*)Address;
}

public class EventObj(nint address) : GameObject(address), IEventObj;

public class Npc(nint address) : Character(address), INpc;

public unsafe class BattleNpc(nint address) : BattleChara(address), IBattleNpc
{
    public BattleNpcSubKind BattleNpcKind => (BattleNpcSubKind)Struct->Character.GameObject.SubKind;

    public override ulong TargetObjectId => Struct->Character.TargetId;
}

public unsafe class PlayerCharacter(nint address) : BattleChara(address), IPlayerCharacter
{
    public override ulong TargetObjectId => Struct->LookAt.Controller.Params[0].TargetParam.TargetId;
}

public interface IGameObject : IEquatable<IGameObject>
{
    public SeString              Name             { get; }
    public ulong                 GameObjectId     { get; }
    public uint                  EntityId         { get; }
    public uint                  DataId           { get; }
    public uint                  OwnerId          { get; }
    public ushort                ObjectIndex      { get; }
    public ObjectKind            ObjectKind       { get; }
    public byte                  SubKind          { get; }
    public byte                  YalmDistanceX    { get; }
    public byte                  YalmDistanceZ    { get; }
    public bool                  IsDead           { get; }
    public bool                  IsTargetable     { get; }
    public Vector3               Position         { get; }
    public float                 Rotation         { get; }
    public float                 HitboxRadius     { get; }
    public ulong                 TargetObjectId   { get; }
    public IGameObject?          TargetObject     { get; }
    public uint                  NamePlateIconId  { get; }
    public ushort                FateId           { get; }
    public float                 Scale            { get; }
    public float                 VfxScale         { get; }
    public ObjectTargetableFlags TargetableStatus { get; }
    public byte                  TargetStatus     { get; }
    public nint                  Address          { get; }
    public Vector3               DrawOffset       { get; }
    public float                 Height           { get; }
    public byte                  Sex              { get; }
    public int                   RenderFlags      { get; }
    public Vector3               DefaultPosition  { get; }
    public float                 DefaultRotation  { get; }

    public bool IsValid();

    public unsafe CSGameObject* ToStruct();
}

public interface ICharacter : IGameObject
{
    public float                ModelScale          { get; }
    public int                  ModelCharaId        { get; }
    public int                  ModelSkeletonId     { get; }
    public uint                 CurrentHp           { get; }
    public uint                 MaxHp               { get; }
    public uint                 CurrentMp           { get; }
    public uint                 MaxMp               { get; }
    public uint                 CurrentGp           { get; }
    public uint                 MaxGp               { get; }
    public uint                 CurrentCp           { get; }
    public uint                 MaxCp               { get; }
    public ushort               TitleId             { get; }
    public byte                 Icon                { get; }
    public byte                 ENpcMap             { get; }
    public BattalionFlags       Battalion           { get; }
    public byte                 ShieldPercentage    { get; }
    public RowRef<ClassJob>     ClassJob            { get; }
    public byte                 Level               { get; }
    public byte[]               Customize           { get; }
    public SeString             CompanyTag          { get; }
    public float                Alpha               { get; }
    public uint                 NameId              { get; }
    public ulong                AccountId           { get; }
    public ulong                ContentId           { get; }
    public CharacterModes       Mode                { get; }
    public byte                 ModeParam           { get; }
    public RowRef<OnlineStatus> OnlineStatus        { get; }
    public StatusFlags          StatusFlags         { get; }
    public RowRef<Emote>?       CurrentEmote        { get; }
    public ulong                EmoteTargetObjectId { get; }
    public IGameObject?         EmoteTargetObject   { get; }
    public bool                 IsWanderer          { get; }
    public bool                 IsTraveler          { get; }
    public bool                 IsVoyager           { get; }
    public RowRef<World>        CurrentWorld        { get; }
    public RowRef<World>        HomeWorld           { get; }
    public RowRef<Mount>?       CurrentMount        { get; }
    public RowRef<Ornament>?    CurrentOrnament     { get; }
    public RowRef<Companion>?   CurrentMinion       { get; }
    
    public new unsafe CSCharacter* ToStruct();
}

public interface IBattleChara : ICharacter
{
    public StatusList   StatusList          { get; }
    public bool         IsCasting           { get; }
    public bool         IsCastInterruptible { get; }
    public ActionType   CastActionType      { get; }
    public uint         CastActionId        { get; }
    public ulong        CastTargetObjectId  { get; }
    public IGameObject? CastTargetObject    { get; }
    public float        CurrentCastTime     { get; }
    public float        BaseCastTime        { get; }
    public float        TotalCastTime       { get; }
    
    public new unsafe CSBattleChara* ToStruct();
}

public interface IBattleNpc : IBattleChara
{
    BattleNpcSubKind BattleNpcKind { get; }
}

public interface IEventObj : IGameObject;

public interface INpc : ICharacter;

public interface IPlayerCharacter : IBattleChara;

public enum BattalionFlags : byte
{
    FriendOrMaelstrom = 0,
    TwinAdder = 1,
    ImmortalFlames = 2,
    Enemy = 4
}
