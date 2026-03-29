using OmenTools.Dalamud.Services.ObjectTable.ObjectKinds;

namespace OmenTools.Dalamud.Services.ObjectTable.Abstractions.ObjectKinds;

public interface IPlayerCharacter : IBattleChara
{
    new static IPlayerCharacter Create(nint address) => new PlayerCharacter(address);
}
