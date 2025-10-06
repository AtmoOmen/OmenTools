using System.Collections.Concurrent;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class LinkPayloadManager : OmenServiceBase
{
    private static readonly ConcurrentDictionary<uint, DalamudLinkPayload> DistributedPayloads = new();

    private static long lastID = -1;

    internal override void Init()
    {
        DistributedPayloads.Clear();
        Interlocked.Exchange(ref lastID, -1);
    }

    public static DalamudLinkPayload Register(Action<uint, SeString> commandAction, out uint id)
    {
        id = GetUniqueID();

        var payload = DService.Chat.AddChatLinkHandler(id, commandAction);
        DistributedPayloads.TryAdd(id, payload);
        return payload;
    }

    public static bool Unregister(uint id)
    {
        if (!DistributedPayloads.TryRemove(id, out _)) return false;

        DService.Chat.RemoveChatLinkHandler(id);
        return true;
    }
    
    public static bool TryGetPayload(uint id, out DalamudLinkPayload? payload)
        => DistributedPayloads.TryGetValue(id, out payload);

    private static uint GetUniqueID()
    {
        while (true)
        {
            var newID = (uint)Interlocked.Increment(ref lastID);
            if (!DistributedPayloads.ContainsKey(newID))
                return newID;
        }
    }

    internal override void Uninit()
    {
        DService.Chat.RemoveChatLinkHandler();
        DistributedPayloads.Clear();
    }
}
