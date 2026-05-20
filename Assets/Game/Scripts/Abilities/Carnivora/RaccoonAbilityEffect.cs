using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class RaccoonAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "CooldownMoney", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var money = abilityData.EffectParams != null ? abilityData.EffectParams.money : 0;
            if (money == 0)
            {
                return false;
            }

            context.RanchManager.AddMoney(money);
            return true;
        }
    }
}
