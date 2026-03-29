using Dalamud.Game.ClientState.Objects.Enums;
using OmenTools.Dalamud.Services.ObjectTable.ObjectKinds;

namespace OmenTools.Dalamud.Services.ObjectTable.Abstractions.ObjectKinds;

public interface IBattleNPC : IBattleChara
{
    BattleNpcSubKind BattleNPCKind { get; }

    new static IBattleNPC Create(nint address) => new BattleNPC(address);
}
