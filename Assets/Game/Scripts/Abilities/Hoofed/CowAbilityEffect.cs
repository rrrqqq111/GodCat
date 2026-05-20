using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class CowAbilityEffect : IAbilityEffect
    {
        private static readonly HashSet<string> TriggerAnimalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "WaterBuffalo",
            "Cow",
            "MuskOx"
        };

        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null ||
                context.RanchManager == null ||
                context.RanchManager.Map == null ||
                context.PreyedAnimal == null ||
                abilityData?.EffectParams?.animalData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "SpawnCalf", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(abilityData.EffectType, "Spawn", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var preyedAnimalId = context.PreyedAnimal.Data != null ? context.PreyedAnimal.Data.Id : string.Empty;
            if (!TriggerAnimalIds.Contains(preyedAnimalId) || !IsAdjacentToPreyedAnimal(context))
            {
                return false;
            }

            return TrySpawnCalf(context, abilityData.EffectParams.animalData);
        }

        private static bool TrySpawnCalf(AnimalAbilityContext context, AnimalData calfData)
        {
            var map = context.RanchManager.Map;
            var triggerCoords = context.PreyedAnimal.Coords;
            if (TryPlaceCalfAt(context, calfData, map, triggerCoords))
            {
                return true;
            }

            foreach (var cell in map.GetCellsInScanOrder())
            {
                if (cell != null && cell.IsEmpty && TryPlaceCalfAt(context, calfData, map, cell.Coords))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryPlaceCalfAt(AnimalAbilityContext context, AnimalData calfData, RanchMap map, Vector2Int coords)
        {
            return map.TryGetCell(coords, out var cell) &&
                cell.IsEmpty &&
                context.RanchManager.TrySetAnimalAt(coords, calfData);
        }

        private static bool IsAdjacentToPreyedAnimal(AnimalAbilityContext context)
        {
            var preyedCoords = context.PreyedAnimal.Coords;
            return context.RanchManager.Map
                .GetNeighbors(context.Owner.Coords)
                .Any(cell => cell != null && cell.Coords == preyedCoords);
        }
    }
}
