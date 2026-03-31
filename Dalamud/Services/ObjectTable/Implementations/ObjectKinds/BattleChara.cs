using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace OmenTools.Dalamud.Services.ObjectTable.ObjectKinds;

internal unsafe class BattleChara
(
    nint address
) : Character(address), IBattleChara
{
    private       CastInfo                                                    CastInfo => Struct->CastInfo;
    protected new FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* Struct => (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)Address;
    public        StatusList.Implementations.StatusList                       StatusList => new(Struct->GetStatusManager());
    public        bool                                                        IsCasting => CastInfo.IsCasting;
    public        bool                                                        IsCastInterruptible => CastInfo.Interruptible;
    public        ActionType                                                  CastActionType => (ActionType)CastInfo.ActionType;
    public        uint                                                        CastActionID => CastInfo.ActionId;
    public        ulong                                                       CastTargetObjectID => CastInfo.TargetId;
    public        IGameObject?                                                CastTargetObject => DService.Instance().ObjectTable.SearchByID(CastTargetObjectID);
    public        float                                                       CurrentCastTime => CastInfo.CurrentCastTime != 0 ? CastInfo.CurrentCastTime : -1;
    public        float                                                       BaseCastTime => CastInfo.BaseCastTime != 0 ? CastInfo.BaseCastTime : -1;
    public        float                                                       TotalCastTime => CastInfo.TotalCastTime != 0 ? CastInfo.TotalCastTime : -1;

    public new FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* ToStruct() => Struct;
}
