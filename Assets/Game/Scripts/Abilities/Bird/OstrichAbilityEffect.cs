using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class OstrichAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null ||
                context.RanchManager == null ||
                context.RanchManager.Map == null ||
                abilityData?.EffectParams == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "MoveRandomAdjacentEmptyAndAddBaseMoneyNearFamily", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var targetFamily = abilityData.EffectParams.targetFamily;
            if (string.IsNullOrWhiteSpace(targetFamily) || string.Equals(targetFamily, "None", StringComparison.OrdinalIgnoreCase))
            {
                targetFamily = "Carnivora";
            }

            var neighbors = context.RanchManager.Map.GetNeighbors(context.Owner.Coords).ToList();
            var hasAdjacentTargetFamily = neighbors.Any(cell =>
                cell?.Animal?.Data != null && cell.Animal.Data.HasFamily(targetFamily));
            if (!hasAdjacentTargetFamily)
            {
                return false;
            }

            var bonus = abilityData.EffectParams.money > 0 ? abilityData.EffectParams.money : 1;
            context.RanchManager.AddAnimalBaseMoneyBonus(context.Owner, bonus);

            var emptyNeighbors = neighbors
                .Where(cell => cell != null && cell.IsEmpty)
                .Select(cell => cell.Coords)
                .ToList();
            if (emptyNeighbors.Count <= 0)
            {
                return true;
            }

            var destination = emptyNeighbors[UnityEngine.Random.Range(0, emptyNeighbors.Count)];
            context.RanchManager.TryMoveAnimal(context.Owner, destination);
            return true;
        }
    }
}
