using System.Numerics;
using System.Runtime.InteropServices;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit)]
public struct ActorSetPosPacket
(
    Vector3 position,
    byte    territoryTransportType       = 1,
    byte    characterMode                = 2,
    uint    transitionTerritoryFilterKey = 0
)
    : IDownstreamPacket
{
    [FieldOffset(2)]
    public byte TerritoryTransportType = territoryTransportType;

    [FieldOffset(3)]
    public byte CharacterMode = characterMode;

    [FieldOffset(4)]
    public uint TransitionTerritoryFilterKey = transitionTerritoryFilterKey;

    [FieldOffset(8)]
    public Vector3 Position = position;

    public string Log() =>
        $"Actor Set Pos 包体\n"                                 +
        $"位置: {Position:F2}\n"                                +
        $"TerritoryTransportType: {TerritoryTransportType}\n" +
        $"CharacterMode: {CharacterMode}\n"                   +
        $"TransitionTerritoryFilterKey: {TransitionTerritoryFilterKey}";


    public static CompSig Signature { get; } = 
        new("4C 8B C2 8B D1 48 8D 0D ?? ?? ?? ?? E9 ?? ?? ?? ?? CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC 48 89 5C 24");

    public unsafe delegate void* Delegate(uint entityID, ActorSetPosPacket* packet);
}
