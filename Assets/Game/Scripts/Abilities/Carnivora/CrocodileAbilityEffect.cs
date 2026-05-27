using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class CrocodileAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "PreyHoofedByMaxRarityForLevelMoney", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var maxRarity = abilityData.EffectParams != null ? abilityData.EffectParams.maxRarity : 1;
            var result = context.RanchManager.TryPrey(new PreyContext(
                context.Owner,
                new PreyTargetRule(
                    abilityData.ImpactType,
                    targetFamilies: new[] { "Hoofed" },
                    targetCount: 1,
                    randomPick: true,
                    maxRarity: maxRarity),
                sourceAbilityId: abilityData.Id));

            if (!result.Success || result.RemovedTargets.Count == 0)
            {
                return false;
            }

            context.RanchManager.AddMoney(Math.Max(1, context.Owner.EvolutionLevel));
            return true;
        }
    }
}
