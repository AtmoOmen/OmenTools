using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace OmenTools.Helpers;

public static class GameObjectExtension
{
    public static Vector3 ToVector3(this Vector2 vector2) => 
        vector2.ToVector3(DService.ObjectTable.LocalPlayer?.Position.Y ?? 0);

    public static unsafe bool TargetInteract(this IGameObject? gameObject)
    {
        if (gameObject == null) return false;
        
        DService.Targets.Target = gameObject;
        return TargetSystem.Instance()->InteractWithObject(gameObject.ToStruct()) != 0;
    }

    public static unsafe bool Interact(this IGameObject? gameObject) => 
        gameObject != null && TargetSystem.Instance()->InteractWithObject(gameObject.ToStruct()) != 0;

    public static IGameObject? FindNearest(this IEnumerable<IGameObject> gameObjects, 
                                           Vector3                       source,
                                           Func<IGameObject, bool>       predicate) =>
        gameObjects.Where(predicate).MinBy(x => Vector3.Distance(source, x.Position));

    public static Vector2 ToVector2(this Vector3 vector3) => 
        new(vector3.X, vector3.Z);

    public static Vector2 ToVector2(this FFXIVClientStructs.FFXIV.Common.Math.Vector3 vector3) => 
        new(vector3.X, vector3.Z);

    public static Vector3 ToVector3(this Vector2 vector2, float Y) => 
        new(vector2.X, Y, vector2.Y);
}
