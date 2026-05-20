using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class ZebraAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || abilityData == null)
            {
                return false;
            }

            var bonus = abilityData.EffectParams != null ? abilityData.EffectParams.money : 0;
            if (bonus == 0)
            {
                return false;
            }

            context.Owner.AddPermanentBaseMoneyBonus(bonus);
            return true;
        }
    }
}
