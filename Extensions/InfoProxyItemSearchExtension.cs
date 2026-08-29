using FFXIVClientStructs.FFXIV.Client.UI.Info;
using OmenTools.OmenService;

namespace OmenTools.Extensions;

public static unsafe class InfoProxyItemSearchExtension
{
    extension
    (
        scoped ref InfoProxyItemSearch proxy
    )
    {
        public static bool IsListingsStuck => 
            GameState.Instance().IsMarketListingsStuck;
        
        public bool IsFullyReceived(uint itemID = 0)
        {
            fixed (InfoProxyItemSearch* ptr = &proxy)
            {
                if (ptr == null ||
                    InfoProxyItemSearch.IsListingsStuck)
                    return false;

                if (itemID            != 0 &&
                    ptr->SearchItemId != itemID)
                    return false;

                var searchItemID = ptr->SearchItemId;
                var currentCount = ptr->Listings
                                   .ToArray()
                                   .Count(x => x.ItemId == searchItemID && x.UnitPrice != 0);
        
                if (currentCount != ptr->ListingCount)
                    return false;

                return ptr->EntryCount switch
                {
                    > 10 => ptr->ListingCount >= 10,
                    0    => true,
                    _    => ptr->ListingCount != 0
                };
            }
        }
    }
}
