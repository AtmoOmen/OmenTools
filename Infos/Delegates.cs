using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Lua;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Infos;

public static unsafe partial class InfosOm
{
    public delegate AtkValue* AgentReceiveEventDelegate(AgentInterface* agent, AtkValue* returnValues, AtkValue* values, uint valueCount, ulong eventKind);
    
    public delegate void AgentUpdateDelegate(AgentInterface* agent, uint frameCount);
    
    public delegate void AgentShowDelegate(AgentInterface* agent);
    
    public delegate void AgentHideDelegate(AgentInterface* agent);
    
    public delegate ulong LuaFunctionDelegate(lua_State* state);
}
