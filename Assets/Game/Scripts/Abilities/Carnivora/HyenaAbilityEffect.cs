using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class HyenaAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null ||
                context.RanchManager?.Map == null ||
                abilityData == null)
            {
                return false;
            }

            var parameters = abilityData.EffectParams;
            if (string.Equals(abilityData.EffectType, "GainMoneyOnAdjacentAnimalPreyed", StringComparison.OrdinalIgnoreCase))
            {
                if (context.PreyedAnimal == null)
                {
                    return false;
                }

                var reward = parameters != null && parameters.money > 0 ? parameters.money : 3;
                context.RanchManager.AddMoney(reward);
                return true;
            }

            if (!string.Equals(abilityData.EffectType, "AmbushRightCarnivore", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var targetCoords = context.Owner.Coords + UnityEngine.Vector2Int.right;
            if (!context.RanchManager.Map.TryGetCell(targetCoords, out var targetCell) ||
                targetCell.Animal == null ||
                targetCell.Animal.Data == null ||
                !string.Equals(targetCell.Animal.Data.Family, "Carnivora", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var result = context.RanchManager.TryPrey(new PreyContext(
                context.Owner,
                new PreyTargetRule(
                    "Right",
                    targetFamilies: new[] { "Carnivora" },
                    targetCount: 1),
                sourceAbilityId: abilityData.Id));
            if (!result.Success || result.RemovedTargets.Count == 0)
            {
                return false;
            }

            var ambushReward = parameters != null && parameters.money > 0 ? parameters.money : 20;
            context.RanchManager.AddMoney(ambushReward);
            return true;
        }
    }
}
