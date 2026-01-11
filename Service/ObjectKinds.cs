using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using BattleNpcSubKind = Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind;
using Companion = Lumina.Excel.Sheets.Companion;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using CSCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using CSBattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;
using Ornament = Lumina.Excel.Sheets.Ornament;

namespace OmenTools.Service;

internal unsafe class GameObject(nint address) : IGameObject
{
    public SeString              Name             => SeString.Parse(Struct->Name);
    public ulong                 GameObjectID     => Struct->GetGameObjectId();
    public uint                  EntityID         => Struct->EntityId;
    public uint                  DataID           => Struct->BaseId;
    public uint                  OwnerID          => Struct->OwnerId;
    public ushort                ObjectIndex      => Struct->ObjectIndex;
    public ObjectKind            ObjectKind       => (ObjectKind)Struct->ObjectKind;
    public byte                  SubKind          => Struct->SubKind;
    public byte                  YalmDistanceX    => Struct->YalmDistanceFromPlayerX;
    public byte                  YalmDistanceZ    => Struct->YalmDistanceFromPlayerZ;
    public Vector3               Position         => new(Struct->Position.X, Struct->Position.Y, Struct->Position.Z);
    public float                 Rotation         => Struct->Rotation;
    public float                 HitboxRadius     => Struct->HitboxRadius;
    public uint                  NamePlateIconID  => Struct->NamePlateIconId;
    public ushort                FateID           => Struct->FateId;
    public float                 Scale            => Struct->Scale;
    public float                 VfxScale         => Struct->VfxScale;
    public ObjectTargetableFlags TargetableStatus => Struct->TargetableStatus;
    public byte                  TargetStatus     => Struct->TargetStatus;
    public Vector3               DrawOffset       => Struct->DrawOffset;
    public float                 Height           => Struct->Height;
    public byte                  Sex              => Struct->Sex;
    public VisibilityFlags       RenderFlags      => Struct->RenderFlags;
    public Vector3               DefaultPosition  => Struct->DefaultPosition;
    public float                 DefaultRotation  => Struct->DefaultRotation;
    public bool                  IsDead           => Struct->IsDead();
    public bool                  IsTargetable     => Struct->GetIsTargetable();
    
    public virtual ulong        TargetObjectID => 0;
    public virtual IGameObject? TargetObject   => DService.Instance().ObjectTable.SearchByID(TargetObjectID);
    
    public bool           IsValid()    => IsValid(this);
    public CSGameObject*  ToStruct()   => Struct;
    public CSBattleChara* ToBCStruct() => (CSBattleChara*)Struct;
    
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
        actor is not null                         && 
        LocalPlayerState.ContentID   != 0         &&
        actor.Address                != nint.Zero &&
        (CSGameObject*)actor.Address != null;
    
    bool IEquatable<IGameObject>.Equals(IGameObject? other) =>
        GameObjectID == other?.GameObjectID;

    public override bool Equals(object? obj) => 
        ((IEquatable<IGameObject>)this).Equals(obj as IGameObject);

    public override int GetHashCode() => 
        GameObjectID.GetHashCode();
    
    public override string ToString() => 
        $"{GameObjectID:X}({Name.TextValue} - {ObjectKind}) Address: {Address:X}";
    
    protected internal CSGameObject* Struct => (CSGameObject*)Address;
}

internal unsafe class Character(nint address) : GameObject(address), ICharacter
{
    public short                TransformationID    => Struct->TransformationId;
    public float                ModelScale          => Struct->CharacterData.ModelScale;
    public int                  ModelCharaID        => Struct->ModelContainer.ModelCharaId;
    public int                  ModelSkeletonID     => Struct->ModelContainer.ModelSkeletonId;
    public uint                 CurrentHp           => Struct->CharacterData.Health;
    public uint                 MaxHp               => Struct->CharacterData.MaxHealth;
    public uint                 CurrentMp           => Struct->CharacterData.Mana;
    public uint                 MaxMp               => Struct->CharacterData.MaxMana;
    public uint                 CurrentGp           => Struct->CharacterData.GatheringPoints;
    public uint                 MaxGp               => Struct->CharacterData.MaxGatheringPoints;
    public uint                 CurrentCp           => Struct->CharacterData.CraftingPoints;
    public uint                 MaxCp               => Struct->CharacterData.MaxCraftingPoints;
    public ushort               TitleID             => Struct->CharacterData.TitleId;
    public byte                 Icon                => Struct->CharacterData.Icon;
    public byte                 ENPCMap             => Struct->CharacterData.Map;
    public BattalionFlags       Battalion           => (BattalionFlags)Struct->CharacterData.Battalion;
    public byte                 ShieldPercentage    => Struct->CharacterData.ShieldValue;
    public RowRef<ClassJob>     ClassJob            => Struct->CharacterData.ClassJob.ToLuminaRowRef<ClassJob>();
    public byte                 Level               => Struct->CharacterData.Level;
    public byte[]               Customize           => Struct->DrawData.CustomizeData.Data.ToArray();
    public SeString             CompanyTag          => SeString.Parse(Struct->FreeCompanyTag);
    public float                Alpha               => Struct->Alpha;
    public uint                 NameID              => Struct->NameId;
    public ulong                AccountID           => Struct->AccountId;
    public ulong                ContentID           => Struct->ContentId;
    public CharacterModes       Mode                => Struct->Mode;
    public byte                 ModeParam           => Struct->ModeParam;
    public RowRef<OnlineStatus> OnlineStatus        => Struct->CharacterData.OnlineStatus.ToLuminaRowRef<OnlineStatus>();
    public ulong                EmoteTargetObjectID => Struct->EmoteController.Target;
    public IGameObject?         EmoteTargetObject   => DService.Instance().ObjectTable.SearchByID(EmoteTargetObjectID);
    public bool                 IsWanderer          => Struct->IsWanderer();
    public bool                 IsTraveler          => Struct->IsTraveler();
    public bool                 IsVoyager           => Struct->IsVoyager();
    public RowRef<World>        CurrentWorld        => Struct->CurrentWorld.ToLuminaRowRef<World>();
    public RowRef<World>        HomeWorld           => Struct->HomeWorld.ToLuminaRowRef<World>();

    public override ulong TargetObjectID => Struct->TargetId;

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
            return emoteID == 0 ? null : emoteID.ToLuminaRowRef<Emote>();
        }
    }

    public RowRef<Mount>? CurrentMount
    {
        get
        {
            if (Struct->IsNotMounted()) return null;

            var mountID = Struct->Mount.MountId;
            return mountID == 0 ? null : mountID.ToLuminaRowRef<Mount>();
        }
    }

    public RowRef<Ornament>? CurrentOrnament
    {
        get
        {
            var ornamentID = Struct->OrnamentData.OrnamentId;
            return ornamentID == 0 ? null : ornamentID.ToLuminaRowRef<Ornament>();
        }
    }

    public RowRef<Companion>? CurrentMinion
    {
        get
        {
            if (Struct->CompanionObject != null)
                return Struct->CompanionObject->BaseId.ToLuminaRowRef<Companion>();

            var hiddenCompanionID = Struct->CompanionData.CompanionId;
            return hiddenCompanionID == 0 ? null : hiddenCompanionID.ToLuminaRowRef<Companion>();
        }
    }

    public new CSCharacter* ToStruct() => Struct;
    
    protected internal new CSCharacter* Struct => (CSCharacter*)Address;
}

internal unsafe class BattleChara(nint address) : Character(address), IBattleChara
{
    public StatusList   StatusList          => new(this.Struct->GetStatusManager());
    public bool         IsCasting           => CastInfo.IsCasting;
    public bool         IsCastInterruptible => CastInfo.Interruptible;
    public ActionType   CastActionType      => (ActionType)CastInfo.ActionType;
    public uint         CastActionID        => CastInfo.ActionId;
    public ulong        CastTargetObjectID  => CastInfo.TargetId;
    public IGameObject? CastTargetObject    => DService.Instance().ObjectTable.SearchByID(CastTargetObjectID);
    public float        CurrentCastTime     => CastInfo.CurrentCastTime != 0 ? CastInfo.CurrentCastTime : -1;
    public float        BaseCastTime        => CastInfo.BaseCastTime    != 0 ? CastInfo.BaseCastTime : -1;
    public float        TotalCastTime       => CastInfo.TotalCastTime   != 0 ? CastInfo.TotalCastTime : -1;

    public new CSBattleChara* ToStruct() => Struct;

    private       CastInfo       CastInfo => Struct->CastInfo;
    protected new CSBattleChara* Struct   => (CSBattleChara*)Address;
}

internal class EventObj(nint address) : GameObject(address), IEventObj;

internal class NPC(nint address) : Character(address), INPC;

internal unsafe class BattleNPC(nint address) : BattleChara(address), IBattleNPC
{
    public BattleNpcSubKind BattleNPCKind => (BattleNpcSubKind)Struct->Character.GameObject.SubKind;

    public override ulong TargetObjectID => Struct->Character.TargetId;
}

internal unsafe class PlayerCharacter(nint address) : BattleChara(address), IPlayerCharacter
{
    public override ulong TargetObjectID => Struct->LookAt.Controller.Params[0].TargetParam.TargetId;
}

public interface IGameObject : IEquatable<IGameObject>
{
    public SeString              Name             { get; }
    public ulong                 GameObjectID     { get; }
    public uint                  EntityID         { get; }
    public uint                  DataID           { get; }
    public uint                  OwnerID          { get; }
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
    public ulong                 TargetObjectID   { get; }
    public IGameObject?          TargetObject     { get; }
    public uint                  NamePlateIconID  { get; }
    public ushort                FateID           { get; }
    public float                 Scale            { get; }
    public float                 VfxScale         { get; }
    public ObjectTargetableFlags TargetableStatus { get; }
    public byte                  TargetStatus     { get; }
    public nint                  Address          { get; }
    public Vector3               DrawOffset       { get; }
    public float                 Height           { get; }
    public byte                  Sex              { get; }
    public VisibilityFlags       RenderFlags      { get; }
    public Vector3               DefaultPosition  { get; }
    public float                 DefaultRotation  { get; }

    public bool IsValid();

    public unsafe CSGameObject* ToStruct();
    
    public unsafe CSBattleChara* ToBCStruct();
    
    public static IGameObject Create(nint address) => new GameObject(address);
}

public interface ICharacter : IGameObject
{
    public short                TransformationID    { get; }
    public float                ModelScale          { get; }
    public int                  ModelCharaID        { get; }
    public int                  ModelSkeletonID     { get; }
    public uint                 CurrentHp           { get; }
    public uint                 MaxHp               { get; }
    public uint                 CurrentMp           { get; }
    public uint                 MaxMp               { get; }
    public uint                 CurrentGp           { get; }
    public uint                 MaxGp               { get; }
    public uint                 CurrentCp           { get; }
    public uint                 MaxCp               { get; }
    public ushort               TitleID             { get; }
    public byte                 Icon                { get; }
    public byte                 ENPCMap             { get; }
    public BattalionFlags       Battalion           { get; }
    public byte                 ShieldPercentage    { get; }
    public RowRef<ClassJob>     ClassJob            { get; }
    public byte                 Level               { get; }
    public byte[]               Customize           { get; }
    public SeString             CompanyTag          { get; }
    public float                Alpha               { get; }
    public uint                 NameID              { get; }
    public ulong                AccountID           { get; }
    public ulong                ContentID           { get; }
    public CharacterModes       Mode                { get; }
    public byte                 ModeParam           { get; }
    public RowRef<OnlineStatus> OnlineStatus        { get; }
    public StatusFlags          StatusFlags         { get; }
    public RowRef<Emote>?       CurrentEmote        { get; }
    public ulong                EmoteTargetObjectID { get; }
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
    
    public new static ICharacter Create(nint address) => new Character(address);
}

public interface IBattleChara : ICharacter
{
    public StatusList   StatusList          { get; }
    public bool         IsCasting           { get; }
    public bool         IsCastInterruptible { get; }
    public ActionType   CastActionType      { get; }
    public uint         CastActionID        { get; }
    public ulong        CastTargetObjectID  { get; }
    public IGameObject? CastTargetObject    { get; }
    public float        CurrentCastTime     { get; }
    public float        BaseCastTime        { get; }
    public float        TotalCastTime       { get; }
    
    public new unsafe CSBattleChara* ToStruct();
    
    public new static IBattleChara Create(nint address) => new BattleChara(address);
}

public interface IBattleNPC : IBattleChara
{
    BattleNpcSubKind BattleNPCKind { get; }
    
    public new static IBattleNPC Create(nint address) => new BattleNPC(address);
}

public interface IEventObj : IGameObject
{
    public new static IEventObj Create(nint address) => new EventObj(address);
}

public interface INPC : ICharacter
{
    public new static INPC Create(nint address) => new NPC(address);
}

public interface IPlayerCharacter : IBattleChara
{
    public new static IPlayerCharacter Create(nint address) => new PlayerCharacter(address);
}

public enum BattalionFlags : byte
{
    FriendOrMaelstrom = 0,
    TwinAdder = 1,
    ImmortalFlames = 2,
    Enemy = 4
}
