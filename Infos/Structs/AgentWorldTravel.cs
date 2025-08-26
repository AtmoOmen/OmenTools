using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Infos;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct AgentWorldTravel
{
    private static readonly CompSig                      WorldTravelSetupInfoSig = new("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B F9 41 0F B7 F0 48 8B 49");
    private delegate        void                         WorldTravelSetupInfoDelegate(AgentWorldTravel* agent, ushort currentWorld, ushort targetWorld);
    private static readonly WorldTravelSetupInfoDelegate WorldTravelSetupInfo = WorldTravelSetupInfoSig.GetDelegate<WorldTravelSetupInfoDelegate>();

    [FieldOffset(0)]
    public AgentInterface AgentInterface;

    [FieldOffset(76)]
    public uint WorldToTravel;

    public void TravelTo(uint worldToTravel)
    {
        WorldToTravel = worldToTravel;

        fixed (AgentWorldTravel* instance = &this)
        {
            WorldTravelSetupInfo(instance, AgentLobby.Instance()->LobbyData.CurrentWorldId, (ushort)WorldToTravel);

            SendEvent(AgentId.WorldTravel, 1, 0);
        }
    }

    public static AgentWorldTravel* Instance() =>
        (AgentWorldTravel*)AgentModule.Instance()->GetAgentByInternalId(AgentId.WorldTravel);
}
