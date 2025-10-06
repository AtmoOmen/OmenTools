using System.Collections;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.Interop;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using CSGameObjectManager = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace OmenTools.Service;

internal sealed class ObjectTable : IObjectTable
{
    private static   int           objectTableLength;
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
            ObjectKind.BattleNpc => new BattleNPC(address),
            ObjectKind.EventNpc  => new NPC(address),
            ObjectKind.Retainer  => new NPC(address),
            ObjectKind.EventObj  => new EventObj(address),
            ObjectKind.Companion => new NPC(address),
            ObjectKind.MountType => new NPC(address),
            ObjectKind.Ornament  => new NPC(address),
            _                    => new GameObject(address)
        };
    }
    
    public IPlayerCharacter? LocalPlayer => this[0] as IPlayerCharacter;
    
    public IEnumerator<IGameObject> GetEnumerator()
    {
        foreach (ref var x in frameworkThreadEnumerators.AsSpan())
        {
            if (x is not null)
            {
                var t = x;
                x = null;
                t.Reset();
                return t;
            }
        }

        return new Enumerator(this, -1);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Enumerator(ObjectTable owner, int slotID) : IEnumerator<IGameObject>
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
            {
                if (cache[index].Update() is { } ao)
                {
                    Current = ao;
                    return true;
                }
            }

            return false;
        }

        public void Reset() => index = -1;

        public void Dispose()
        {
            if (owner is not { } o)
                return;

            if (slotID != -1)
                o.frameworkThreadEnumerators[slotID] = this;
        }

        public bool TryReset()
        {
            Reset();
            return true;
        }
    }
    
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
                _                    => gameObject
            };
            activeObject.Address = (nint)address;
            return activeObject;
        }
    }
}

public interface IObjectTable : IEnumerable<IGameObject>
{
    public IGameObject? this[int index] { get; }
    
    public nint              Address     { get; }
    public int               Length      { get; }
    public IPlayerCharacter? LocalPlayer { get; }

    public IGameObject? SearchByID(ulong gameObjectID);
    
    public IGameObject? SearchByEntityID(uint entityID);
    
    public nint GetObjectAddress(int index);
    
    public IGameObject? CreateObjectReference(nint address);
}
