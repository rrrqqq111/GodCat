using System.Collections.Generic;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class HorseAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RanchManager == null || context.RanchManager.Map == null || abilityData == null)
            {
                return false;
            }

            var map = context.RanchManager.Map;
            var startCoords = context.Owner.Coords;
            var destination = startCoords;

            for (var x = startCoords.x + 1; x < map.Width; x++)
            {
                if (!map.TryGetCell(new Vector2Int(x, startCoords.y), out var cell) || !cell.IsEmpty)
                {
                    break;
                }

                destination = cell.Coords;
            }

            var movedSteps = destination.x - startCoords.x;
            if (movedSteps <= 0 || !context.RanchManager.TryMoveAnimal(context.Owner, destination))
            {
                return false;
            }

            var moneyPerStep = abilityData.EffectParams != null ? abilityData.EffectParams.money : 0;
            if (moneyPerStep > 0)
            {
                context.RanchManager.AddMoney(movedSteps * moneyPerStep);
            }

#if UNITY_EDITOR
            Debug.Log($"[HorseAbilityEffect] owner={context.Owner.DisplayName} from=({startCoords.x},{startCoords.y}) to=({destination.x},{destination.y}) steps={movedSteps} money={movedSteps * moneyPerStep}");
#endif

            return true;
        }
    }
}
