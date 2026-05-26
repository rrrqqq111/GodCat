using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class MaleLionAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.Predator?.Data == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "AddBaseMoneyOnLionessPrey", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(context.Predator.Data.Id, "Lioness", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var bonus = abilityData.EffectParams != null && abilityData.EffectParams.money > 0
                ? abilityData.EffectParams.money
                : 2;
            context.Owner.AddPermanentBaseMoneyBonus(bonus);
            return true;
        }
    }
}
