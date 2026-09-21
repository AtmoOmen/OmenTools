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

        public bool IsFullyReceived
        (
            uint itemID = 0
        )
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
                var listingCount = (int)ptr->ListingCount;
                var listings     = ptr->Listings.ToArray();

                if (listingCount > listings.Length)
                    return false;

                // 购买成功时客户端只会前移条目并递减计数, 残留的尾巴不会被清理, 因此只能校验计数范围内的条目。
                for (var i = 0; i < listingCount; i++)
                {
                    if (listings[i].ItemId != searchItemID || listings[i].UnitPrice == 0)
                        return false;
                }

                return ptr->EntryCount switch
                {
                    > 10 => listingCount >= 10,
                    0    => true,
                    _    => listingCount != 0
                };
            }
        }
    }
}
