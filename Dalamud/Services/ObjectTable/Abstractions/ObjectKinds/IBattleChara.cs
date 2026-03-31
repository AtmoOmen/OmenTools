using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace OmenTools.Dalamud.Services.ObjectTable.Abstractions.ObjectKinds;

public interface IBattleChara : ICharacter
{
    StatusList.Implementations.StatusList StatusList          { get; }
    bool                                  IsCasting           { get; }
    bool                                  IsCastInterruptible { get; }
    ActionType                            CastActionType      { get; }
    uint                                  CastActionID        { get; }
    ulong                                 CastTargetObjectID  { get; }
    IGameObject?                          CastTargetObject    { get; }
    float                                 CurrentCastTime     { get; }
    float                                 BaseCastTime        { get; }
    float                                 TotalCastTime       { get; }

    new unsafe BattleChara* ToStruct();

    new static IBattleChara Create(nint address) => new Services.ObjectTable.ObjectKinds.BattleChara(address);
}
