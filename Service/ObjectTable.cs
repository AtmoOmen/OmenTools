using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.Interop;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using CSCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using CSBattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;
using CSGameObjectManager = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager;
using Status = Dalamud.Game.ClientState.Statuses.Status;

namespace OmenTools.Service;

internal sealed partial class ObjectTable : IObjectTable
{
    private static int objectTableLength;

    private readonly CachedEntry[] cachedObjectTable;

    private readonly Enumerator?[] frameworkThreadEnumerators = new Enumerator?[4];

    internal unsafe ObjectTable()
    {
        var nativeObjectTable = CSGameObjectManager.Instance()->Objects.IndexSorted;
        objectTableLength = nativeObjectTable.Length;

        cachedObjectTable = new CachedEntry[objectTableLength];
        for (var i = 0; i < cachedObjectTable.Length; i++)
            cachedObjectTable[i] = new(nativeObjectTable.GetPointer(i));

        for (var i = 0; i < frameworkThreadEnumerators.Length; i++)
            frameworkThreadEnumerators[i] = new(this, i);
    }

    public unsafe nint Address
    {
        get => (nint)(&CSGameObjectManager.Instance()->Objects);
    }

    public int Length => objectTableLength;

    public IGameObject? this[int index]
    {
        get => index >= objectTableLength || index < 0 ? null : cachedObjectTable[index].Update();
    }

    public IGameObject? SearchById(ulong gameObjectId)
    {
        if (gameObjectId is 0)
            return null;

        foreach (var e in cachedObjectTable)
            if (e.Update() is { } o && o.GameObjectId == gameObjectId)
                return o;

        return null;
    }

    public IGameObject? SearchByEntityId(uint entityId)
    {
        if (entityId is 0 or 0xE0000000)
            return null;

        foreach (var e in cachedObjectTable)
            if (e.Update() is { } o && o.EntityId == entityId)
                return o;

        return null;
    }

    public unsafe nint GetObjectAddress(int index) =>
        index >= objectTableLength || index < 0 ? nint.Zero : (nint)cachedObjectTable[index].Address;

    public unsafe IGameObject? CreateObjectReference(nint address)
    {
        if (DService.ClientState.LocalContentId == 0)
            return null;

        if (address == nint.Zero)
            return null;

        var obj     = (CSGameObject*)address;
        var objKind = (ObjectKind)obj->ObjectKind;
        return objKind switch
        {
            ObjectKind.Player    => new PlayerCharacter(address),
            ObjectKind.BattleNpc => new BattleNpc(address),
            ObjectKind.EventNpc  => new Npc(address),
            ObjectKind.Retainer  => new Npc(address),
            ObjectKind.EventObj  => new EventObj(address),
            ObjectKind.Companion => new Npc(address),
            ObjectKind.MountType => new Npc(address),
            ObjectKind.Ornament  => new Npc(address),
            _                    => new GameObject(address)
        };
    }
    
    public IPlayerCharacter? LocalPlayer => this[0] as IPlayerCharacter;
    
    public IEnumerator<IGameObject> GetEnumerator()
    {
        foreach (ref var x in frameworkThreadEnumerators.AsSpan())
            if (x is not null)
            {
                var t = x;
                x = null;
                t.Reset();
                return t;
            }

        return new Enumerator(this, -1);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Enumerator(ObjectTable owner, int slotId) : IEnumerator<IGameObject>
    {
        private readonly ObjectTable? owner = owner;

        private int index = -1;

        public IGameObject Current { get; private set; } = null!;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (index == objectTableLength)
                return false;

            var cache = owner!.cachedObjectTable.AsSpan();
            for (index++; index < objectTableLength; index++)
                if (cache[index].Update() is { } ao)
                {
                    Current = ao;
                    return true;
                }

            return false;
        }

        public void Reset() => index = -1;

        public void Dispose()
        {
            if (owner is not { } o)
                return;

            if (slotId != -1)
                o.frameworkThreadEnumerators[slotId] = this;
        }

        public bool TryReset()
        {
            Reset();
            return true;
        }
    }
}

internal sealed partial class ObjectTable
{
    internal readonly unsafe struct CachedEntry(Pointer<CSGameObject>* gameObjectPtr)
    {
        private readonly PlayerCharacter playerCharacter = new(nint.Zero);
        private readonly BattleNpc       battleNpc       = new(nint.Zero);
        private readonly Npc             npc             = new(nint.Zero);
        private readonly EventObj        eventObj        = new(nint.Zero);
        private readonly GameObject      gameObject      = new(nint.Zero);

        public CSGameObject* Address
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => gameObjectPtr->Value;
        }

        public GameObject? Update()
        {
            var address = Address;
            if (address is null)
                return null;

            var activeObject = (ObjectKind)address->ObjectKind switch
            {
                ObjectKind.Player    => playerCharacter,
                ObjectKind.BattleNpc => battleNpc,
                ObjectKind.EventNpc  => npc,
                ObjectKind.Retainer  => npc,
                ObjectKind.EventObj  => eventObj,
                ObjectKind.Companion => npc,
                ObjectKind.MountType => npc,
                ObjectKind.Ornament  => npc,
                _                    => gameObject
            };
            activeObject.Address = (nint)address;
            return activeObject;
        }
    }
    
    internal partial class GameObject
    {
        internal GameObject(nint address) => Address = address;

        public nint Address { get; internal set; }

        public static implicit operator bool(GameObject? gameObject) => IsValid(gameObject);

        public static bool operator ==(GameObject? gameObject1, GameObject? gameObject2)
        {
            if (gameObject1 is null || gameObject2 is null)
                return Equals(gameObject1, gameObject2);

            return gameObject1.Equals(gameObject2);
        }

        public static bool operator !=(GameObject? actor1, GameObject? actor2) => !(actor1 == actor2);

        public static bool IsValid(IGameObject? actor)
        {
            var clientState = DService.ClientState;

            if (actor is null || clientState == null)
                return false;

            if (clientState.LocalContentId == 0)
                return false;

            return true;
        }

        public bool IsValid() => IsValid(this);

        bool IEquatable<IGameObject>.Equals(IGameObject? other) => GameObjectId == other?.GameObjectId;

        public override bool Equals(object? obj) => ((IEquatable<IGameObject>)this).Equals(obj as IGameObject);

        public override int GetHashCode() => GameObjectId.GetHashCode();
    }

    internal unsafe partial class GameObject : IGameObject
    {
        public SeString Name => SeString.Parse(Struct->Name);

        public ulong GameObjectId => Struct->GetGameObjectId();

        public uint EntityId => Struct->EntityId;

        public uint DataId => Struct->BaseId;

        public uint OwnerId => Struct->OwnerId;

        public ushort ObjectIndex => Struct->ObjectIndex;

        public ObjectKind ObjectKind => (ObjectKind)Struct->ObjectKind;

        public byte SubKind => Struct->SubKind;

        public byte YalmDistanceX => Struct->YalmDistanceFromPlayerX;

        public byte YalmDistanceZ => Struct->YalmDistanceFromPlayerZ;

        public bool IsDead => Struct->IsDead();

        public bool IsTargetable => Struct->GetIsTargetable();

        public Vector3 Position => new(Struct->Position.X, Struct->Position.Y, Struct->Position.Z);

        public float Rotation => Struct->Rotation;

        public float HitboxRadius => Struct->HitboxRadius;

        public virtual ulong TargetObjectId => 0;

        public virtual IGameObject? TargetObject => DService.ObjectTable.SearchById(TargetObjectId);

        protected internal CSGameObject* Struct => (CSGameObject*)Address;

        public override string ToString() => $"{GameObjectId:X}({Name.TextValue} - {ObjectKind}) at {Address:X}";
    }

    internal class EventObj : GameObject, IEventObj
    {
        internal EventObj(nint address) : base(address) { }
    }

    internal unsafe class Character : GameObject, ICharacter
    {
        internal Character(nint address) : base(address) { }

        public uint CurrentHp => Struct->CharacterData.Health;

        public uint MaxHp => Struct->CharacterData.MaxHealth;

        public uint CurrentMp => Struct->CharacterData.Mana;

        public uint MaxMp => Struct->CharacterData.MaxMana;

        public uint CurrentGp => Struct->CharacterData.GatheringPoints;

        public uint MaxGp => Struct->CharacterData.MaxGatheringPoints;

        public uint CurrentCp => Struct->CharacterData.CraftingPoints;

        public uint MaxCp => Struct->CharacterData.MaxCraftingPoints;

        public byte ShieldPercentage => Struct->CharacterData.ShieldValue;

        public RowRef<ClassJob> ClassJob => CreateRef<ClassJob>(Struct->CharacterData.ClassJob);

        public byte Level => Struct->CharacterData.Level;

        public byte[] Customize => Struct->DrawData.CustomizeData.Data.ToArray();

        public SeString CompanyTag => SeString.Parse(Struct->FreeCompanyTag);

        public override ulong TargetObjectId => Struct->TargetId;

        public uint NameId => Struct->NameId;

        public RowRef<OnlineStatus> OnlineStatus => CreateRef<OnlineStatus>(Struct->CharacterData.OnlineStatus);
        
        public StatusFlags StatusFlags =>
            (Struct->IsHostile ? StatusFlags.Hostile : StatusFlags.None)               |
            (Struct->InCombat ? StatusFlags.InCombat : StatusFlags.None)               |
            (Struct->IsWeaponDrawn ? StatusFlags.WeaponOut : StatusFlags.None)         |
            (Struct->IsOffhandDrawn ? StatusFlags.OffhandOut : StatusFlags.None)       |
            (Struct->IsPartyMember ? StatusFlags.PartyMember : StatusFlags.None)       |
            (Struct->IsAllianceMember ? StatusFlags.AllianceMember : StatusFlags.None) |
            (Struct->IsFriend ? StatusFlags.Friend : StatusFlags.None)                 |
            (Struct->IsCasting ? StatusFlags.IsCasting : StatusFlags.None);

        public RowRef<Mount>? CurrentMount
        {
            get
            {
                if (Struct->IsNotMounted()) return null;

                var mountId = Struct->Mount.MountId;
                return mountId == 0 ? null : CreateRef<Mount>(mountId);
            }
        }

        public RowRef<Companion>? CurrentMinion
        {
            get
            {
                if (Struct->CompanionObject != null)
                    return CreateRef<Companion>(Struct->CompanionObject->BaseId);

                var hiddenCompanionId = Struct->CompanionData.CompanionId;
                return hiddenCompanionId == 0 ? null : CreateRef<Companion>(hiddenCompanionId);
            }
        }
        
        protected internal new CSCharacter* Struct => (CSCharacter*)Address;
    }

    internal class Npc : Character, INpc
    {
        internal Npc(nint address) : base(address) { }
    }

    internal unsafe class BattleChara : Character, IBattleChara
    {
        internal BattleChara(nint address) : base(address) { }

        public StatusList StatusList => new(this.Struct->GetStatusManager());

        public bool IsCasting => Struct->GetCastInfo()->IsCasting > 0;

        public bool IsCastInterruptible => Struct->GetCastInfo()->Interruptible > 0;

        public byte CastActionType => (byte)Struct->GetCastInfo()->ActionType;

        public uint CastActionId => Struct->GetCastInfo()->ActionId;

        public ulong CastTargetObjectId => Struct->GetCastInfo()->TargetId;

        public float CurrentCastTime => Struct->GetCastInfo()->CurrentCastTime;

        public float BaseCastTime => Struct->GetCastInfo()->BaseCastTime;

        public float TotalCastTime => Struct->GetCastInfo()->TotalCastTime;

        protected new CSBattleChara* Struct => (CSBattleChara*)Address;
    }

    internal unsafe class BattleNpc : BattleChara, IBattleNpc
    {
        internal BattleNpc(nint address) : base(address) { }

        public BattleNpcSubKind BattleNpcKind => (BattleNpcSubKind)Struct->Character.GameObject.SubKind;

        public override ulong TargetObjectId => Struct->Character.TargetId;
    }

    internal unsafe class PlayerCharacter : BattleChara, IPlayerCharacter
    {
        internal PlayerCharacter(nint address) : base(address) { }

        public RowRef<World> CurrentWorld => CreateRef<World>(Struct->CurrentWorld);

        public RowRef<World> HomeWorld => CreateRef<World>(Struct->HomeWorld);

        public override ulong TargetObjectId => Struct->LookAt.Controller.Params[0].TargetParam.TargetId;
    }
}

public interface IObjectTable : IEnumerable<IGameObject>
{
    public nint Address { get; }
    
    public int Length { get; }

    public IGameObject? this[int index] { get; }

    public IGameObject? SearchById(ulong gameObjectId);

    public IGameObject? SearchByEntityId(uint entityId);

    public nint GetObjectAddress(int index);
    
    public IPlayerCharacter? LocalPlayer { get; }

    public IGameObject? CreateObjectReference(nint address);
}

public interface IGameObject : IEquatable<IGameObject>
{
    public SeString Name { get; }
    
    public ulong GameObjectId { get; }
    
    public uint EntityId { get; }

    public uint DataId { get; }

    public uint OwnerId { get; }

    public ushort ObjectIndex { get; }

    public ObjectKind ObjectKind { get; }

    public byte SubKind { get; }

    public byte YalmDistanceX { get; }

    public byte YalmDistanceZ { get; }

    public bool IsDead { get; }

    public bool IsTargetable { get; }

    public Vector3 Position { get; }

    public float Rotation { get; }

    public float HitboxRadius { get; }

    public ulong TargetObjectId { get; }

    public IGameObject? TargetObject { get; }

    public IntPtr Address { get; }

    public bool IsValid();
}

public interface ICharacter : IGameObject
{
    public uint CurrentHp { get; }

    public uint MaxHp { get; }
    
    public uint CurrentMp { get; }

    public uint MaxMp { get; }

    public uint CurrentGp { get; }

    public uint MaxGp { get; }

    public uint CurrentCp { get; }

    public uint MaxCp { get; }

    public byte ShieldPercentage { get; }

    public RowRef<ClassJob> ClassJob { get; }

    public byte Level { get; }
    
    public byte[] Customize { get; }
    
    public SeString CompanyTag { get; }
    
    public uint NameId { get; }
    
    public RowRef<OnlineStatus> OnlineStatus { get; }
    
    public StatusFlags StatusFlags { get; }
    
    public RowRef<Mount>? CurrentMount { get; }

    public RowRef<Companion>? CurrentMinion { get; }
}

public interface IEventObj : IGameObject;

public interface INpc : ICharacter { }

public interface IBattleChara : ICharacter
{
    public StatusList StatusList { get; }

    public bool IsCasting { get; }

    public bool IsCastInterruptible { get; }

    public byte CastActionType { get; }

    public uint CastActionId { get; }

    public ulong CastTargetObjectId { get; }

    public float CurrentCastTime { get; }

    public float BaseCastTime { get; }

    public float TotalCastTime { get; }
}

public interface IBattleNpc : IBattleChara
{
    BattleNpcSubKind BattleNpcKind { get; }
}

public interface IPlayerCharacter : IBattleChara
{
    RowRef<World> CurrentWorld { get; }

    RowRef<World> HomeWorld { get; }
}
