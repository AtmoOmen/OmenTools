using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Infos;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    private static readonly Dictionary<AgentId, nint> CachedAgents = [];

    public static AtkValue* SendEvent(AgentId agentID, ulong eventKind, params object[] eventparams)
    {
        if (!CachedAgents.TryGetValue(agentID, out var agent))
        {
            var newAgent = (nint)AgentModule.Instance()->GetAgentByInternalId(agentID);
            CachedAgents[agentID] = newAgent;
            agent = newAgent;
        }

        return agent == nint.Zero ? null : SendEvent((AgentInterface*)agent, eventKind, eventparams);
    }

    public static AtkValue* SendEvent(AgentInterface* agentInterface, ulong eventKind, params object[] eventParams)
    {
        var returnValue = stackalloc AtkValue[1];
        return SendEvent(agentInterface, returnValue, eventKind, eventParams);
    }

    public static AtkValue* SendEvent(AgentInterface* agentInterface, AtkValue* returnValue, ulong eventKind, params object[] eventParams)
    {
        using var atkValues = new AtkValueArray(eventParams);
        agentInterface->ReceiveEvent(returnValue, atkValues, (uint)eventParams.Length, eventKind);
        return returnValue;
    }
}
