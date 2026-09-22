using OmenTools.Info.Game.AetheryteRecord;

namespace OmenTools.OmenService;

public sealed record AetheryteHop
(
    AetheryteRecord Record
) : ZoneHop(Record.ZoneID);
