using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class DonkeyAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!context.RanchManager.HasAnimalById("Horse") && !context.RanchManager.HasAnimalById("Zebra"))
            {
                return false;
            }

            var reward = abilityData.EffectParams != null ? abilityData.EffectParams.money : 0;
            if (reward <= 0)
            {
                return false;
            }

            context.RanchManager.AddMoney(reward);
            return true;
        }
    }
}
