using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using StatusListType = OmenTools.Dalamud.Services.Game.StatusList;
using CSBattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;
using CSCastInfo = FFXIVClientStructs.FFXIV.Client.Game.Character.CastInfo;

namespace OmenTools.Dalamud.Services.Game.Object.ObjectKinds;

internal unsafe class BattleChara
(
    nint address
) : Character(address), IBattleChara
{
    protected new CSBattleChara* Struct => (CSBattleChara*)Address;

    private CSCastInfo     CastInfo            => Struct->CastInfo;
    public  StatusListType StatusList          => new(Struct->GetStatusManager());
    public  bool           IsCasting           => CastInfo.IsCasting;
    public  bool           IsCastInterruptible => CastInfo.Interruptible;
    public  ActionType     CastActionType      => (ActionType)CastInfo.ActionType;
    public  uint           CastActionID        => CastInfo.ActionId;
    public  ulong          CastTargetObjectID  => CastInfo.TargetId;
    public  IGameObject?   CastTargetObject    => DService.Instance().ObjectTable.SearchByID(CastTargetObjectID);

    public float CurrentCastTime => CastInfo.CurrentCastTime != 0 ?
                                        CastInfo.CurrentCastTime :
                                        -1;

    public float BaseCastTime => CastInfo.BaseCastTime != 0 ?
                                     CastInfo.BaseCastTime :
                                     -1;

    public float TotalCastTime => CastInfo.TotalCastTime != 0 ?
                                      CastInfo.TotalCastTime :
                                      -1;

    public uint       CastSourceSequence     => CastInfo.SourceSequence;
    public Vector3    CastTargetLocation     => CastInfo.TargetLocation;
    public float      CastInfoRotation       => CastInfo.Rotation;
    public uint       ResponseSpellID        => CastInfo.ResponseSpellId;
    public ActionType ResponseActionType     => CastInfo.ResponseActionType;
    public uint       ResponseActionID       => CastInfo.ResponseActionId;
    public uint       ResponseGlobalSequence => CastInfo.ResponseGlobalSequence;
    public uint       ResponseSourceSequence => CastInfo.ResponseSourceSequence;
    public byte       ResponseTargetCount    => CastInfo.ResponseTargetCount;
    public byte       ResponseFlags          => CastInfo.ResponseFlags;
    public byte       ForayLevel             => Struct->ForayInfo.Level;
    public byte       ForayElement           => Struct->ForayInfo.Element;
    public string     FreeCompanyName        => Struct->FreeCompanyTagString;

    public new CSBattleChara* ToStruct() => Struct;
}
