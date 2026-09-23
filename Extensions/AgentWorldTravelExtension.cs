using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Extensions;

public static unsafe class AgentWorldTravelExtension
{
    extension
    (
        scoped ref AgentWorldTravel agent
    )
    {
        public void TravelTo
        (
            uint destinationWorldID
        ) =>
            agent.TravelTo((ushort)destinationWorldID);

        public void TravelTo
        (
            ushort destinationWorldID
        )
        {
            fixed (AgentWorldTravel* ptr = &agent)
            {
                if (ptr == null)
                    return;

                ptr->DestinationWorldId = destinationWorldID;
                ptr->SetupWorldTravelInfo((ushort)GameState.CurrentWorld, destinationWorldID);
                AgentId.WorldTravel.SendEvent(1, 0);
            }
        }
    }
}
