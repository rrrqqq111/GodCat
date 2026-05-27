using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class BreedAbilityEffect : IAbilityEffect, IBreedAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null ||
                context.RanchManager == null ||
                context.RanchManager.Map == null ||
                abilityData == null)
            {
                return false;
            }

            if (!string.Equals(abilityData.EffectType, "Breed", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(abilityData.EffectType, "Reproduce", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var offspringData = ResolveOffspringAnimalData(context, abilityData);
            if (offspringData == null)
            {
                return false;
            }

            if (!TryResolveSpawnCoords(context, out var spawnCoords))
            {
                return false;
            }

            return context.RanchManager.TryAddAnimalAtEmptyCell(spawnCoords, offspringData, out _);
        }

        public virtual AnimalData ResolveOffspringAnimalData(AnimalAbilityContext context, AbilityData abilityData)
        {
            return context.Owner != null ? context.Owner.Data : null;
        }

        public virtual bool TryResolveSpawnCoords(AnimalAbilityContext context, out Vector2Int spawnCoords)
        {
            spawnCoords = default;
            if (context.RanchManager == null || context.Owner == null || context.RanchManager.Map == null)
            {
                return false;
            }

            var map = context.RanchManager.Map;
            var emptyNeighborCoords = map.GetNeighbors(context.Owner.Coords)
                .Where(cell => cell != null && cell.IsEmpty)
                .Select(cell => cell.Coords)
                .ToList();
            if (emptyNeighborCoords.Count > 0)
            {
                spawnCoords = emptyNeighborCoords[UnityEngine.Random.Range(0, emptyNeighborCoords.Count)];
                return true;
            }

            var emptyCoords = map.GetCells()
                .Where(cell => cell != null && cell.IsEmpty)
                .Select(cell => cell.Coords)
                .ToList();
            if (emptyCoords.Count == 0)
            {
                return false;
            }

            spawnCoords = emptyCoords[UnityEngine.Random.Range(0, emptyCoords.Count)];
            return true;
        }
    }
}
