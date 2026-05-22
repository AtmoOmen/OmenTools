namespace OmenTools.Utils.FuzzyMatcher;

public record struct FuzzySearchWeight
(
    int BaseScore,
    int BoundaryBonus,
    int ConsecutiveBonus,
    int InitialismBase,
    int ApproximateBase,
    int GapPenalty
)
{
    public static FuzzySearchWeight Default { get; } = new(300, 20, 12, 10, 150, 4);
    public static FuzzySearchWeight Title { get; } = new(400, 24, 18, 16, 200, 3);
    public static FuzzySearchWeight Name { get; } = new(320, 20, 14, 12, 160, 4);
    public static FuzzySearchWeight Meta { get; } = new(200, 14, 10, 8, 100, 5);
}
