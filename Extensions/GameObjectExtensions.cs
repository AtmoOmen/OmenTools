using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace OmenTools.Extensions;

public static class GameObjectExtensions
{
    extension(IGameObject? gameObject)
    {
        public unsafe bool TargetInteract()
        {
            if (gameObject == null) return false;
        
            TargetManager.Target = gameObject;
            return TargetSystem.Instance()->InteractWithObject(gameObject.ToStruct()) != 0;
        }

        public unsafe bool Interact() => 
            gameObject != null && 
            TargetSystem.Instance()->InteractWithObject(gameObject.ToStruct()) != 0;
    }

    extension(IEnumerable<IGameObject> gameObjects)
    {
        public IGameObject? FindNearest(
            Vector3                 source,
            Func<IGameObject, bool> predicate) =>
            gameObjects.Where(predicate)
                       .MinBy(x => Vector3.DistanceSquared(source, x.Position));
    }
    
    extension(scoped in Vector2 vector2)
    {
        public Vector3 ToPlayerHeight() => 
            vector2.ToVector3(DService.Instance().ObjectTable.LocalPlayer?.Position.Y ?? 0);
    }
}
