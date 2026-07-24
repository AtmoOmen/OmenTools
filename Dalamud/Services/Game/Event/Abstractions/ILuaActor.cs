using CSLuaActor = FFXIVClientStructs.FFXIV.Client.Game.Event.LuaActor;

namespace OmenTools.Dalamud.Services.Game.Event.Abstractions;

public interface ILuaActor
{
    nint         Address { get; }
    IGameObject? Object  { get; }
    string       Value   { get; }

    unsafe CSLuaActor* ToStruct();

    static ILuaActor Create(nint address) => new LuaActor(address);
}
