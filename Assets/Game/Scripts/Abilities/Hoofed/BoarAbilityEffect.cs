using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class BoarAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            var effectParams = abilityData.EffectParams;
            var bonusMoney = effectParams != null ? effectParams.money : 0;
            var applied = false;

            var boarCount = context.RanchManager.CountAnimalsById("Boar");
            if (boarCount >= 2 && bonusMoney > 0)
            {
                context.RanchManager.AddMoney(bonusMoney);
                applied = true;
            }

            var pigTargets = targets
                .Where(animal => animal != null && animal.Data != null && string.Equals(animal.Data.Id, "Pig", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var transformChance = effectParams != null ? effectParams.transformChancePercent : 100;
            transformChance = Mathf.Clamp(transformChance, 0, 100);
            var transformPassed = transformChance >= 100 || UnityEngine.Random.Range(0, 100) < transformChance;

            if (transformPassed && pigTargets.Count > 0)
            {
                var selectedPig = pigTargets[UnityEngine.Random.Range(0, pigTargets.Count)];
                if (context.RanchManager.TransformAnimal(selectedPig, context.Owner.Data))
                {
                    applied = true;
                }
            }

#if UNITY_EDITOR
            Debug.Log($"[BoarAbilityEffect] owner={context.Owner.DisplayName} boarCount={boarCount}, bonusMoney={(boarCount >= 2 ? bonusMoney : 0)}, transformChance={transformChance}, transformPassed={transformPassed}, pigTargets={pigTargets.Count}, applied={applied}");
#endif

            return applied;
        }
    }
}
