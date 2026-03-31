using OmenTools.Dalamud.Services.ObjectTable.ObjectKinds;

namespace OmenTools.Dalamud.Services.ObjectTable.Abstractions.ObjectKinds;

public interface INPC : ICharacter
{
    new static INPC Create(nint address) => new NPC(address);
}
