using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class SaltwaterCrocodileAbilityEffect : IAbilityEffect
    {
        private static readonly string[] ExcludedAnimalIds = { "Crocodile", "SaltwaterCrocodile" };
        private static readonly string[] ExcludedFamilies = { "Reptilia" };

        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "PreyNonReptileForLevelCans", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var result = context.RanchManager.TryPrey(new PreyContext(
                context.Owner,
                new PreyTargetRule(
                    abilityData.ImpactType,
                    excludeAnimalIds: ExcludedAnimalIds,
                    excludeFamilies: ExcludedFamilies,
                    targetCount: 1,
                    randomPick: true),
                sourceAbilityId: abilityData.Id));

            if (!result.Success || result.RemovedTargets.Count == 0)
            {
                return false;
            }

            context.RanchManager.AddCans(Math.Max(1, context.Owner.EvolutionLevel));
            return true;
        }
    }
}
