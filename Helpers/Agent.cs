using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OmenTools.Infos;
using System.Runtime.InteropServices;

namespace OmenTools.Helpers;

public static unsafe partial class HelpersOm
{
    public static AtkValue* SendEvent(AgentId agentId, ulong eventKind, params object[] eventparams)
    {
        var agent = AgentModule.Instance()->GetAgentByInternalId(agentId);
        return agent == null ? null : SendEvent(agent, eventKind, eventparams);
    }

    public static AtkValue* SendEvent(AgentInterface* agentInterface, ulong eventKind, params object[] eventParams)
    {
        var eventObject = stackalloc AtkValue[1];
        return SendEvent(agentInterface, eventObject, eventKind, eventParams);
    }

    public static AtkValue* SendEvent(AgentInterface* agentInterface, AtkValue* eventObject, ulong eventKind, params object[] eventParams)
    {
        using var atkValues = new AtkValueArray(eventParams);
        agentInterface->ReceiveEvent(eventObject, atkValues, (uint)eventParams.Length, eventKind);
        return eventObject;
    }

    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct EventObject
    {
        [FieldOffset(0)]
        public ulong Unknown0;

        [FieldOffset(8)]
        public ulong Unknown8;
    }
}