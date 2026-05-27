using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class FlamingoAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null ||
                context.RanchManager == null ||
                context.RanchManager.Map == null ||
                abilityData?.EffectParams == null)
            {
                return false;
            }

            if (string.Equals(abilityData.EffectType, "AddBaseMoneyPerSameAnimalGroup", StringComparison.OrdinalIgnoreCase))
            {
                return AddBaseMoneyPerSameAnimalGroup(context, abilityData);
            }

            if (string.Equals(abilityData.EffectType, "BreedOnAdjacentPuddle", StringComparison.OrdinalIgnoreCase))
            {
                return BreedOnAdjacentPuddle(context, abilityData);
            }

            return false;
        }

        private static bool AddBaseMoneyPerSameAnimalGroup(AnimalAbilityContext context, AbilityData abilityData)
        {
            var ownerId = context.Owner.Data != null ? context.Owner.Data.Id : string.Empty;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return false;
            }

            var requiredCount = abilityData.EffectParams.count > 0 ? abilityData.EffectParams.count : 4;
            var groupCount = context.RanchManager.CountAnimalsById(ownerId) / requiredCount;
            if (groupCount <= 0)
            {
                return false;
            }

            var reward = context.Owner.BaseMoney * groupCount;
            if (reward == 0)
            {
                return false;
            }

            context.RanchManager.AddMoney(reward);
            return true;
        }

        private static bool BreedOnAdjacentPuddle(AnimalAbilityContext context, AbilityData abilityData)
        {
            var hasAdjacentPuddle = context.RanchManager.Map
                .GetNeighbors(context.Owner.Coords)
                .Any(cell => cell != null && context.RanchManager.Map.IsTileType(cell.Coords, RanchTileType.Puddle));
            if (!hasAdjacentPuddle)
            {
                return false;
            }

            var offspringData = abilityData.EffectParams.animalData != null
                ? abilityData.EffectParams.animalData
                : context.Owner.Data;
            if (offspringData == null)
            {
                return false;
            }

            return TrySpawnOffspring(context, offspringData);
        }

        private static bool TrySpawnOffspring(AnimalAbilityContext context, AnimalData offspringData)
        {
            var map = context.RanchManager.Map;
            var candidateCoords = map.GetNeighbors(context.Owner.Coords)
                .Where(cell => cell != null && cell.IsEmpty)
                .Select(cell => cell.Coords)
                .ToList();

            if (candidateCoords.Count == 0)
            {
                candidateCoords = map.GetCells()
                    .Where(cell => cell != null && cell.IsEmpty)
                    .Select(cell => cell.Coords)
                    .ToList();
            }

            foreach (var coords in candidateCoords.OrderBy(_ => UnityEngine.Random.value))
            {
                if (context.RanchManager.TryAddAnimalAtEmptyCell(coords, offspringData, out _))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
