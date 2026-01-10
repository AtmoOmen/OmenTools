using System.Collections;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.Interop;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using CSGameObjectManager = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace OmenTools.Service;

internal sealed partial class ObjectTable : IObjectTable
{
    private static int ObjectTableLength;

    private readonly CachedEntry[] cachedObjectTable;

    internal unsafe ObjectTable()
    {
        var nativeObjectTable = CSGameObjectManager.Instance()->Objects.IndexSorted;
        ObjectTableLength = nativeObjectTable.Length;

        cachedObjectTable = new CachedEntry[ObjectTableLength];
        for (var i = 0; i < cachedObjectTable.Length; i++)
            cachedObjectTable[i] = new(nativeObjectTable.GetPointer(i));
    }

    public unsafe nint Address => (nint)(&CSGameObjectManager.Instance()->Objects);

    public int Length => ObjectTableLength;

    public IPlayerCharacter? LocalPlayer => this[0] as IPlayerCharacter;

    public IEnumerable<IBattleChara> PlayerObjects => GetPlayerObjects();

    public IEnumerable<IGameObject> CharacterManagerObjects => GetObjectsInRange(..199);

    public IEnumerable<IGameObject> ClientObjects => GetObjectsInRange(200..448);

    public IEnumerable<IGameObject> EventObjects => GetObjectsInRange(449..488);

    public IEnumerable<IGameObject> StandObjects => GetObjectsInRange(489..628);

    public IEnumerable<IGameObject> ReactionEventObjects => GetObjectsInRange(629..728);

    public IGameObject? this[int index] => 
        index >= ObjectTableLength || index < 0 ? null : cachedObjectTable[index].Update();

    public IGameObject? SearchByID(ulong gameObjectID)
    {
        if (gameObjectID is 0)
            return null;

        foreach (var e in cachedObjectTable)
        {
            if (e.Update() is { } o && o.GameObjectID == gameObjectID)
                return o;
        }

        return null;
    }

    public IGameObject? SearchByEntityID(uint entityID)
    {
        if (entityID is 0 or 0xE0000000)
            return null;

        foreach (var e in cachedObjectTable)
        {
            if (e.Update() is { } o && o.EntityID == entityID)
                return o;
        }

        return null;
    }

    public unsafe nint GetObjectAddress(int index) => 
        index >= ObjectTableLength || index < 0 ? nint.Zero : (nint)cachedObjectTable[index].Address;

    public unsafe IGameObject? CreateObjectReference(nint address)
    {
        if (address == nint.Zero)
            return null;

        if (!DService.Instance().PlayerState.IsLoaded)
            return null;

        var obj = (CSGameObject*)address;
        var objKind = (ObjectKind)obj->ObjectKind;
        return objKind switch
        {
            ObjectKind.Player    => new PlayerCharacter(address),
            ObjectKind.BattleNpc => new BattleNPC(address),
            ObjectKind.EventNpc  => new NPC(address),
            ObjectKind.Retainer  => new NPC(address),
            ObjectKind.EventObj  => new EventObj(address),
            ObjectKind.Companion => new NPC(address),
            ObjectKind.MountType => new NPC(address),
            ObjectKind.Ornament  => new NPC(address),
            _                    => new GameObject(address),
        };
    }

    private IEnumerable<IBattleChara> GetPlayerObjects()
    {
        for (var index = 0; index < 200; index += 2)
        {
            if (this[index] is IBattleChara { ObjectKind: ObjectKind.Player } gameObject)
                yield return gameObject;
        }
    }

    private IEnumerable<IGameObject> GetObjectsInRange(Range range)
    {
        for (var index = range.Start.Value; index <= range.End.Value; index++)
        {
            if (this[index] is { } gameObject)
                yield return gameObject;
        }
    }

    internal readonly unsafe struct CachedEntry(Pointer<CSGameObject>* gameObjectPtr)
    {
        private readonly PlayerCharacter playerCharacter = new(nint.Zero);
        private readonly BattleNPC       BattleNPC       = new(nint.Zero);
        private readonly NPC             npc             = new(nint.Zero);
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
                ObjectKind.BattleNpc => BattleNPC,
                ObjectKind.EventNpc  => npc,
                ObjectKind.Retainer  => npc,
                ObjectKind.EventObj  => eventObj,
                ObjectKind.Companion => npc,
                ObjectKind.MountType => npc,
                ObjectKind.Ornament  => npc,
                _                    => gameObject,
            };
            
            activeObject.Address = (nint)address;
            return activeObject;
        }
    }
}

internal sealed partial class ObjectTable
{
    public IEnumerator<IGameObject> GetEnumerator() => 
        new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => 
        GetEnumerator();

    private struct Enumerator(ObjectTable owner) : IEnumerator<IGameObject>
    {
        private int index = -1;

        public IGameObject Current { get; private set; }

        object IEnumerator.Current => 
            Current;

        public bool MoveNext()
        {
            var cache = owner.cachedObjectTable.AsSpan();

            while (++index < ObjectTableLength)
            {
                if (cache[index].Update() is { } ao)
                {
                    Current = ao;
                    return true;
                }
            }

            Current = null;
            return false;
        }

        public void Reset() => 
            index = -1;

        public void Dispose() { }
    }
}

public interface IObjectTable : IEnumerable<IGameObject>
{
    public nint Address { get; }
    
    public int Length { get; }

    public IPlayerCharacter? LocalPlayer { get; }

    public IEnumerable<IBattleChara> PlayerObjects { get; }

    public IEnumerable<IGameObject> CharacterManagerObjects { get; }

    public IEnumerable<IGameObject> ClientObjects { get; }

    public IEnumerable<IGameObject> EventObjects { get; }

    public IEnumerable<IGameObject> StandObjects { get; }
    
    public IEnumerable<IGameObject> ReactionEventObjects { get; }

    public IGameObject? this[int index] { get; }

    public IGameObject? SearchByID(ulong gameObjectID);

    public IGameObject? SearchByEntityID(uint entityID);

    public nint GetObjectAddress(int index);

    public IGameObject? CreateObjectReference(nint address);
}
