using System.Collections;
using System.Runtime.CompilerServices;
using Dalamud.Utility;
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

    public unsafe nint Address
    {
        get
        {
            ThreadSafety.AssertMainThread();
            return (nint)(&CSGameObjectManager.Instance()->Objects);
        }
    }

    public int Length => ObjectTableLength;

    public IPlayerCharacter? LocalPlayer => this[0] as IPlayerCharacter;

    public IGameObject? this[int index]
    {
        get
        {
            ThreadSafety.AssertMainThread();
            return index >= ObjectTableLength || index < 0 ? null : cachedObjectTable[index].Update();
        }
    }

    public IGameObject? SearchByID(ulong gameObjectID)
    {
        ThreadSafety.AssertMainThread();
        
        if (gameObjectID is 0)
            return null;

        foreach (var e in cachedObjectTable)
        {
            if (e.Update() is { } o && o.GameObjectID == gameObjectID)
                return o;
        }

        return null;
    }
    
    public unsafe IGameObject? SearchByID(ulong gameObjectID, Range range)
    {
        ThreadSafety.AssertMainThread();

        if (gameObjectID is 0)
            return null;

        var (offset, length) = range.GetOffsetAndLength(ObjectTableLength);
        foreach (ref readonly var e in cachedObjectTable.AsSpan(offset, length))
        {
            var obj = e.Address;
            if (obj != null && obj->GetGameObjectId() == gameObjectID)
                return e.Update();
        }

        return null;
    }

    public unsafe IGameObject? SearchByEntityID(uint entityID)
    {
        ThreadSafety.AssertMainThread();
        
        if (entityID is 0 or 0xE0000000)
            return null;

        foreach (ref readonly var e in cachedObjectTable.AsSpan())
        {
            var obj = e.Address;
            if (obj != null && obj->EntityId == entityID)
                return e.Update();
        }

        return null;
    }

    public unsafe IGameObject? SearchByEntityID(uint entityID, Range range) 
    {
        ThreadSafety.AssertMainThread();

        if (entityID is 0 or 0xE0000000)
            return null;

        var (offset, length) = range.GetOffsetAndLength(ObjectTableLength);
        foreach (ref readonly var e in cachedObjectTable.AsSpan(offset, length))
        {
            var obj = e.Address;
            if (obj != null && obj->EntityId == entityID)
                return e.Update();
        }

        return null;
    }

    public unsafe nint GetObjectAddress(int index)
    {
        ThreadSafety.AssertMainThread();
        
        return index >= ObjectTableLength || index < 0 ? nint.Zero : (nint)cachedObjectTable[index].Address;
    }

    public unsafe IGameObject? CreateObjectReference(nint address)
    {
        ThreadSafety.AssertMainThread();
        
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

    public IEnumerable<IGameObject> SearchObjects(Predicate<IGameObject> predicate, Range range)
    {
        ThreadSafety.AssertMainThread();

        var (offset, length) = range.GetOffsetAndLength(ObjectTableLength);

        for (var i = 0; i < length; i++)
        {
            ref readonly var e = ref cachedObjectTable[offset + i];
            if (e.Update() is { } o && predicate(o))
                yield return o;
        }
    }

    public IEnumerable<IGameObject> SearchObjects(Predicate<IGameObject> predicate) => 
        SearchObjects(predicate, Range.All);

    public IGameObject? SearchObject(Predicate<IGameObject> predicate, Range range)
    {
        ThreadSafety.AssertMainThread();

        var (offset, length) = range.GetOffsetAndLength(ObjectTableLength);

        for (var i = 0; i < length; i++)
        {
            ref readonly var e = ref cachedObjectTable[offset + i];
            if (e.Update() is { } o && predicate(o))
                return o;
        }
        
        return null;
    }

    public IGameObject? SearchObject(Predicate<IGameObject> predicate) => 
        SearchObject(predicate, Range.All);

    internal readonly unsafe struct CachedEntry(Pointer<CSGameObject>* gameObjectPtr)
    {
        private readonly PlayerCharacter playerCharacter = new(nint.Zero);
        private readonly BattleNPC       battleNPC       = new(nint.Zero);
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
                ObjectKind.BattleNpc => battleNPC,
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
    public IEnumerator<IGameObject> GetEnumerator()
    {
        ThreadSafety.AssertMainThread();
        
        return new Enumerator(this);
    }

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
    public static readonly Range CharactersRange    = ..200;
    public static readonly Range ClientRange        = 200..449;
    public static readonly Range EventRange         = 449..489;
    public static readonly Range StandRange         = 489..629;
    public static readonly Range ReactionEventRange = 629..729;
    
    public nint Address { get; }
    
    public int Length { get; }
    
    public IPlayerCharacter? LocalPlayer { get; }

    public IGameObject? this[int index] { get; }

    public IGameObject? SearchByID(ulong gameObjectID);

    public IGameObject? SearchByID(ulong gameObjectID, Range range);

    public IGameObject? SearchByEntityID(uint entityID);

    public IGameObject? SearchByEntityID(uint entityID, Range range);

    public IEnumerable<IGameObject> SearchObjects(Predicate<IGameObject> predicate, Range range);

    public IEnumerable<IGameObject> SearchObjects(Predicate<IGameObject> predicate);

    public IGameObject? SearchObject(Predicate<IGameObject> predicate, Range range);

    public IGameObject? SearchObject(Predicate<IGameObject> predicate);

    public nint GetObjectAddress(int index);

    public IGameObject? CreateObjectReference(nint address);
}
