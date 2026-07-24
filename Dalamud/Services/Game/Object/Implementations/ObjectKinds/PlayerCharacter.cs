namespace OmenTools.Dalamud.Services.Game.Object.ObjectKinds;

internal unsafe class PlayerCharacter
(
    nint address
) : BattleChara(address), IPlayerCharacter
{
    public override ulong TargetObjectID => Struct->LookAt.Controller.Params[0].TargetParam.TargetId;
}
