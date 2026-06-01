using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class PigeonAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null ||
                context.RanchManager == null ||
                context.RanchManager.Map == null ||
                abilityData?.EffectParams == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "AddMoneyPerAdjacentSameAnimal", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var ownerId = context.Owner.Data != null ? context.Owner.Data.Id : string.Empty;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                return false;
            }

            var adjacentSameAnimalCount = context.RanchManager.Map
                .GetNeighbors(context.Owner.Coords)
                .Count(cell => string.Equals(cell?.Animal?.Data?.Id, ownerId, StringComparison.OrdinalIgnoreCase));
            if (adjacentSameAnimalCount <= 0)
            {
                return false;
            }

            var reward = abilityData.EffectParams.money != 0 ? abilityData.EffectParams.money : 2;
            context.RanchManager.AddMoney(reward * adjacentSameAnimalCount);
            return true;
        }
    }
}
