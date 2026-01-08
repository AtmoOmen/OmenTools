using System.Collections.Concurrent;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using OmenTools.Abstracts;

namespace OmenTools.Managers;

public class LinkPayloadManager : OmenServiceBase<LinkPayloadManager>
{
    public DalamudLinkPayload Reg(Action<uint, SeString> commandAction, out uint id)
    {
        id = GetUniqueID();

        var payload = DService.Instance().Chat.AddChatLinkHandler(id, commandAction);
        distributedPayloads.TryAdd(id, payload);
        return payload;
    }

    public bool Unreg(uint id)
    {
        if (!distributedPayloads.TryRemove(id, out _)) return false;

        DService.Instance().Chat.RemoveChatLinkHandler(id);
        return true;
    }
    
    public bool TryGetPayload(uint id, out DalamudLinkPayload? payload)
        => distributedPayloads.TryGetValue(id, out payload);
    
    
    private readonly ConcurrentDictionary<uint, DalamudLinkPayload> distributedPayloads = [];

    private long lastID = -1;

    internal override void Init()
    {
        distributedPayloads.Clear();
        Interlocked.Exchange(ref lastID, -1);
    }
    
    internal override void Uninit()
    {
        DService.Instance().Chat.RemoveChatLinkHandler();
        distributedPayloads.Clear();
    }

    private uint GetUniqueID()
    {
        while (true)
        {
            var newID = (uint)Interlocked.Increment(ref lastID);
            if (!distributedPayloads.ContainsKey(newID))
                return newID;
        }
    }
}
