using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchMapObjectCommandService
    {
        private readonly MapObjectService mapObjectService;
        private readonly System.Action stateChanged;

        public RanchMapObjectCommandService(MapObjectService mapObjectService, System.Action stateChanged)
        {
            this.mapObjectService = mapObjectService;
            this.stateChanged = stateChanged;
        }

        public bool TryAddMapObject(Vector2Int coords, string effectScriptId, Animal sourceAnimal = null)
        {
            if (mapObjectService == null)
            {
                return false;
            }

            var added = mapObjectService.TryAddMapCellObject(coords, effectScriptId, sourceAnimal);
            if (added)
            {
                stateChanged?.Invoke();
            }

            return added;
        }

        public bool TryConsumeMapObjectAt(Animal consumer, Vector2Int coords)
        {
            if (mapObjectService == null)
            {
                return false;
            }

            var consumed = mapObjectService.TryConsumeMapObjectAt(consumer, coords);
            if (consumed)
            {
                stateChanged?.Invoke();
            }

            return consumed;
        }

        public bool TryConsumeNearbyMapObjects(Animal consumer)
        {
            if (mapObjectService == null)
            {
                return false;
            }

            var consumed = mapObjectService.TryConsumeNearbyMapObjects(consumer);
            if (consumed)
            {
                stateChanged?.Invoke();
            }

            return consumed;
        }
    }
}
