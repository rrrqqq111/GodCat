using System.Collections.Generic;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using NekogamiRanch.Terrains;

namespace NekogamiRanch.Abilities
{
    public class WaterBuffaloAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || context.RanchManager.Map == null || abilityData == null)
            {
                return false;
            }

            if (!context.RanchManager.Map.IsTerrain(context.Owner, RanchTerrainIds.Puddle))
            {
                return false;
            }

            var bonus = abilityData.EffectParams != null ? abilityData.EffectParams.money : 0;
            if (bonus == 0)
            {
                return false;
            }

            context.RanchManager.AddAnimalBaseMoneyBonus(context.Owner, bonus);
            return true;
        }
    }
}
