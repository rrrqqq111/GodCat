using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class SheepdogAbilityEffect : IAbilityEffect, IPassiveProtectionEffect
    {
        private static readonly string[] ProtectedAnimalIds = { "Lamb", "Sheep", "Goat", "Alpaca" };

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
                    protectedAnimalIds: ProtectedAnimalIds,
                    protector: protector,
                    reason: abilityData != null ? abilityData.Id : "SheepdogProtection");
        }

        public void OnProtected(Animal protector, Animal protectedAnimal, AbilityData abilityData, Action<int> addMoney)
        {
            if (protector == null || protectedAnimal == null || addMoney == null)
            {
                return;
            }

            var reward = abilityData?.EffectParams != null && abilityData.EffectParams.money > 0
                ? abilityData.EffectParams.money
                : 4;
            addMoney(reward);
        }
    }
}
