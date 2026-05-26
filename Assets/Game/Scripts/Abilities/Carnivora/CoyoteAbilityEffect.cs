using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class CoyoteAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager?.Map == null || abilityData?.EffectParams == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "MoneyByFamilyCountLimit", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var parameters = abilityData.EffectParams;
            var family = string.IsNullOrWhiteSpace(parameters.targetFamily) ||
                string.Equals(parameters.targetFamily, "None", StringComparison.OrdinalIgnoreCase)
                ? "Carnivora"
                : parameters.targetFamily;
            var maxCount = parameters.maxCount > 0 ? parameters.maxCount : 3;
            var familyCount = context.RanchManager.Map
                .GetCellsInScanOrder()
                .Select(cell => cell.Animal)
                .Count(animal => animal?.Data != null && string.Equals(animal.Data.Family, family, StringComparison.OrdinalIgnoreCase));
            var reward = parameters.money != 0 ? parameters.money : 2;
            var penalty = parameters.count > 0 ? parameters.count : 1;

            context.RanchManager.AddMoney(familyCount <= maxCount ? reward : -penalty);
            return true;
        }
    }
}
