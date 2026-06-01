using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;

namespace NekogamiRanch.Abilities
{
    public class ParrotAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "GainMoneyOnAdjacentBaseMoneyIncrease", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var baseMoneyIncrease = context.Owner.GetRuntimeCounter(RanchBaseMoneyBonusTriggerService.IncreasedAmountCounterKey);
            if (baseMoneyIncrease <= 0)
            {
                return false;
            }

            var multiplier = abilityData.EffectParams != null && abilityData.EffectParams.minMultiplier > 0
                ? abilityData.EffectParams.minMultiplier
                : 3;
            context.RanchManager.AddMoney(baseMoneyIncrease * multiplier);
            return true;
        }
    }
}
