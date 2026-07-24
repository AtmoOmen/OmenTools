using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using OmenTools.Dalamud.Services.Game.Object.Enums;
using Companion = Lumina.Excel.Sheets.Companion;
using Ornament = Lumina.Excel.Sheets.Ornament;
using CSCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using CharacterModes = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterModes;
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

namespace OmenTools.Dalamud.Services.Game.Object.ObjectKinds;

internal unsafe class Character
(
    nint address
) : GameObject(address), ICharacter
{
    protected internal new CSCharacter* Struct => (CSCharacter*)Address;

    public IBattleChara? ContainerOwner => DService.Instance().ObjectTable.CreateObjectReference((nint)Struct->MoveController.OwnerObject) as IBattleChara;

    public short            TransformationID => Struct->TransformationId;
    public ushort           StatusLoopVfxID => Struct->CharacterData.StatusLoopVfxId;
    public byte             SEPack => Struct->CharacterData.SEPack;
    public float            ModelScale => Struct->CharacterData.ModelScale;
    public int              ModelCharaID => Struct->ModelContainer.ModelCharaId;
    public int              ModelSkeletonID => Struct->ModelContainer.ModelSkeletonId;
    public uint             CurrentHp => Struct->CharacterData.Health;
    public uint             MaxHp => Struct->CharacterData.MaxHealth;
    public uint             CurrentMp => Struct->CharacterData.Mana;
    public uint             MaxMp => Struct->CharacterData.MaxMana;
    public uint             CurrentGp => Struct->CharacterData.GatheringPoints;
    public uint             MaxGp => Struct->CharacterData.MaxGatheringPoints;
    public uint             CurrentCp => Struct->CharacterData.CraftingPoints;
    public uint             MaxCp => Struct->CharacterData.MaxCraftingPoints;
    public ushort           TitleID => Struct->CharacterData.TitleId;
    public byte             Icon => Struct->CharacterData.Icon;
    public byte             ENPCMap => Struct->CharacterData.Map;
    public BattalionFlags   Battalion => (BattalionFlags)Struct->CharacterData.Battalion;
    public byte             ShieldPercentage => Struct->CharacterData.ShieldValue;
    public byte             CharacterFlags => Struct->CharacterData.Flags;
    public byte             CombatTagType => Struct->CharacterData.CombatTagType;
    public ulong            CombatTaggerObjectID => Struct->CharacterData.CombatTaggerId;
    public RowRef<ClassJob> ClassJob => Struct->CharacterData.ClassJob.ToLuminaRowRef<ClassJob>();
    public byte             Level => Struct->CharacterData.Level;
    public byte[]           Customize => Struct->DrawData.CustomizeData.Data.ToArray();
    public string           CompanyTag => Struct->FreeCompanyTagString;
    public float            Alpha => Struct->Alpha;
    public uint             NameID => Struct->NameId;
    public ulong            AccountID => Struct->AccountId;
    public ulong            ContentID => Struct->ContentId;
    public ulong            SoftTargetObjectID => Struct->GetSoftTargetId();
    public IGameObject?     SoftTargetObject => DService.Instance().ObjectTable.SearchByID(SoftTargetObjectID);
    public CharacterModes   Mode => Struct->Mode;
    public byte             ModeParam => Struct->ModeParam;
    public byte             WeaponFlags => Struct->WeaponFlags;
    public bool             IsWeaponDrawn => Struct->IsWeaponDrawn;
    public bool             IsSwimming => Struct->MoveController.IsSwimming;
    public bool             IsMounted => Struct->IsMounted();
    public bool             IsInPvP => Struct->IsInPvP();
    public uint             EventNPCInstanceID => Struct->EventNpcInstanceId;
    public byte             RelationFlags => Struct->RelationFlags;
    public byte             ActorControlFlags => Struct->ActorControlFlags;
    public float            CastRotation => Struct->CastRotation;
    public ICharacter?      ChildObject => DService.Instance().ObjectTable.CreateObjectReference((nint)Struct->ChildObject) as ICharacter;
    public uint             CompanionOwnerID => Struct->CompanionOwnerId;
    public ObjectType       ObjectType => Struct->ObjectType;
    public byte             GMRank => Struct->GMRank;
    public byte             SoundVolumeCategory => Struct->SoundVolumeCategory;
    public byte             SoundVolumeCategoryOverride => Struct->SoundVolumeCategoryOverride;
    public ushort           MountID => Struct->Mount.MountId;
    public float            DismountTimer => Struct->Mount.DismountTimer;
    public byte             MountFlags => Struct->Mount.Flags;
    public ICharacter?      MountObject => DService.Instance().ObjectTable.CreateObjectReference((nint)Struct->Mount.MountObject) as ICharacter;
    public ushort           CompanionID => Struct->CompanionData.CompanionId;
    public ushort           FollowMountID => Struct->CompanionData.FollowMountId;
    public ICharacter?      CompanionObject => DService.Instance().ObjectTable.CreateObjectReference((nint)Struct->CompanionData.CompanionObject) as ICharacter;
    public ushort           OrnamentID => Struct->OrnamentData.OrnamentId;
    public ICharacter?      OrnamentObject => DService.Instance().ObjectTable.CreateObjectReference((nint)Struct->OrnamentData.OrnamentObject) as ICharacter;
    public int              ModelCharaID2 => Struct->ModelContainer.ModelCharaId_2;
    public int              ModelSkeletonID2 => Struct->ModelContainer.ModelSkeletonId_2;
    public byte             ModelScaleID => Struct->ModelContainer.ModelScaleId;
    public byte             ModelAttributeFlags => Struct->ModelContainer.ModeAttributeFlags;
    public float            UnscaledRadius => Struct->ModelContainer.UnscaledRadius;
    public byte             RepresentationNameType => Struct->RepresentationContainer.NameType;

    public string NameOverride => Struct->RepresentationContainer.NameOverride == null ?
                                      string.Empty :
                                      Struct->RepresentationContainer.NameOverride->ToString();

    public float RepresentationUpdateTimer => Struct->RepresentationContainer.UpdateTimer;
    public ushort VoiceID => Struct->Vfx.VoiceId;
    public byte TimelineModelState => Struct->Timeline.ModelState;
    public float OverallSpeed => Struct->Timeline.OverallSpeed;
    public ushort BaseOverride => Struct->Timeline.BaseOverride;
    public ushort LipsOverride => Struct->Timeline.LipsOverride;
    public ushort BannerTimelineRowID => Struct->Timeline.BannerTimelineRowId;
    public byte BannerFacialRowID => Struct->Timeline.BannerFacialRowId;
    public uint BannerTimelineNameOffset => Struct->Timeline.BannerTimelineNameOffset;
    public uint BannerTimelineAdditionalData => Struct->Timeline.BannerTimelineAdditionalData;
    public int BannerTimelineIcon => Struct->Timeline.BannerTimelineIcon;
    public ushort BannerTimelineUnlockCondition => Struct->Timeline.BannerTimelineUnlockCondition;
    public ushort BannerTimelineSortKey => Struct->Timeline.BannerTimelineSortKey;
    public byte BannerTimelineType => Struct->Timeline.BannerTimelineType;
    public byte BannerTimelineAcceptClassJobCategory => Struct->Timeline.BannerTimelineAcceptClassJobCategory;
    public byte BannerTimelineCategory => Struct->Timeline.BannerTimelineCategory;
    public float BannerRequestStartTimestamp => Struct->Timeline.BannerRequestStartTimestamp;
    public Vector3 CameraVector => Struct->LookAt.CameraVector;
    public bool IsFacingCamera => Struct->LookAt.IsFacingCamera;
    public Vector2 BannerHeadDirection => Struct->LookAt.BannerHeadDirection;
    public Vector2 BannerEyeDirection => Struct->LookAt.BannerEyeDirection;
    public BannerCameraFollowFlags BannerCameraFollowFlag => Struct->LookAt.BannerCameraFollowFlag;
    public bool IsHatHidden => Struct->DrawData.IsHatHidden;
    public bool IsWeaponHidden => Struct->DrawData.IsWeaponHidden;
    public bool IsVisorToggled => Struct->DrawData.IsVisorToggled;
    public bool VieraEarsHidden => Struct->DrawData.VieraEarsHidden;
    public byte FreeCompanyCrestBitfield => Struct->DrawData.FreeCompanyCrestBitfield;
    public ulong FreeCompanyCrestDataValue => Struct->DrawData.FreeCompanyCrestData.Data;
    public byte FreeCompanyCrestCharge => Struct->DrawData.FreeCompanyCrestData.Charge;
    public byte FreeCompanyCrestOrdinaryTinctures => Struct->DrawData.FreeCompanyCrestData.OrdinaryTinctures;
    public byte ReaperStanceChangeID => Struct->ReaperShroud.StanceChangeId;
    public uint ReaperStanceChangeState => Struct->ReaperShroud.StanceChangeState;
    public float ReaperTimer => Struct->ReaperShroud.Timer;
    public ICharacter? ReaperCopyObject => DService.Instance().ObjectTable.CreateObjectReference((nint)Struct->ReaperShroud.CopyObject) as ICharacter;
    public ShroudFlags ReaperFlags => Struct->ReaperShroud.Flags;
    public ushort ReaperNPCEquipID => Struct->ReaperShroud.NpcEquipId;
    public float CurrentFloatHeight => Struct->Effects.CurrentFloatHeight;
    public float TargetFloatHeight => Struct->Effects.TargetFloatHeight;
    public float FloatHeightChangeSpeed => Struct->Effects.FloatHeightChangeSpeed;
    public StatusEffect StatusEffects => Struct->Effects.StatusEffects;
    public int MountTiltSetupState1 => Struct->Effects.MountTiltSetupState1;
    public int MountTiltSetupState2 => Struct->Effects.MountTiltSetupState2;
    public TiltOrigin MountGroundTiltOrigin => Struct->Effects.MountGroundTiltOrigin;
    public float MountGroundTiltAngle => Struct->Effects.MountGroundTiltAngle;
    public float MountGroundTiltSpeed => Struct->Effects.MountGroundTiltSpeed;
    public TiltFlags MountGroundTiltFlags => Struct->Effects.MountGroundTiltFlags;
    public TiltOrigin MountFlightSwimTiltOrigin => Struct->Effects.MountFlightSwimTiltOrigin;
    public float MountFlightSwimTiltAngle => Struct->Effects.MountFlightSwimTiltAngle;
    public float MountFlightSwimTiltSpeed => Struct->Effects.MountFlightSwimTiltSpeed;
    public TiltFlags MountFlightSwimTiltFlags => Struct->Effects.MountFlightSwimTiltFlags;
    public TiltOrigin RiderGroundTiltOrigin => Struct->Effects.RiderGroundTiltOrigin;
    public float RiderGroundTiltAngle => Struct->Effects.RiderGroundTiltAngle;
    public float RiderGroundTiltSpeed => Struct->Effects.RiderGroundTiltSpeed;
    public TiltFlags RiderGroundTiltFlags => Struct->Effects.RiderGroundTiltFlags;
    public TiltOrigin RiderFlightSwimTiltOrigin => Struct->Effects.RiderFlightSwimTiltOrigin;
    public float RiderFlightSwimTiltAngle => Struct->Effects.RiderFlightSwimTiltAngle;
    public float RiderFlightSwimTiltSpeed => Struct->Effects.RiderFlightSwimTiltSpeed;
    public TiltFlags RiderFlightSwimReverseTilt => Struct->Effects.RiderFlightSwimReverseTilt;
    public ushort BalloonDefaultID => Struct->Balloon.DefaultBalloonId;
    public ushort BalloonCurrentID => Struct->Balloon.NowPlayingBalloonId;
    public float BalloonPlayTimer => Struct->Balloon.PlayTimer;
    public BalloonType BalloonType => Struct->Balloon.Type;
    public BalloonState BalloonState => Struct->Balloon.State;
    public float NPCYellPlayTimer => Struct->YellBalloon.PlayTimer;
    public float NPCYellDelayTime => Struct->YellBalloon.DelayTime;
    public NPCYellBalloonState NPCYellState => Struct->YellBalloon.State;
    public NPCYellBalloonCloseType NPCYellCloseType => Struct->YellBalloon.CloseType;
    public byte NPCYellParentBone => Struct->YellBalloon.ParentBone;
    public NPCYellBalloonFlags NPCYellFlags => Struct->YellBalloon.Flags;
    public ICharacter? NPCYellCharacter => DService.Instance().ObjectTable.CreateObjectReference((nint)Struct->YellBalloon.Character) as ICharacter;
    public RowRef<OnlineStatus> OnlineStatus => Struct->CharacterData.OnlineStatus.ToLuminaRowRef<OnlineStatus>();
    public ulong EmoteTargetObjectID => Struct->EmoteController.Target;
    public IGameObject? EmoteTargetObject => DService.Instance().ObjectTable.SearchByID(EmoteTargetObjectID);
    public IBattleChara? EmoteOwnerObject => DService.Instance().ObjectTable.CreateObjectReference((nint)Struct->EmoteController.OwnerObject) as IBattleChara;
    public byte EmoteStance => Struct->EmoteController.Stance;
    public PoseType CurrentPoseType => Struct->EmoteController.CurrentPoseType;
    public byte CPoseState => Struct->EmoteController.CPoseState;
    public bool IsEmoting => Struct->EmoteController.IsEmoting();
    public bool IsInEmoteLoop => Struct->EmoteController.IsInEmoteLoop();
    public bool IsWanderer => Struct->IsWanderer();
    public bool IsTraveler => Struct->IsTraveler();
    public bool IsVoyager => Struct->IsVoyager();
    public RowRef<World> CurrentWorld => Struct->CurrentWorld.ToLuminaRowRef<World>();
    public RowRef<World> HomeWorld => Struct->HomeWorld.ToLuminaRowRef<World>();

    public override ulong TargetObjectID => Struct->TargetId;

    public StatusFlags StatusFlags =>
        (Struct->IsHostile ?
             StatusFlags.Hostile :
             StatusFlags.None) |
        (Struct->InCombat ?
             StatusFlags.InCombat :
             StatusFlags.None) |
        (Struct->IsWeaponDrawn ?
             StatusFlags.WeaponOut :
             StatusFlags.None) |
        (Struct->IsOffhandDrawn ?
             StatusFlags.OffhandOut :
             StatusFlags.None) |
        (Struct->IsPartyMember ?
             StatusFlags.PartyMember :
             StatusFlags.None) |
        (Struct->IsAllianceMember ?
             StatusFlags.AllianceMember :
             StatusFlags.None) |
        (Struct->IsFriend ?
             StatusFlags.Friend :
             StatusFlags.None) |
        (Struct->IsCasting ?
             StatusFlags.IsCasting :
             StatusFlags.None);

    public RowRef<Emote>? CurrentEmote
    {
        get
        {
            var emoteID = Struct->EmoteController.EmoteId;
            return emoteID == 0 ?
                       null :
                       emoteID.ToLuminaRowRef<Emote>();
        }
    }

    public RowRef<Mount>? CurrentMount
    {
        get
        {
            if (Struct->IsNotMounted()) return null;

            var mountID = Struct->Mount.MountId;
            return mountID == 0 ?
                       null :
                       mountID.ToLuminaRowRef<Mount>();
        }
    }

    public RowRef<Ornament>? CurrentOrnament
    {
        get
        {
            var ornamentID = Struct->OrnamentData.OrnamentId;
            return ornamentID == 0 ?
                       null :
                       ornamentID.ToLuminaRowRef<Ornament>();
        }
    }

    public RowRef<Companion>? CurrentMinion
    {
        get
        {
            if (Struct->ChildObject != null)
                return Struct->ChildObject->BaseId.ToLuminaRowRef<Companion>();

            var hiddenCompanionID = Struct->CompanionData.CompanionId;
            return hiddenCompanionID == 0 ?
                       null :
                       hiddenCompanionID.ToLuminaRowRef<Companion>();
        }
    }

    public new CSCharacter* ToStruct() => Struct;
}
