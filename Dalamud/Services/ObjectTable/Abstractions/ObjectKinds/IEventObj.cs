using OmenTools.Dalamud.Services.ObjectTable.ObjectKinds;

namespace OmenTools.Dalamud.Services.ObjectTable.Abstractions.ObjectKinds;

public interface IEventObj : IGameObject
{
    new static IEventObj Create(nint address) => new EventObj(address);
}
