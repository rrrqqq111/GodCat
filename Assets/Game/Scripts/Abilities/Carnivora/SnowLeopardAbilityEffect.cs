using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class SnowLeopardAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "PreyHoofedForTargetBaseMoneyMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var result = context.RanchManager.TryPrey(new PreyContext(
                context.Owner,
                new PreyTargetRule(
                    abilityData.ImpactType,
                    targetFamilies: new[] { "Hoofed" },
                    targetCount: 1,
                    randomPick: true),
                sourceAbilityId: abilityData.Id));
            if (!result.Success || result.RemovedTargets.Count == 0)
            {
                return false;
            }

            var multiplier = abilityData.EffectParams != null && abilityData.EffectParams.minMultiplier > 0
                ? abilityData.EffectParams.minMultiplier
                : 2;
            context.Owner.AddPermanentBaseMoneyBonus(result.RemovedTargets[0].BaseMoney * multiplier);
            return true;
        }
    }
}
