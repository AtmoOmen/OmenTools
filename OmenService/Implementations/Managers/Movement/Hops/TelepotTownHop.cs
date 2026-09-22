using OmenTools.Info.Game.AetheryteRecord;

namespace OmenTools.OmenService;

public sealed record TelepotTownHop
(
    AetheryteRecord Target
) : ZoneHop(Target.ZoneID);
