using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class DuckAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData?.EffectParams == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "AddMoneyToAdjacentLowBaseMoneyAnimals", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var targetCount = targets != null
                ? targets.Count(target => target != null && target.BaseMoney < 1)
                : 0;
            if (targetCount <= 0)
            {
                return false;
            }

            var rewardPerTarget = abilityData.EffectParams.money != 0 ? abilityData.EffectParams.money : 2;
            context.RanchManager.AddMoney(rewardPerTarget * targetCount);
            return true;
        }
    }
}
