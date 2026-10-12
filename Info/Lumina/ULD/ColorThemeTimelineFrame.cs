namespace OmenTools.Info.Lumina.ULD;

public readonly record struct ColorThemeTimelineFrame
(
    int    AnimationIndex,
    int    GroupIndex,
    int    FrameIndex,
    ushort FrameID,
    uint   ColorID
);
