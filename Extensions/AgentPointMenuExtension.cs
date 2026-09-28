using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OmenTools.Extensions;

public static unsafe class AgentPointMenuExtension
{
    extension
    (
        scoped ref AgentPointMenu agent
    )
    {
        public int FindFirstUncompleteEntry()
        {
            fixed (AgentPointMenu* ptr = &agent)
            {
                if (ptr == null)
                    return -1;
                
                var entryCount        = ptr->Context->Entries.Count;
                var completionKey     = ptr->Context->PointMenuId;
                var completedBitfield = 0;
                
                if (ptr->CompletionData != null)
                    ptr->CompletionData->TryGetValue(in completionKey, out completedBitfield, false);
                
                if (entryCount is <= 0 or > 32)
                    return -1;

                for (var index = 0; index < entryCount; index++)
                {
                    if ((completedBitfield & (1 << index)) == 0)
                        return index;
                }

                return -1;
            }
        }
    }
}
