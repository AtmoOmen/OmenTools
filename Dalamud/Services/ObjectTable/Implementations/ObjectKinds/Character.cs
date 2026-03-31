using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using OmenTools.Dalamud.Services.ObjectTable.Enums;
using Companion = Lumina.Excel.Sheets.Companion;
using Ornament = Lumina.Excel.Sheets.Ornament;

namespace OmenTools.Dalamud.Services.ObjectTable.ObjectKinds;

internal unsafe class Character
(
    nint address
) : GameObject(address), ICharacter
{
    protected internal new FFXIVClientStructs.FFXIV.Client.Game.Character.Character* Struct => (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)Address;
    public                 short TransformationID => Struct->TransformationId;
    public                 float ModelScale => Struct->CharacterData.ModelScale;
    public                 int ModelCharaID => Struct->ModelContainer.ModelCharaId;
    public                 int ModelSkeletonID => Struct->ModelContainer.ModelSkeletonId;
    public                 uint CurrentHp => Struct->CharacterData.Health;
    public                 uint MaxHp => Struct->CharacterData.MaxHealth;
    public                 uint CurrentMp => Struct->CharacterData.Mana;
    public                 uint MaxMp => Struct->CharacterData.MaxMana;
    public                 uint CurrentGp => Struct->CharacterData.GatheringPoints;
    public                 uint MaxGp => Struct->CharacterData.MaxGatheringPoints;
    public                 uint CurrentCp => Struct->CharacterData.CraftingPoints;
    public                 uint MaxCp => Struct->CharacterData.MaxCraftingPoints;
    public                 ushort TitleID => Struct->CharacterData.TitleId;
    public                 byte Icon => Struct->CharacterData.Icon;
    public                 byte ENPCMap => Struct->CharacterData.Map;
    public                 BattalionFlags Battalion => (BattalionFlags)Struct->CharacterData.Battalion;
    public                 byte ShieldPercentage => Struct->CharacterData.ShieldValue;
    public                 RowRef<ClassJob> ClassJob => Struct->CharacterData.ClassJob.ToLuminaRowRef<ClassJob>();
    public                 byte Level => Struct->CharacterData.Level;
    public                 byte[] Customize => Struct->DrawData.CustomizeData.Data.ToArray();
    public                 SeString CompanyTag => SeString.Parse(Struct->FreeCompanyTag);
    public                 float Alpha => Struct->Alpha;
    public                 uint NameID => Struct->NameId;
    public                 ulong AccountID => Struct->AccountId;
    public                 ulong ContentID => Struct->ContentId;
    public                 CharacterModes Mode => Struct->Mode;
    public                 byte ModeParam => Struct->ModeParam;
    public                 RowRef<OnlineStatus> OnlineStatus => Struct->CharacterData.OnlineStatus.ToLuminaRowRef<OnlineStatus>();
    public                 ulong EmoteTargetObjectID => Struct->EmoteController.Target;
    public                 IGameObject? EmoteTargetObject => DService.Instance().ObjectTable.SearchByID(EmoteTargetObjectID);
    public                 bool IsWanderer => Struct->IsWanderer();
    public                 bool IsTraveler => Struct->IsTraveler();
    public                 bool IsVoyager => Struct->IsVoyager();
    public                 RowRef<World> CurrentWorld => Struct->CurrentWorld.ToLuminaRowRef<World>();
    public                 RowRef<World> HomeWorld => Struct->HomeWorld.ToLuminaRowRef<World>();

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

    public new FFXIVClientStructs.FFXIV.Client.Game.Character.Character* ToStruct() => Struct;
}
