using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class BrownBearAbilityEffect : IAbilityEffect
    {
        private static readonly string[] TargetFamilies =
        {
            "Ocean",
            "Marine",
            "Aquatic",
            "Insect",
            "Bug",
            "RodentRabbit",
            "Rodent",
            "Lagomorph"
        };

        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "PreyTargetFamiliesForBaseMoney", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var result = context.RanchManager.TryPrey(new PreyContext(
                context.Owner,
                new PreyTargetRule(
                    abilityData.ImpactType,
                    targetFamilies: TargetFamilies,
                    targetCount: 1,
                    randomPick: true),
                sourceAbilityId: abilityData.Id));
            if (!result.Success || result.RemovedTargets.Count == 0)
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
