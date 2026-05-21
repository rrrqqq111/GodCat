using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class LambAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "GrowUp", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var growUpAnimalData = RollGrowUpAnimalData(abilityData.EffectParams);
            return growUpAnimalData != null && context.RanchManager.GrowAnimal(context.Owner, growUpAnimalData);
        }

        private static AnimalData RollGrowUpAnimalData(AbilityEffectParams effectParams)
        {
            if (effectParams == null)
            {
                return null;
            }

            var candidates = new List<WeightedAnimalData>
            {
                new WeightedAnimalData(effectParams.growUpAnimalDataA, effectParams.growUpWeightA),
                new WeightedAnimalData(effectParams.growUpAnimalDataB, effectParams.growUpWeightB),
                new WeightedAnimalData(effectParams.growUpAnimalDataC, effectParams.growUpWeightC)
            };

            var totalWeight = 0;
            for (var i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].AnimalData != null && candidates[i].Weight > 0)
                {
                    totalWeight += candidates[i].Weight;
                }
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            var roll = UnityEngine.Random.Range(0, totalWeight);
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate.AnimalData == null || candidate.Weight <= 0)
                {
                    continue;
                }

                if (roll < candidate.Weight)
                {
                    return candidate.AnimalData;
                }

                roll -= candidate.Weight;
            }

            return null;
        }

        private readonly struct WeightedAnimalData
        {
            public WeightedAnimalData(AnimalData animalData, int weight)
            {
                AnimalData = animalData;
                Weight = weight;
            }

            public AnimalData AnimalData { get; }
            public int Weight { get; }
        }
    }
}
