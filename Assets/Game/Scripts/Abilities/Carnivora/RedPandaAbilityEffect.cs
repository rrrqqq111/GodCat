using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class RedPandaAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager?.Map == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "SpawnRandomOtherFamilyAdjacent", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var emptyNeighbors = context.RanchManager.Map
                .GetNeighbors(context.Owner.Coords)
                .Where(cell => cell != null && cell.IsEmpty)
                .ToList();
            if (emptyNeighbors.Count == 0)
            {
                return false;
            }

            var excludedFamily = abilityData.EffectParams != null &&
                !string.IsNullOrWhiteSpace(abilityData.EffectParams.targetFamily) &&
                !string.Equals(abilityData.EffectParams.targetFamily, "None", StringComparison.OrdinalIgnoreCase)
                    ? abilityData.EffectParams.targetFamily
                    : "Carnivora";
            var destination = emptyNeighbors[UnityEngine.Random.Range(0, emptyNeighbors.Count)].Coords;
            return context.RanchManager.TryAddRandomAnimalOutsideFamilyAt(excludedFamily, destination, out _);
        }
    }
}
