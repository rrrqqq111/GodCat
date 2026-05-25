using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class ReindeerAbilityEffect : IAbilityEffect
    {
        private const int DefaultMaxTargetCount = 3;

        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager?.Map == null || abilityData?.EffectParams == null)
            {
                return false;
            }

            var bonus = abilityData.EffectParams.money;
            if (bonus == 0)
            {
                return false;
            }

            var maxTargetCount = abilityData.EffectParams.maxCount > 0
                ? abilityData.EffectParams.maxCount
                : DefaultMaxTargetCount;
            var targetCount = Mathf.Min(context.Owner.AgeDays + 1, maxTargetCount);
            if (targetCount <= 0)
            {
                return false;
            }

            var selectedTargets = context.RanchManager.Map
                .GetNeighbors(context.Owner.Coords)
                .OrderBy(_ => Random.value)
                .Take(targetCount)
                .Select(cell => cell.Animal)
                .Where(target => target != null && target != context.Owner)
                .ToList();
            foreach (var target in selectedTargets)
            {
                target.AddPermanentBaseMoneyBonus(bonus);
            }

            return selectedTargets.Count > 0;
        }
    }
}
