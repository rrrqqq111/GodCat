using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class PeacockAbilityEffect : IAbilityEffect
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

            if (!string.Equals(abilityData.EffectType, "AdjacentBirdsGainBaseMoneyMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var targetFamily = abilityData.EffectParams.targetFamily;
            if (string.IsNullOrWhiteSpace(targetFamily) || string.Equals(targetFamily, "None", StringComparison.OrdinalIgnoreCase))
            {
                targetFamily = "Bird";
            }

            var adjacentBirds = context.RanchManager.Map
                .GetNeighbors(context.Owner.Coords)
                .Select(cell => cell?.Animal)
                .Where(animal => animal?.Data != null && animal.Data.HasFamily(targetFamily))
                .ToList();
            if (adjacentBirds.Count <= 0)
            {
                return false;
            }

            var sameAnimalKinds = adjacentBirds
                .Select(animal => animal.Data.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
            var normalMultiplier = abilityData.EffectParams.minMultiplier > 0
                ? abilityData.EffectParams.minMultiplier
                : 2;
            var sameKindMultiplier = abilityData.EffectParams.maxMultiplier > 0
                ? abilityData.EffectParams.maxMultiplier
                : 3;
            var multiplier = sameAnimalKinds == 1 ? sameKindMultiplier : normalMultiplier;

            var applied = false;
            foreach (var bird in adjacentBirds)
            {
                var reward = bird.BaseMoney * multiplier;
                if (reward <= 0)
                {
                    continue;
                }

                context.RanchManager.AddExtraMoney(bird, reward);
                applied = true;
            }

            return applied;
        }
    }
}
