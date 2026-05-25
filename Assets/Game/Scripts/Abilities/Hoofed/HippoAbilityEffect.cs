using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class HippoAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null ||
                context.RemovedAnimal?.Data == null ||
                context.RanchManager?.Map == null ||
                abilityData?.EffectParams == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "AddBaseMoneyOnAdjacentAnimalRemoved", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var targetFamily = abilityData.EffectParams.targetFamily;
            if (!string.Equals(context.RemovedAnimal.Data.Family, targetFamily, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var removedCoords = context.RemovedCoords ?? context.RemovedAnimal.Coords;
            if (!context.RanchManager.Map.GetNeighbors(context.Owner.Coords).Any(cell => cell.Coords == removedCoords))
            {
                return false;
            }

            var bonus = abilityData.EffectParams.money;
            if (bonus == 0)
            {
                return false;
            }

            context.Owner.AddPermanentBaseMoneyBonus(bonus);
            return true;
        }
    }
}
