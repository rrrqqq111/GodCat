using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class AlpacaAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (abilityData == null || targets == null || targets.Count == 0)
            {
                return false;
            }

            var multiplier = abilityData.EffectParams != null ? abilityData.EffectParams.money : 1;
            if (multiplier <= 1)
            {
                return false;
            }

            foreach (var target in targets)
            {
                target.AddExtraMoneyMultiplier(multiplier, abilityData.Stackable);
            }

            return true;
        }
    }
}
