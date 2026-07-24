using CSLuaActor = FFXIVClientStructs.FFXIV.Client.Game.Event.LuaActor;

namespace OmenTools.Dalamud.Services.Game.Event;

internal unsafe class LuaActor
(
    nint address
) : ILuaActor
{
    private CSLuaActor* Struct => (CSLuaActor*)Address;

    public nint         Address { get; } = address;
    public IGameObject? Object  => DService.Instance().ObjectTable.CreateObjectReference((nint)Struct->Object);
    public string       Value   => Struct->LuaString.ToString();

    public CSLuaActor* ToStruct() => Struct;
}
