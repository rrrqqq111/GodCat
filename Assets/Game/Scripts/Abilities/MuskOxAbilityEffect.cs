using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class MuskOxAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || context.RanchManager.Map == null || abilityData == null)
            {
                return false;
            }

            var bonus = abilityData.EffectParams != null ? abilityData.EffectParams.money : 0;
            if (bonus == 0)
            {
                return false;
            }

            var targetMode = abilityData.EffectParams != null ? Normalize(abilityData.EffectParams.target) : string.Empty;
            var targetFamily = abilityData.EffectParams != null ? abilityData.EffectParams.targetFamily : "Hoofed";
            var applied = false;

            foreach (var cell in context.RanchManager.Map.GetCells())
            {
                var animal = cell.Animal;
                if (animal == null || animal.Coords.y != context.Owner.Coords.y)
                {
                    continue;
                }

                if (targetMode == "self" && animal != context.Owner)
                {
                    continue;
                }

                if (targetMode == "other" && animal == context.Owner)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(targetFamily) &&
                    !string.Equals(targetFamily, "None", StringComparison.OrdinalIgnoreCase) &&
                    (animal.Data == null || !string.Equals(animal.Data.Family, targetFamily, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                animal.AddBaseMoneyBonus(bonus);
                applied = true;
            }

            return applied;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }
    }
}
