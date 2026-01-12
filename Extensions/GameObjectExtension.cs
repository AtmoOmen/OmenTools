using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel.Sheets;
using Aetheryte = Lumina.Excel.Sheets.Aetheryte;
using Treasure = Lumina.Excel.Sheets.Treasure;

namespace OmenTools.Extensions;

public static class GameObjectExtension
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
            gameObject                                                         != null &&
            TargetSystem.Instance()->InteractWithObject(gameObject.ToStruct()) != 0;
    }

    extension(IEnumerable<IGameObject> gameObjects)
    {
        public IGameObject? FindNearest
        (
            Vector3                 source,
            Func<IGameObject, bool> predicate
        ) =>
            gameObjects.Where(predicate)
                       .MinBy(x => Vector3.DistanceSquared(source, x.Position));
    }

    extension(scoped in Vector2 vector2)
    {
        public Vector3 ToPlayerHeight() =>
            vector2.ToVector3(DService.Instance().ObjectTable.LocalPlayer?.Position.Y ?? 0);
    }

    extension(uint objStrID)
    {
        public (ObjectKind Kind, uint DataID) FromObjStrID()
        {
            if (objStrID < 1000000)
            {
                if (LuminaGetter.TryGetRow<BNpcBase>(objStrID, out _))
                    return (ObjectKind.BattleNpc, objStrID);
                
                if (objStrID >= 100000)
                {
                    var highDataID = objStrID + 900000;

                    if (LuminaGetter.TryGetRow<BNpcBase>(highDataID, out _))
                        return (ObjectKind.BattleNpc, highDataID);
                }

                return (ObjectKind.None, 0);
            }

            if (objStrID < 2000000)
            {
                if (LuminaGetter.TryGetRow<ENpcBase>(objStrID, out _))
                    return (ObjectKind.EventNpc, objStrID);

                return (ObjectKind.None, 0);
            }
            
            if (objStrID < 3000000)
            {
                var rawID = objStrID - 2000000;

                if (LuminaGetter.TryGetRow<Treasure>(rawID, out _))
                    return (ObjectKind.Treasure, rawID);

                return (ObjectKind.None, 0);
            }
            
            if (objStrID < 4000000)
            {
                var rawID = objStrID - 3000000;

                if (LuminaGetter.TryGetRow<Aetheryte>(rawID, out _))
                    return (ObjectKind.Aetheryte, rawID);

                return (ObjectKind.ReactionEventObject, rawID);
            }
            
            if (objStrID < 5000000)
            {
                var rawID = objStrID - 4000000;

                if (LuminaGetter.TryGetRow<GatheringPoint>(rawID, out _))
                    return (ObjectKind.GatheringPoint, rawID);

                return (ObjectKind.None, 0);
            }
            
            if (objStrID >= 10000000)
            {
                var rawID = objStrID - 10000000;

                if (LuminaGetter.TryGetRow<HousingFurniture>(rawID, out _) || LuminaGetter.TryGetRow<HousingYardObject>(rawID, out _))
                    return (ObjectKind.HousingEventObject, rawID);
            }
            
            if (objStrID >= 7000000)
            {
                var rawID = objStrID - 7000000;

                if (LuminaGetter.TryGetRow<Companion>(rawID, out _))
                    return (ObjectKind.Companion, rawID);
            }
            
            if (objStrID >= 5000000)
            {
                var rawID = objStrID - 3000000;

                if (LuminaGetter.TryGetRow<EObj>(rawID, out _))
                    return (ObjectKind.EventObj, rawID);
            }
            
            return (ObjectKind.None, 0);
        }
    }
}
