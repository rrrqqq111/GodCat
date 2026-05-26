using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class RhinoAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData?.EffectParams == null)
            {
                return false;
            }

            var parameters = abilityData.EffectParams;
            var spawnCount = parameters.count > 0 ? parameters.count : 1;
            var targetFamily = string.IsNullOrWhiteSpace(parameters.targetFamily) ? "Hoofed" : parameters.targetFamily;
            var applied = false;
            for (var i = 0; i < spawnCount; i++)
            {
                if (!context.RanchManager.TryAddRandomAnimalFromFamily(targetFamily, parameters.money, out _))
                {
                    break;
                }

                applied = true;
            }

            return applied;
        }
    }
}
