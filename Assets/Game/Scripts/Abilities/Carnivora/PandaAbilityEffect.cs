using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class PandaAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager?.Map == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "AddBaseMoneyToRandomAdjacentAnimal", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var candidates = context.RanchManager.Map
                .GetNeighbors(context.Owner.Coords)
                .Select(cell => cell.Animal)
                .Where(animal => animal != null && animal != context.Owner)
                .ToList();
            if (candidates.Count == 0)
            {
                return false;
            }

            var bonus = abilityData.EffectParams != null && abilityData.EffectParams.money > 0
                ? abilityData.EffectParams.money
                : 4;
            var selectedAnimal = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            selectedAnimal.AddPermanentBaseMoneyBonus(bonus);
            return true;
        }
    }
}
