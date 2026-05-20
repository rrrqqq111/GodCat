using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class GoatAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null ||
                context.RanchManager == null ||
                context.RanchManager.Map == null ||
                abilityData?.EffectParams?.animalData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "Spawn", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return TrySpawnLamb(context, abilityData.EffectParams.animalData);
        }

        private static bool TrySpawnLamb(AnimalAbilityContext context, AnimalData lambData)
        {
            var map = context.RanchManager.Map;
            if (TryPlaceLambAt(context, lambData, map, context.Owner.Coords))
            {
                return true;
            }

            foreach (var cell in map.GetCellsInScanOrder())
            {
                if (cell != null && cell.IsEmpty && TryPlaceLambAt(context, lambData, map, cell.Coords))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryPlaceLambAt(AnimalAbilityContext context, AnimalData lambData, RanchMap map, Vector2Int coords)
        {
            return map.TryGetCell(coords, out var cell) &&
                cell.IsEmpty &&
                context.RanchManager.TrySetAnimalAt(coords, lambData);
        }
    }
}
