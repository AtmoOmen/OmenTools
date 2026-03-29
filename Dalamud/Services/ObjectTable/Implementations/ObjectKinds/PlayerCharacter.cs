namespace OmenTools.Dalamud.Services.ObjectTable.ObjectKinds;

internal unsafe class PlayerCharacter
(
    nint address
) : BattleChara(address), IPlayerCharacter
{
    public override ulong TargetObjectID => Struct->LookAt.Controller.Params[0].TargetParam.TargetId;
}
