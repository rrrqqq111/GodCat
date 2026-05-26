using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class CheetahAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null ||
                context.MovedAnimal == null ||
                !context.MovedFromCoords.HasValue ||
                context.RanchManager?.Map == null ||
                abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "FollowAdjacentMovedAnimal", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var destination = context.MovedFromCoords.Value;
            if (!context.RanchManager.Map.TryGetCell(destination, out var destinationCell) ||
                !destinationCell.IsEmpty ||
                !context.RanchManager.TryMoveAnimal(context.Owner, destination))
            {
                return false;
            }

            var reward = abilityData.EffectParams != null && abilityData.EffectParams.money > 0
                ? abilityData.EffectParams.money
                : 4;
            context.RanchManager.AddMoney(reward);
            return true;
        }
    }
}
