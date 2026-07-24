namespace OmenTools.Dalamud.Services.Game.Object.ObjectKinds;

internal class EventObj
(
    nint address
) : GameObject(address), IEventObj;
