using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using GameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace OmenTools.Helpers;

public static unsafe class EventFrameworkExtensions
{
    extension(scoped ref EventFramework framework)
    {
        public bool IsEventIDNearby(EventId eventID)
        {
            fixed (EventFramework* frameworkPtr = &framework)
            {
                if (frameworkPtr == null) return false;
                
                foreach (var eve in frameworkPtr->EventHandlerModule.EventHandlerMap)
                {
                    if (eve.Item2.Value->Info.EventId == eventID && eve.Item2.Value->EventObjects.Count != 0)
                        return true;
                }

                return false;
            }
        }

        public bool TryGetNearestEventID(
            Predicate<EventHandlerInfo> conditionInfo,
            Predicate<GameObject>       conditionObj,
            Vector3                     source,
            out EventId                 eventID)
        {
            fixed (EventFramework* frameworkPtr = &framework)
            {
                eventID = 0;
                
                if (frameworkPtr == null) return false;

                List<(Vector3 Position, uint EventId)> candidates = [];
                foreach (var eve in frameworkPtr->EventHandlerModule.EventHandlerMap)
                {
                    var value = eve.Item2.Value;
                    if (value->EventObjects.Count != 0 && conditionInfo(value->Info))
                    {
                        foreach (var obj in value->EventObjects)
                        {
                            if (obj.Value == null || !conditionObj(*obj.Value)) continue;
                            candidates.Add((obj.Value->Position, value->Info.EventId));
                        }
                    }
                }

                if (candidates.Count == 0) return false;

                var first = candidates.OrderBy(x => Vector3.DistanceSquared(source, x.Position)).FirstOrDefault();
                eventID = first.EventId;
                return true;
            }
        }

        public bool TryGetNearestEvent(
            Predicate<EventHandlerInfo> conditionInfo,
            Predicate<GameObject>       conditionObj,
            Vector3                     source,
            out EventId                 eventID,
            out GameObjectId            eventObjectID)
        {
            fixed (EventFramework* frameworkPtr = &framework)
            {
                eventID       = 0;
                eventObjectID = 0;
                
                if (frameworkPtr == null) return false;

                List<(Vector3 Position, uint EventId, ulong ObjectID)> candidates = [];
                foreach (var eve in frameworkPtr->EventHandlerModule.EventHandlerMap)
                {
                    var value = eve.Item2.Value;
                    if (value->EventObjects.Count != 0 && conditionInfo(value->Info))
                    {
                        foreach (var obj in value->EventObjects)
                        {
                            if (obj.Value == null || !conditionObj(*obj.Value)) continue;
                            candidates.Add((obj.Value->Position, value->Info.EventId, obj.Value->GetGameObjectId()));
                        }
                    }
                }

                if (candidates.Count == 0) return false;

                var first = candidates.OrderBy(x => Vector3.DistanceSquared(source, x.Position)).FirstOrDefault();

                eventID       = first.EventId;
                eventObjectID = first.ObjectID;
                return true;
            }
        }

        public bool TryGetNearestEvent(
            Predicate<EventHandlerInfo> conditionInfo,
            Predicate<GameObject>       conditionObj,
            Vector3                     source,
            out EventId                 eventID,
            out GameObjectId            eventObjectID,
            out Vector3                 position,
            out float                   distance3D)
        {
            fixed (EventFramework* frameworkPtr = &framework)
            {
                eventID       = 0;
                eventObjectID = 0;
                distance3D    = 0;
                position      = default;
                
                if (frameworkPtr == null) return false;

                List<(Vector3 Position, uint EventId, ulong ObjectID, float Distance3D)> candidates = [];
                foreach (var eve in frameworkPtr->EventHandlerModule.EventHandlerMap)
                {
                    var value = eve.Item2.Value;
                    if (value->EventObjects.Count != 0 && conditionInfo(value->Info))
                    {
                        foreach (var obj in value->EventObjects)
                        {
                            if (obj.Value == null || !conditionObj(*obj.Value)) continue;
                            candidates.Add((obj.Value->Position, value->Info.EventId, obj.Value->GetGameObjectId(), Vector3.Distance(source, obj.Value->Position)));
                        }
                    }
                }

                if (candidates.Count == 0) return false;

                var first = candidates.OrderBy(x => x.Distance3D).FirstOrDefault();

                eventID       = first.EventId;
                eventObjectID = first.ObjectID;
                distance3D    = first.Distance3D;
                position      = first.Position;
                return true;
            }
        }

        public bool TryGetEvents(
            Predicate<EventHandlerInfo>                                               conditionInfo,
            Predicate<GameObject>                                                     conditionObj,
            Vector3                                                                   source,
            out List<(EventId EventID, GameObjectId EventObjectID, Vector3 Position)> result)
        {
            fixed (EventFramework* frameworkPtr = &framework)
            {
                result = [];
                
                if (frameworkPtr == null) return false;

                List<(Vector3 Position, EventId EventId, GameObjectId ObjectID)> candidates = [];
                foreach (var eve in frameworkPtr->EventHandlerModule.EventHandlerMap)
                {
                    var value = eve.Item2.Value;
                    if (value->EventObjects.Count != 0 && conditionInfo(value->Info))
                    {
                        foreach (var obj in value->EventObjects)
                        {
                            if (obj.Value == null || !conditionObj(*obj.Value)) continue;
                            candidates.Add((obj.Value->Position, value->Info.EventId, obj.Value->GetGameObjectId()));
                        }
                    }
                }

                if (candidates.Count == 0) return false;

                result = candidates.OrderBy(x => Vector3.DistanceSquared(source, x.Position))
                                   .Select(x => (x.EventId, x.ObjectID, x.Position))
                                   .ToList();
                return true;
            }
        }
    }
}
