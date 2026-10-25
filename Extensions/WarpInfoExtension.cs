using System.Numerics;
using FFXIVClientStructs.Expansion.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Layer;
using Control = FFXIVClientStructs.FFXIV.Client.Game.Control.Control;
using WarpInfo = FFXIVClientStructs.Expansion.FFXIV.Client.Game.UI.WarpInfo;

namespace OmenTools.Extensions;

public static unsafe class WarpInfoExtension
{
    extension
    (
        scoped ref WarpInfo warpInfo
    )
    {
        public void PopRange
        (
            ExitRangeLayoutInstance* exit,
            Vector3?                 recoveredPosition = null
        )
        {
            fixed (WarpInfo* ptr = &warpInfo)
            {
                var pop = exit->GetReturnInstance();

                if (recoveredPosition == null)
                {
                    var player = Control.GetLocalPlayer();
                    ptr->ZoneLineLastHit = player == null ?
                                               default :
                                               player->Position;
                    ptr->WarpPos = *pop->GetTranslationImpl();
                }
                else
                {
                    ptr->ZoneLineLastHit = new(0);
                    ptr->WarpPos         = recoveredPosition.Value;
                }

                ptr->ExitRange = exit;
                ptr->State     = WarpState.WaitingForZoneLine;
            }
        }
    }
}
