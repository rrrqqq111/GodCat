using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;

namespace NekogamiRanch.Abilities
{
    public class RaccoonAbilityEffect : IAbilityEffect
    {
        private readonly Dictionary<Animal, int> remainingCooldownByAnimal = new Dictionary<Animal, int>();

        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "CooldownMoney", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var cooldownTurns = Math.Max(1, abilityData.EffectParams != null ? abilityData.EffectParams.delayDays : 3);
            var money = abilityData.EffectParams != null ? abilityData.EffectParams.money : 0;
            if (money == 0)
            {
                return false;
            }

            if (!remainingCooldownByAnimal.TryGetValue(context.Owner, out var remainingCooldown))
            {
                remainingCooldown = cooldownTurns;
            }

            remainingCooldown = ReduceCooldown(
                context,
                abilityData,
                remainingCooldown,
                1,
                "NaturalDailyCooldownReduction");

            if (IsAdjacentToPuddle(context.RanchManager.Map, context.Owner))
            {
                remainingCooldown = ReduceCooldown(
                    context,
                    abilityData,
                    remainingCooldown,
                    1,
                    "PuddleCooldownBonusReduction");
            }

            if (remainingCooldown > 0)
            {
                remainingCooldownByAnimal[context.Owner] = remainingCooldown;
                return false;
            }

            remainingCooldownByAnimal[context.Owner] = cooldownTurns;
            context.RanchManager.AddMoney(money);
            return true;
        }

        private static int ReduceCooldown(
            AnimalAbilityContext context,
            AbilityData abilityData,
            int remainingCooldown,
            int amount,
            string reason)
        {
            if (amount <= 0 || remainingCooldown <= 0)
            {
                return remainingCooldown;
            }

            var previousCooldown = remainingCooldown;
            var newCooldown = Math.Max(0, remainingCooldown - amount);
            context.RanchManager.NotifyAnimalCooldownReduced(new AnimalCooldownReductionContext(
                context.Owner,
                previousCooldown - newCooldown,
                previousCooldown,
                newCooldown,
                context.Owner,
                abilityData.Id,
                reason));

            return newCooldown;
        }

        private static bool IsAdjacentToPuddle(RanchMap map, Animal animal)
        {
            return map != null &&
                animal != null &&
                map.GetNeighbors(animal.Coords).Any(cell => map.IsTileType(cell.Coords, RanchTileType.Puddle));
        }
    }
}
