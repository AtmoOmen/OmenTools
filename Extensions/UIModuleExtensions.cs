using FFXIVClientStructs.FFXIV.Client.UI;

namespace OmenTools.Extensions;

public static unsafe class UIModuleExtensions
{
    extension(scoped ref UIModule module)
    {
        public static bool IsScreenReady()
        {
            if (NowLoading->IsAddonAndNodesReady()) 
                return false;
            if (FadeMiddle->IsAddonAndNodesReady()) 
                return false;
            if (FadeBack->IsAddonAndNodesReady())
                return false;

            return true;
        }
    } 
}
