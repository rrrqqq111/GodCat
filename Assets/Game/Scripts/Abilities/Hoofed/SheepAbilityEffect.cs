using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class SheepAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "SellSelf", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var sellMoney = abilityData.EffectParams != null ? abilityData.EffectParams.money : 0;
            if (!context.RanchManager.SellAnimal(context.Owner))
            {
                return false;
            }

            context.RanchManager.AddMoney(sellMoney);
            return true;
        }
    }
}
