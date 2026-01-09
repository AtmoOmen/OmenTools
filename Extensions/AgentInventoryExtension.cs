using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Extensions;

public static unsafe class AgentInventoryExtension
{
    extension(scoped ref AgentInventory agent)
    {
        public uint GetActiveAddonID()
        {
            if (!UIModule.Instance()->IsInventoryOpen()) return 0;

            var addonID = (ushort)agent.AddonId;
            var addon   = RaptureAtkUnitManager.Instance()->GetAddonById(addonID);
            if (!addon->IsAddonAndNodesReady()) return 0;

            return addonID;
        }
    }
}
