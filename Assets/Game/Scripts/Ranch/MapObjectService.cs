using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.MapObjects;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class MapObjectService
    {
        private readonly RanchManager manager;
        private readonly RanchMap ranchMap;
        private readonly RanchEventHub eventHub;
        private readonly List<MapCellObjectRuntime> runtimeObjects = new List<MapCellObjectRuntime>();

        public MapObjectService(RanchManager manager, RanchMap ranchMap, RanchEventHub eventHub)
        {
            this.manager = manager;
            this.ranchMap = ranchMap;
            this.eventHub = eventHub;
        }

        public IReadOnlyList<MapCellObjectRuntime> RuntimeObjects => runtimeObjects;

        public bool TryAddMapCellObject(Vector2Int coords, string effectScriptId, Animal sourceAnimal = null)
        {
            if (ranchMap == null || string.IsNullOrWhiteSpace(effectScriptId) || !ranchMap.TryGetCell(coords, out var cell) || cell.MapObject != null)
            {
                return false;
            }

            var mapObject = new MapCellObjectRuntime(
                effectScriptId,
                effectScriptId,
                null,
                ResolveDefaultConsumeScope(effectScriptId),
                true,
                effectScriptId,
                new MapCellObjectEffectParams(),
                coords,
                sourceAnimal);

            if (!cell.TryPlaceMapObject(mapObject))
            {
                return false;
            }

            runtimeObjects.Add(mapObject);
            eventHub?.NotifyMapObjectAdded(mapObject);
            return true;
        }

        public bool TryConsumeMapObjectAt(Animal consumer, Vector2Int coords)
        {
            if (consumer == null || ranchMap == null || !ranchMap.TryGetCell(coords, out var cell) || cell.MapObject == null)
            {
                return false;
            }

            return TryConsumeMapObject(consumer, cell.MapObject);
        }

        public bool TryConsumeMapObject(Animal consumer, MapCellObjectRuntime mapObject)
        {
            if (consumer == null || mapObject == null || ranchMap == null || mapObject.EffectParams == null)
            {
                return false;
            }

            if (!IsAllowedConsumer(consumer, mapObject))
            {
                return false;
            }

            if (!MapCellObjectEffectRegistry.TryGet(mapObject.EffectScriptId, out var effect))
            {
                return false;
            }

            var context = new MapCellObjectUseContext(manager, consumer, mapObject);
            var result = effect.TryExecute(mapObject, context);
            eventHub?.NotifyMapObjectConsumed(context, result);
            if (!result.Success)
            {
                return false;
            }

            if (mapObject.ConsumeOnSuccess)
            {
                RemoveMapObject(mapObject);
            }

            return true;
        }

        public bool TryConsumeNearbyMapObjects(Animal consumer)
        {
            if (consumer == null || ranchMap == null || !ranchMap.TryGetCell(consumer.Coords, out var originCell))
            {
                return false;
            }

            var consumedAny = false;
            foreach (var cell in GetCandidateCells(consumer, originCell.Coords))
            {
                if (cell.MapObject == null)
                {
                    continue;
                }

                consumedAny |= TryConsumeMapObject(consumer, cell.MapObject);
            }

            return consumedAny;
        }

        private IEnumerable<MapCell> GetCandidateCells(Animal consumer, Vector2Int originCoords)
        {
            if (ranchMap.TryGetCell(originCoords, out var selfCell))
            {
                yield return selfCell;
            }

            foreach (var neighbor in ranchMap.GetNeighbors(originCoords))
            {
                yield return neighbor;
            }
        }

        private bool IsAllowedConsumer(Animal consumer, MapCellObjectRuntime mapObject)
        {
            if (consumer == null || mapObject == null)
            {
                return false;
            }

            if (consumer.Ability is not IMapCellObjectConsumerAbility consumerAbility || !consumerAbility.CanConsumeMapCellObjects)
            {
                return false;
            }

            switch (mapObject.ConsumeScope)
            {
                case MapCellObjectConsumeScope.Self:
                    return consumer.Coords == mapObject.Coords;
                case MapCellObjectConsumeScope.Adjacent:
                    return ranchMap.GetNeighbors(consumer.Coords).Any(cell => cell != null && cell.MapObject == mapObject);
                case MapCellObjectConsumeScope.SelfAndAdjacent:
                    return consumer.Coords == mapObject.Coords ||
                        ranchMap.GetNeighbors(consumer.Coords).Any(cell => cell != null && cell.MapObject == mapObject);
                case MapCellObjectConsumeScope.Any:
                    return true;
                default:
                    return false;
            }
        }

        private static MapCellObjectConsumeScope ResolveDefaultConsumeScope(string effectScriptId)
        {
            if (string.Equals(effectScriptId, "PoopReward", System.StringComparison.OrdinalIgnoreCase))
            {
                return MapCellObjectConsumeScope.Self;
            }

            if (string.Equals(effectScriptId, "LeftoverMeatReward", System.StringComparison.OrdinalIgnoreCase))
            {
                return MapCellObjectConsumeScope.Adjacent;
            }

            return MapCellObjectConsumeScope.Any;
        }

        private void RemoveMapObject(MapCellObjectRuntime mapObject)
        {
            if (mapObject == null || ranchMap == null)
            {
                return;
            }

            if (ranchMap.TryRemoveMapObject(mapObject))
            {
                var index = runtimeObjects.FindIndex(item => item == mapObject);
                if (index >= 0)
                {
                    runtimeObjects.RemoveAt(index);
                }

                eventHub?.NotifyMapObjectRemoved(mapObject);
            }
        }
    }
}
