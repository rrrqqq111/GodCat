using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class HenAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null ||
                context.RanchManager == null ||
                context.RanchManager.Map == null ||
                abilityData?.EffectParams?.animalData == null)
            {
#if UNITY_EDITOR
                Debug.Log($"[HenAbilityEffect] invalid context or chick data. owner={context.Owner?.DisplayName} hasManager={context.RanchManager != null} hasMap={context.RanchManager?.Map != null} hasChickData={abilityData?.EffectParams?.animalData != null}");
#endif
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "Breed", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(abilityData.EffectType, "Reproduce", StringComparison.OrdinalIgnoreCase))
            {
#if UNITY_EDITOR
                Debug.Log($"[HenAbilityEffect] unsupported effectType={abilityData.EffectType}");
#endif
                return false;
            }

            if (!HasAdjacentRooster(context, targets))
            {
#if UNITY_EDITOR
                Debug.Log($"[HenAbilityEffect] no adjacent rooster. owner={context.Owner.DisplayName} targets={targets?.Count ?? 0}");
#endif
                return false;
            }

            var added = TrySpawnChicken(context, abilityData.EffectParams.animalData, out var spawnCoords);
#if UNITY_EDITOR
            Debug.Log($"[HenAbilityEffect] try spawn chick. owner={context.Owner.DisplayName} coords=({spawnCoords.x},{spawnCoords.y}) chick={abilityData.EffectParams.animalData.Id} added={added}");
#endif
            return added;
        }

        private static bool HasAdjacentRooster(AnimalAbilityContext context, IReadOnlyList<Animal> targets)
        {
            if (targets != null && targets.Any(IsRooster))
            {
                return true;
            }

            return context.RanchManager.Map.GetNeighbors(context.Owner.Coords)
                .Any(cell => string.Equals(cell?.Animal?.Data?.Id, "Rooster", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsRooster(Animal animal)
        {
            return string.Equals(animal?.Data?.Id, "Rooster", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TrySpawnChicken(AnimalAbilityContext context, AnimalData chickenData, out Vector2Int spawnCoords)
        {
            spawnCoords = default;
            var map = context.RanchManager.Map;

            var candidateCoords = map.GetNeighbors(context.Owner.Coords)
                .Where(cell => cell != null && cell.IsEmpty)
                .Select(cell => cell.Coords)
                .ToList();

            if (candidateCoords.Count == 0)
            {
                candidateCoords = map.GetCells()
                    .Where(cell => cell != null && cell.IsEmpty)
                    .Select(cell => cell.Coords)
                    .ToList();
            }

            foreach (var coords in candidateCoords.OrderBy(_ => UnityEngine.Random.value))
            {
                if (context.RanchManager.TryAddAnimalAtEmptyCell(coords, chickenData, out _))
                {
                    spawnCoords = coords;
                    return true;
                }
            }

            return false;
        }
    }
}
