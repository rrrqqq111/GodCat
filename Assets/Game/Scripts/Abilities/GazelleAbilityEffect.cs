using System.Collections.Generic;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class GazelleAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || context.RanchManager.Map == null || abilityData == null)
            {
                return false;
            }

            var aboveCoords = context.Owner.Coords + Vector2Int.up;
            if (!context.RanchManager.Map.TryGetCell(aboveCoords, out var aboveCell) || aboveCell.Animal == null)
            {
                return false;
            }

            if (!context.RanchManager.TrySwapAnimals(context.Owner, aboveCell.Animal))
            {
                return false;
            }

            var reward = abilityData.EffectParams != null ? abilityData.EffectParams.money : 0;
            if (reward > 0)
            {
                context.RanchManager.AddMoney(reward);
            }

            return true;
        }
    }
}
