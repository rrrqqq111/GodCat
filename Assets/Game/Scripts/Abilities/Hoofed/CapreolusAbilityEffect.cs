using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class CapreolusAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager?.Map == null || abilityData?.EffectParams == null)
            {
                return false;
            }

            var parameters = abilityData.EffectParams;
            var sellCount = parameters.count > 0 ? parameters.count : 1;
            var excludedRarity = parameters.maxRarity > 0 ? parameters.maxRarity : 4;
            var minMultiplier = parameters.minMultiplier > 0 ? parameters.minMultiplier : 3;
            var maxMultiplier = Mathf.Max(minMultiplier, parameters.maxMultiplier > 0 ? parameters.maxMultiplier : 7);
            var selectedTargets = context.RanchManager.Map
                .GetNeighbors(context.Owner.Coords)
                .Select(cell => cell.Animal)
                .Where(animal => animal?.Data != null && animal.Data.Rarity < excludedRarity)
                .OrderBy(_ => Random.value)
                .Take(sellCount)
                .ToList();

            var applied = false;
            foreach (var target in selectedTargets)
            {
                var sellMoney = target.BaseMoney * Random.Range(minMultiplier, maxMultiplier + 1);
                if (!context.RanchManager.SellAnimal(target))
                {
                    continue;
                }

                context.RanchManager.AddMoney(sellMoney);
                applied = true;
            }

            return applied;
        }
    }
}
