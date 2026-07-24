using CSEventHandler = FFXIVClientStructs.FFXIV.Client.Game.Event.EventHandler;
using EventId = FFXIVClientStructs.FFXIV.Client.Game.Event.EventId;
using LuaStatus = FFXIVClientStructs.FFXIV.Common.Lua.LuaStatus;
using SceneFlag = FFXIVClientStructs.FFXIV.Client.Game.Event.SceneFlag;

namespace OmenTools.Dalamud.Services.Game.Event;

internal unsafe class EventHandler
(
    nint address
) : IEventHandler
{
    private CSEventHandler* Struct => (CSEventHandler*)Address;

    public nint         Address          { get; } = address;
    public EventId      EventID          => Struct->GetEventId();
    public uint         IconID           => Struct->IconId;
    public short        Scene            => Struct->Scene;
    public IGameObject? SceneGameObject  => DService.Instance().ObjectTable.CreateObjectReference((nint)Struct->SceneGameObject);
    public SceneFlag    SceneFlags       => Struct->SceneFlags;
    public LuaStatus    LuaStatus        => Struct->LuaStatus;
    public bool         HasTimer         => Struct->HasTimer();
    public uint         EventItemID      => Struct->GetEventItemId();
    public int          RecommendedLevel => Struct->GetRecommendedLevel();

    public int GetTimeRemaining(int currentTimestamp) => Struct->GetTimeRemaining(currentTimestamp);

    public CSEventHandler* ToStruct() => Struct;
}
