using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class RoosterAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "ZeroBaseMoneyWhenAdjacentSameAnimal", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var ownerId = context.Owner.Data != null ? context.Owner.Data.Id : string.Empty;
            var hasAdjacentRooster = targets != null && targets.Any(target =>
                target?.Data != null &&
                string.Equals(target.Data.Id, ownerId, StringComparison.OrdinalIgnoreCase));
            if (!hasAdjacentRooster)
            {
                return false;
            }

            var baseMoney = context.Owner.BaseMoney;
            if (baseMoney == 0)
            {
                return false;
            }

            context.RanchManager.AddMoney(-baseMoney);
            return true;
        }
    }
}
