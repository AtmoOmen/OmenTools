namespace OmenTools.Dalamud.Services.ObjectTable.Abstractions;

using IGameObject = IGameObject;

public interface IObjectTable : IEnumerable<IGameObject>
{
    static readonly Range CharactersRange    = ..200;
    static readonly Range ClientRange        = 200..449;
    static readonly Range EventRange         = 449..489;
    static readonly Range StandRange         = 489..629;
    static readonly Range ReactionEventRange = 629..729;

    nint Address { get; }

    int Length { get; }

    IPlayerCharacter? LocalPlayer { get; }

    IGameObject? this[int index] { get; }

    IGameObject? SearchByID(ulong gameObjectID);

    IGameObject? SearchByID(ulong gameObjectID, Range range);

    IGameObject? SearchByEntityID(uint entityID);

    IGameObject? SearchByEntityID(uint entityID, Range range);

    IEnumerable<IGameObject> SearchObjects(Predicate<IGameObject> predicate, Range range);

    IEnumerable<IGameObject> SearchObjects(Predicate<IGameObject> predicate);

    IGameObject? SearchObject(Predicate<IGameObject> predicate, Range range);

    IGameObject? SearchObject(Predicate<IGameObject> predicate);

    nint GetObjectAddress(int index);

    IGameObject? CreateObjectReference(nint address);
}
