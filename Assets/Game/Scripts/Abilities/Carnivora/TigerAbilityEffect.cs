using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class TigerAbilityEffect : IAbilityEffect
    {
        private const int MaxAdjacentTargets = 6;

        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "Prey", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var result = context.RanchManager.TryPrey(new PreyContext(
                context.Owner,
                new PreyTargetRule(
                    abilityData.ImpactType,
                    targetFamilies: new[] { "Hoofed" },
                    targetCount: MaxAdjacentTargets),
                sourceAbilityId: abilityData.Id));

            if (!result.Success || result.RemovedTargets.Count == 0)
            {
                return false;
            }

            var bonusPerPrey = abilityData.EffectParams != null ? abilityData.EffectParams.money : 0;
            if (bonusPerPrey > 0)
            {
                context.Owner.AddPermanentBaseMoneyBonus(bonusPerPrey * result.RemovedTargets.Count);
            }

            return true;
        }
    }
}
