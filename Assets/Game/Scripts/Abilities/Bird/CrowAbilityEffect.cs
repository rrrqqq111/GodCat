using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class CrowAbilityEffect : IAbilityEffect
    {
        private static readonly string[] TargetFamilies = { "Insect", "Bug" };

        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null ||
                context.RanchManager == null ||
                abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "PreyInsectForCrow", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var maxRarity = abilityData.EffectParams != null && abilityData.EffectParams.maxRarity >= 0
                ? abilityData.EffectParams.maxRarity
                : 1;
            var result = context.RanchManager.TryPrey(new PreyContext(
                context.Owner,
                new PreyTargetRule(
                    abilityData.ImpactType,
                    targetFamilies: TargetFamilies,
                    targetCount: 1,
                    randomPick: true,
                    maxRarity: maxRarity),
                sourceAbilityId: abilityData.Id));

            if (!result.Success || result.RemovedTargets.Count == 0)
            {
                return false;
            }

            var offspringData = abilityData.EffectParams != null && abilityData.EffectParams.animalData != null
                ? abilityData.EffectParams.animalData
                : context.Owner.Data;
            return context.RanchManager.TryAddAnimalToRandomEmptyCell(offspringData);
        }
    }
}
