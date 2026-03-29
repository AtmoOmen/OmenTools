using Dalamud.Game.ClientState.Objects.Enums;

namespace OmenTools.Dalamud.Services.ObjectTable.ObjectKinds;

internal unsafe class BattleNPC
(
    nint address
) : BattleChara(address), IBattleNPC
{
    public BattleNpcSubKind BattleNPCKind => (BattleNpcSubKind)Struct->Character.GameObject.SubKind;

    public override ulong TargetObjectID => Struct->Character.TargetId;
}
