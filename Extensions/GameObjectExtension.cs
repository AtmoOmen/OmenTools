using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel.Sheets;
using Aetheryte = Lumina.Excel.Sheets.Aetheryte;
using Control = FFXIVClientStructs.FFXIV.Client.Game.Control.Control;
using GameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using Treasure = Lumina.Excel.Sheets.Treasure;

namespace OmenTools.Extensions;

public static class GameObjectExtension
{
    private static readonly HashSet<ObjectKind> ValidMTQObjectKinds = [ObjectKind.EventObj, ObjectKind.EventNpc];
    
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

        public unsafe bool IsMTQ()
        {
            if (gameObject == null) return false;
            return gameObject.ToStruct()->IsMTQ();
        }
        
        public unsafe bool IsReachable()
        {
            if (gameObject == null) return false;
            return gameObject.ToStruct()->IsReachable();
        }
    }

    extension(scoped ref GameObject gameObject)
    {
        public unsafe bool IsMTQ()
        {
            fixed (GameObject* ptr = &gameObject)
            {
                if (ptr == null) return false;
                
                if (ptr->RenderFlags != 0    ||
                    !ptr->GetIsTargetable()  ||
                    !ValidMTQObjectKinds.Contains(ptr->ObjectKind))
                    return false;

                return QuestIcon.IsQuest(ptr->NamePlateIconId) ||
                       ptr->ObjectKind == ObjectKind.EventObj && ptr->TargetStatus == 15;
            }
        }

        public unsafe bool IsReachable()
        {
            fixed (GameObject* ptr = &gameObject)
            {
                if (ptr == null) return false;

                var localPlayer = Control.GetLocalPlayer();
                if (localPlayer == null) return false;
                
                return EventFramework.Instance()->CheckInteractRange((GameObject*)localPlayer, ptr, 23, false);
            }
        }
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
            switch (objStrID)
            {
                case 0:
                    return (ObjectKind.None, 0);
                case < 1000000 when LuminaGetter.TryGetRow<BNpcBase>(objStrID, out _):
                    return (ObjectKind.BattleNpc, objStrID);
                case < 1000000:
                {
                    if (objStrID >= 100000)
                    {
                        var highDataID = objStrID + 900000;
                        if (LuminaGetter.TryGetRow<BNpcBase>(highDataID, out _))
                            return (ObjectKind.BattleNpc, highDataID);
                    }

                    return (ObjectKind.None, 0);
                }
                case < 2000000 when LuminaGetter.TryGetRow<ENpcBase>(objStrID, out _):
                    return (ObjectKind.EventNpc, objStrID);
                case < 2000000:
                    return (ObjectKind.None, 0);
                case < 3000000:
                {
                    var rawID = objStrID - 2000000;
                    if (LuminaGetter.TryGetRow<Treasure>(rawID, out _))
                        return (ObjectKind.Treasure, rawID);

                    return (ObjectKind.None, 0);
                }
                case < 4000000:
                {
                    var rawID = objStrID - 3000000;
                    if (LuminaGetter.TryGetRow<Aetheryte>(rawID, out _))
                        return (ObjectKind.Aetheryte, rawID);

                    return (ObjectKind.ReactionEventObject, rawID);
                }
                case < 5000000:
                {
                    var rawID = objStrID - 4000000;
                    if (LuminaGetter.TryGetRow<GatheringPoint>(rawID, out _))
                        return (ObjectKind.GatheringPoint, rawID);

                    return (ObjectKind.None, 0);
                }
                case >= 10000000:
                {
                    var rawID = objStrID - 10000000;
                    if (LuminaGetter.TryGetRow<HousingFurniture>(rawID, out _) || LuminaGetter.TryGetRow<HousingYardObject>(rawID, out _))
                        return (ObjectKind.HousingEventObject, rawID);

                    break;
                }
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
