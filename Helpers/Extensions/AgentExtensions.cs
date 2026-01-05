using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Helpers;

public static unsafe class AgentExtensions
{
    extension(AgentId agentID)
    {
        public AtkValue* SendEvent(ulong eventKind, params object[] eventparams)
        {
            var agent = AgentModule.Instance()->GetAgentByInternalId(agentID);
            return agent == null ? null : agent->SendEvent(eventKind, eventparams);
        }
    }

    extension(scoped ref AgentInterface agentInterface)
    {
        public AtkValue* SendEvent(ulong eventKind, params object[] eventParams)
        {
            var returnValue = stackalloc AtkValue[1];
            return agentInterface.SendEvent(returnValue, eventKind, eventParams);
        }

        public AtkValue* SendEvent(AtkValue* returnValue, ulong eventKind, params object[] eventParams)
        {
            using var atkValues = new AtkValueArray(eventParams);
            agentInterface.ReceiveEvent(returnValue, atkValues, (uint)eventParams.Length, eventKind);
            return returnValue;
        }
    }
}
