using CSEventHandler = FFXIVClientStructs.FFXIV.Client.Game.Event.EventHandler;
using EventId = FFXIVClientStructs.FFXIV.Client.Game.Event.EventId;
using LuaStatus = FFXIVClientStructs.FFXIV.Common.Lua.LuaStatus;
using SceneFlag = FFXIVClientStructs.FFXIV.Client.Game.Event.SceneFlag;

namespace OmenTools.Dalamud.Services.Game.Event.Abstractions;

public interface IEventHandler
{
    nint         Address          { get; }
    EventId      EventID          { get; }
    uint         IconID           { get; }
    short        Scene            { get; }
    IGameObject? SceneGameObject  { get; }
    SceneFlag    SceneFlags       { get; }
    LuaStatus    LuaStatus        { get; }
    bool         HasTimer         { get; }
    uint         EventItemID      { get; }
    int          RecommendedLevel { get; }

    int GetTimeRemaining(int currentTimestamp);

    unsafe CSEventHandler* ToStruct();

    static IEventHandler Create(nint address) => new EventHandler(address);
}
