using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class GooseAbilityEffect : IAbilityEffect, IPassiveProtectionEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            return false;
        }

        public ProtectionRule CreateProtectionRule(Animal protector, AbilityData abilityData)
        {
            return protector == null
                ? null
                : new ProtectionRule(
                    "Adjacent",
                    protectedFamilies: new[] { "Bird" },
                    protector: protector,
                    reason: abilityData != null ? abilityData.Id : "GooseProtection");
        }

        public void OnProtected(Animal protector, Animal protectedAnimal, AbilityData abilityData, Action<int> addMoney)
        {
        }
    }
}
