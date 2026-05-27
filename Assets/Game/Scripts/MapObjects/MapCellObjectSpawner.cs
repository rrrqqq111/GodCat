using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.MapObjects
{
    public static class MapCellObjectSpawner
    {
        public static bool TrySpawnLeftoverMeat(RanchManager manager, Vector2Int coords, Animal sourceAnimal)
        {
            return TrySpawn(manager, coords, sourceAnimal, "LeftoverMeatReward");
        }

        public static bool TrySpawnPoop(RanchManager manager, Vector2Int coords, Animal sourceAnimal)
        {
            return TrySpawn(manager, coords, sourceAnimal, "PoopReward");
        }

        private static bool TrySpawn(RanchManager manager, Vector2Int coords, Animal sourceAnimal, string effectScriptId)
        {
            if (manager == null || string.IsNullOrWhiteSpace(effectScriptId))
            {
                return false;
            }

            return manager.TryAddMapCellObject(coords, effectScriptId, sourceAnimal);
        }
    }
}
