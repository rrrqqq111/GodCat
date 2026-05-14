using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public static class AbilityTargetResolver
    {
        public static List<Animal> Resolve(AnimalAbilityContext context, AbilityData abilityData)
        {
            var results = new List<Animal>();
            if (context.Owner == null || context.RanchManager == null || context.RanchManager.Map == null || abilityData == null)
            {
                return results;
            }

            foreach (var cell in ResolveCells(context, abilityData.ImpactType))
            {
                var animal = cell.Animal;
                if (animal == null)
                {
                    continue;
                }

                if (!MatchesTarget(context.Owner, animal, abilityData.EffectParams))
                {
                    continue;
                }

                results.Add(animal);
            }

            return results;
        }

        private static IEnumerable<MapCell> ResolveCells(AnimalAbilityContext context, string impactType)
        {
            var map = context.RanchManager.Map;
            var origin = context.Owner.Coords;

            switch (Normalize(impactType))
            {
                case "self":
                    if (map.TryGetCell(origin, out var selfCell))
                    {
                        yield return selfCell;
                    }
                    break;
                case "adjacent":
                    foreach (var cell in map.GetNeighbors(origin))
                    {
                        yield return cell;
                    }
                    break;
                case "left":
                    if (map.TryGetCell(origin + Vector2Int.left, out var leftCell))
                    {
                        yield return leftCell;
                    }
                    break;
                case "right":
                    if (map.TryGetCell(origin + Vector2Int.right, out var rightCell))
                    {
                        yield return rightCell;
                    }
                    break;
                case "up":
                case "upper":
                case "upperadjacent":
                    foreach (var upCell in map.GetUpperNeighbors(origin))
                    {
                        yield return upCell;
                    }
                    break;
                case "down":
                case "lower":
                case "loweradjacent":
                    foreach (var downCell in map.GetLowerNeighbors(origin))
                    {
                        yield return downCell;
                    }
                    break;
                case "all":
                case "global":
                case "field":
                    foreach (var cell in map.GetCells())
                    {
                        yield return cell;
                    }
                    break;
            }
        }

        private static bool MatchesTarget(Animal owner, Animal candidate, AbilityEffectParams effectParams)
        {
            var target = Normalize(effectParams != null ? effectParams.target : "Self");
            if (target == "self" && candidate != owner)
            {
                return false;
            }

            if (target == "other" && candidate == owner)
            {
                return false;
            }

            var targetFamily = effectParams != null ? effectParams.targetFamily : "None";
            if (!string.IsNullOrWhiteSpace(targetFamily) && !string.Equals(targetFamily, "None", StringComparison.OrdinalIgnoreCase))
            {
                return candidate.Data != null && string.Equals(candidate.Data.Family, targetFamily, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }
    }
}
