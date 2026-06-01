using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;

namespace NekogamiRanch.Abilities
{
    public class TerrorBirdAbilityEffect : IAbilityEffect
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

            if (!string.Equals(abilityData.EffectType, "RemoveNonExtinctAnimalsAndGainBaseMoney", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var excludedRarity = abilityData.EffectParams.maxRarity > 0 ? abilityData.EffectParams.maxRarity : 4;
            var targetsToRemove = context.RanchManager.Map
                .GetCellsInScanOrder()
                .Select(cell => cell?.Animal)
                .Where(animal => animal != null &&
                    animal != context.Owner &&
                    (animal.Data == null || animal.Data.Rarity != excludedRarity))
                .Distinct()
                .ToList();

            if (targetsToRemove.Count <= 0)
            {
                return false;
            }

            var baseMoneyGain = targetsToRemove.Sum(animal => animal.BaseMoney);
            foreach (var animal in targetsToRemove)
            {
                context.RanchManager.RemoveAnimalSilently(animal);
            }

            if (baseMoneyGain != 0)
            {
                context.RanchManager.AddAnimalBaseMoneyBonus(context.Owner, baseMoneyGain);
            }

            return true;
        }
    }
}
