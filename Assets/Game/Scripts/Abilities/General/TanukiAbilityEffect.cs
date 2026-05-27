using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class TanukiAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "ReplaceAdjacentCommonWithRandomRareAnimal", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var candidates = targets
                .Where(animal => animal?.Data != null && animal.Data.Rarity == 0)
                .ToList();
            if (candidates.Count == 0)
            {
                return false;
            }

            var selectedTarget = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            return context.RanchManager.TryReplaceAnimalWithRandomRaritySilently(selectedTarget, 1, 4);
        }
    }
}
