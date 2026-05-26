using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class RainbowUnicornAbilityEffect : IAbilityEffect
    {
        private const string RemoveCounterKey = "RainbowUnicorn.RemovedAdjacentAnimals";

        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RemovedAnimal == null || context.RanchManager == null || abilityData?.EffectParams == null)
            {
                return false;
            }

            var parameters = abilityData.EffectParams;
            var requiredRemoveCount = parameters.count > 0 ? parameters.count : 3;
            var rewardItemCount = parameters.itemCount > 0 ? parameters.itemCount : 1;
            var removeCount = context.Owner.AddRuntimeCounter(RemoveCounterKey);
            if (removeCount < requiredRemoveCount)
            {
                return false;
            }

            var applied = false;
            for (var i = 0; i < rewardItemCount; i++)
            {
                applied |= context.RanchManager.TryAddRandomItem();
            }

            if (applied)
            {
                context.Owner.SetRuntimeCounter(RemoveCounterKey, removeCount - requiredRemoveCount);
            }

            return applied;
        }
    }
}
