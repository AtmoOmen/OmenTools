using Dalamud.Game.ClientState.Objects.Enums;
using OmenTools.Dalamud.Services.Game.Object.ObjectKinds;

namespace OmenTools.Dalamud.Services.Game.Object.Abstractions.ObjectKinds;

public interface IBattleNPC : IBattleChara
{
    BattleNpcSubKind BattleNPCKind { get; }

    new static IBattleNPC Create(nint address) => new BattleNPC(address);
}
