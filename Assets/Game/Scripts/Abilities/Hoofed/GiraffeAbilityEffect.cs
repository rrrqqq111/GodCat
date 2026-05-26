using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class GiraffeAbilityEffect : IAbilityEffect, IPassiveProtectionEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            return false;
        }

        public ProtectionRule CreateProtectionRule(Animal protector, AbilityData abilityData)
        {
            return protector == null
                ? null
                : new ProtectionRule("Global", protector: protector, reason: abilityData != null ? abilityData.Id : "GiraffeProtection");
        }

        public void OnProtected(Animal protector, Animal protectedAnimal, AbilityData abilityData, Action<int> addMoney)
        {
            if (protector == null || protectedAnimal == null)
            {
                return;
            }

            var bonus = abilityData?.EffectParams != null && abilityData.EffectParams.money > 0
                ? abilityData.EffectParams.money
                : 1;
            protector.AddPermanentBaseMoneyBonus(bonus);
        }
    }
}
