using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class FoxAbilityEffect : IAbilityEffect
    {
        private static readonly string[] TargetAnimalIds = { "Chicken", "Rooster", "Hen", "Turkey" };

        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "PreyAdjacentPoultryAndGainMoney", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var result = context.RanchManager.TryPrey(new PreyContext(
                context.Owner,
                new PreyTargetRule(
                    abilityData.ImpactType,
                    targetAnimalIds: TargetAnimalIds,
                    targetCount: 1,
                    randomPick: true),
                sourceAbilityId: abilityData.Id));
            if (!result.Success || result.RemovedTargets.Count == 0)
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
