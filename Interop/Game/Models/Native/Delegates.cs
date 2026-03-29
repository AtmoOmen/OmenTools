using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Lua;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace OmenTools.Interop.Game.Models.Native;

public unsafe delegate AtkValue* AgentReceiveEventDelegate
(
    AgentInterface* agent,
    AtkValue*       returnValues,
    AtkValue*       values,
    uint            valueCount,
    ulong           eventKind
);

public unsafe delegate void AgentUpdateDelegate(AgentInterface* agent, uint frameCount);

public unsafe delegate void AgentShowDelegate(AgentInterface* agent);

public unsafe delegate void AgentHideDelegate(AgentInterface* agent);

public unsafe delegate ulong LuaFunctionDelegate(lua_State* state);
