using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;

namespace NekogamiRanch.Abilities
{
    public class SwanPuddleAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager?.Map == null || abilityData?.EffectParams == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "AddMoneyOnAdjacentPuddle", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var hasAdjacentPuddle = context.RanchManager.Map
                .GetNeighbors(context.Owner.Coords)
                .Any(cell => cell != null && context.RanchManager.Map.IsTileType(cell.Coords, RanchTileType.Puddle));
            if (!hasAdjacentPuddle)
            {
                return false;
            }

            var reward = abilityData.EffectParams.money != 0 ? abilityData.EffectParams.money : 2;
            context.RanchManager.AddMoney(reward);
            return true;
        }
    }
}
