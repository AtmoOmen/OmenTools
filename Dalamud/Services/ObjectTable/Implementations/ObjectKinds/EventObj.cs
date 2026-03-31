namespace OmenTools.Dalamud.Services.ObjectTable.ObjectKinds;

internal class EventObj
(
    nint address
) : GameObject(address), IEventObj;
