using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class MagpieAbilityEffect : IAbilityEffect
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

            if (!string.Equals(abilityData.EffectType, "AddBaseMoneyPerAnimalKindCount", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var requiredCount = abilityData.EffectParams.count > 0 ? abilityData.EffectParams.count : 3;
            var bonusPerKind = abilityData.EffectParams.money > 0 ? abilityData.EffectParams.money : 1;
            var qualifiedKindCount = context.RanchManager.Map
                .GetCellsInScanOrder()
                .Select(cell => cell?.Animal?.Data?.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .GroupBy(id => id, StringComparer.OrdinalIgnoreCase)
                .Count(group => group.Count() >= requiredCount);

            var bonus = qualifiedKindCount * bonusPerKind;
            if (bonus <= 0)
            {
                return false;
            }

            context.RanchManager.AddAnimalBaseMoneyBonus(context.Owner, bonus);
            return true;
        }
    }
}
