using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class CamelAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || context.RanchManager.Map == null)
            {
                return false;
            }

            var map = context.RanchManager.Map;
            if (!map.IsTileType(context.Owner, RanchTileType.Sand))
            {
                return false;
            }

            var adjacentAnimals = map.GetNeighbors(context.Owner.Coords)
                .Select(cell => cell.Animal)
                .Where(HasRegisteredAbilityEffect)
                .ToList();

            if (adjacentAnimals.Count == 0)
            {
                return false;
            }

            var selectedAnimal = adjacentAnimals[Random.Range(0, adjacentAnimals.Count)];
            return context.RanchManager.TryTriggerAnimalAbility(selectedAnimal);
        }

        private static bool HasRegisteredAbilityEffect(Animal animal)
        {
            var abilityData = animal?.Data?.Ability;
            return animal != null &&
                animal.Ability != null &&
                abilityData != null &&
                AbilityEffectRegistry.TryGet(abilityData.EffectScriptId, out _);
        }
    }
}
