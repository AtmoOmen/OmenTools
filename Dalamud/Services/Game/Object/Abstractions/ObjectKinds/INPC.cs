using OmenTools.Dalamud.Services.Game.Object.ObjectKinds;

namespace OmenTools.Dalamud.Services.Game.Object.Abstractions.ObjectKinds;

public interface INPC : ICharacter
{
    new static INPC Create(nint address) => new NPC(address);
}
