using OmenTools.Dalamud.Services.Game.Object.ObjectKinds;

namespace OmenTools.Dalamud.Services.Game.Object.Abstractions.ObjectKinds;

public interface IPlayerCharacter : IBattleChara
{
    new static IPlayerCharacter Create(nint address) => new PlayerCharacter(address);
}
