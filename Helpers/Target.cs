﻿using GameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace OmenTools.Helpers;

public static partial class HelpersOm
{
    public static unsafe bool CanUseActionOnObject(GameObject* player, GameObject* target, float actionRange) 
        => !(GetGameDistanceFromObject(player, target) - 0.5 > actionRange);

    public static unsafe float GetGameDistanceFromObject(GameObject* player, GameObject* target) 
        => GetRealDistanceFromObject(player, target) - target->HitboxRadius;

    public static unsafe float GetRealDistanceFromObject(GameObject* player, GameObject* target)
    {
        var distance = player->Position - target->Position;
        return MathF.Sqrt(distance.X * distance.X + distance.Z * distance.Z);
    }
}
