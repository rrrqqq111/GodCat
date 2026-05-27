using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class SwanCountAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData?.EffectParams == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "AddMoneyBySameAnimalCount", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var requiredCount = abilityData.EffectParams.count > 0 ? abilityData.EffectParams.count : 3;
            var ownerId = context.Owner.Data != null ? context.Owner.Data.Id : string.Empty;
            if (string.IsNullOrWhiteSpace(ownerId) || context.RanchManager.CountAnimalsById(ownerId) < requiredCount)
            {
                return false;
            }

            var reward = abilityData.EffectParams.money != 0 ? abilityData.EffectParams.money : 1;
            context.RanchManager.AddMoney(reward);
            return true;
        }
    }
}
