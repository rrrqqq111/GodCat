using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class BadgerAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData?.EffectParams == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "MoneyBySameAnimalAlone", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var badgerCount = context.RanchManager.CountAnimalsById("Badger");
            var reward = abilityData.EffectParams.money != 0 ? abilityData.EffectParams.money : 4;
            var penalty = abilityData.EffectParams.count > 0 ? abilityData.EffectParams.count : 2;
            context.RanchManager.AddMoney(badgerCount <= 1 ? reward : -penalty);
            return true;
        }
    }
}
