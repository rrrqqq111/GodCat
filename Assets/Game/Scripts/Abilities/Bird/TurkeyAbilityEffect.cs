using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class TurkeyAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "GainMoneyOnPreyed", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var reward = abilityData.EffectParams != null && abilityData.EffectParams.money > 0
                ? abilityData.EffectParams.money
                : 5;
            context.RanchManager.AddMoney(reward);
            return true;
        }
    }
}
