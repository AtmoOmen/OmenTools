using OmenTools.Dalamud.Services.Game.Object.ObjectKinds;

namespace OmenTools.Dalamud.Services.Game.Object.Abstractions.ObjectKinds;

public interface IEventObj : IGameObject
{
    new static IEventObj Create(nint address) => new EventObj(address);
}
