using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class SkunkAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager?.Map == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "MoneyByAdjacentEmptyCell", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var hasAdjacentEmptyCell = context.RanchManager.Map
                .GetNeighbors(context.Owner.Coords)
                .Any(cell => cell != null && cell.IsEmpty);
            var reward = abilityData.EffectParams != null && abilityData.EffectParams.money != 0
                ? abilityData.EffectParams.money
                : 2;
            var penalty = abilityData.EffectParams != null && abilityData.EffectParams.count > 0
                ? abilityData.EffectParams.count
                : 1;

            context.RanchManager.AddMoney(hasAdjacentEmptyCell ? reward : -penalty);
            return true;
        }
    }
}
