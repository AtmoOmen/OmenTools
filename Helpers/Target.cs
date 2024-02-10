namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static unsafe bool CanUseActionOnObject(GameObject* player, GameObject* target, float actionRange)
    {
        var adjustedActionRange = actionRange + 0.5f;
        var actionRangeSquared = adjustedActionRange * adjustedActionRange;

        return GetSquareDistanceFromObject(player, target) <= actionRangeSquared;
    }

    public static unsafe float GetSquareDistanceFromObject(GameObject* player, GameObject* target)
    {
        var distance = player->Position - target->Position;
        return (distance.X * distance.X) + (distance.Z * distance.Z) - 2 * target->HitboxRadius * target->HitboxRadius;
    }

    public static unsafe float GetGameDistanceFromObject(GameObject* player, GameObject* target)
    {
        return GetRealDistanceFromObject(player, target) - target->HitboxRadius;
    }

    public static unsafe float GetRealDistanceFromObject(GameObject* player, GameObject* target)
    {
        var distance = player->Position - target->Position;
        return MathF.Sqrt((distance.X * distance.X) + (distance.Z * distance.Z));
    }
}