using Dalamud.Game.NativeWrapper;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Interop.Game.Models;

namespace OmenTools.Extensions;

public static unsafe class AgentExtension
{
    extension(AgentId agentID)
    {
        public AtkValue* SendEvent(ulong eventKind, params object[] eventParams)
        {
            var agent = AgentModule.Instance()->GetAgentByInternalId(agentID);
            return agent == null ? null : agent->SendEvent(eventKind, eventParams);
        }
        
        /// <remarks>请自行处理处理传入的 AtkValueArray 生命周期</remarks>
        public AtkValue* SendEvent(ulong eventKind, scoped in AtkValueArray valueArray)
        {
            var agent = AgentModule.Instance()->GetAgentByInternalId(agentID);
            return agent == null ? null : agent->SendEvent(eventKind, valueArray);
        }
    }

    extension(scoped ref AgentInterface agentInterface)
    {
        public AtkValue* SendEvent(ulong eventKind, params object[] eventParams)
        {
            var returnValue = stackalloc AtkValue[1];
            return agentInterface.SendEvent(returnValue, eventKind, eventParams);
        }
        
        /// <remarks>请自行处理处理传入的 AtkValueArray 生命周期</remarks>
        public AtkValue* SendEvent(ulong eventKind, scoped in AtkValueArray valueArray)
        {
            var returnValue = stackalloc AtkValue[1];
            return agentInterface.SendEvent(returnValue, eventKind, valueArray);
        }

        public AtkValue* SendEvent(AtkValue* returnValue, ulong eventKind, params object[] eventParams)
        {
            using var atkValues = new AtkValueArray(eventParams);
            agentInterface.ReceiveEvent(returnValue, atkValues, (uint)eventParams.Length, eventKind);
            return returnValue;
        }
        
        /// <remarks>请自行处理处理传入的 AtkValueArray 生命周期</remarks>
        public AtkValue* SendEvent(AtkValue* returnValue, ulong eventKind, scoped in AtkValueArray valueArray)
        {
            agentInterface.ReceiveEvent(returnValue, valueArray, (uint)valueArray.Length, eventKind);
            return returnValue;
        }
    }

    extension(scoped in AgentInterfacePtr agent)
    {
        public AgentInterface* ToStruct() =>
            (AgentInterface*)agent.Address;

        public T* ToStruct<T>() where T : unmanaged =>
            (T*)agent.Address;
    }
}
