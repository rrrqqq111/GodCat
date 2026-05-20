using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.Abilities.Prey
{
    public static class PreyTargetResolver
    {
        public static IReadOnlyList<Animal> Resolve(PreyContext context, RanchMap map)
        {
            var results = new List<Animal>();
            if (context?.Predator == null || context.TargetRule == null || map == null)
            {
                return results;
            }

            var includePredator = RuleText.Normalize(context.TargetRule.ImpactType) == "self";
            foreach (var cell in ResolveCells(map, context.Predator.Coords, context.TargetRule.ImpactType))
            {
                var animal = cell?.Animal;
                if (animal == null)
                {
                    continue;
                }

                if (!includePredator && animal == context.Predator)
                {
                    continue;
                }

                if (context.TargetRule.Matches(animal))
                {
                    results.Add(animal);
                }
            }

            if (context.TargetRule.RandomPick)
            {
                results = results.OrderBy(_ => Random.value).ToList();
            }

            var targetCount = Mathf.Max(1, context.ResolveTargetCount());
            return results.Take(targetCount).ToList();
        }

        private static IEnumerable<MapCell> ResolveCells(RanchMap map, Vector2Int origin, string impactType)
        {
            switch (RuleText.Normalize(impactType))
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
                    foreach (var cell in map.GetUpperNeighbors(origin))
                    {
                        yield return cell;
                    }
                    break;
                case "down":
                case "lower":
                case "loweradjacent":
                    foreach (var cell in map.GetLowerNeighbors(origin))
                    {
                        yield return cell;
                    }
                    break;
                case "row":
                    foreach (var cell in map.GetCellsInScanOrder())
                    {
                        if (cell.Coords.y == origin.y)
                        {
                            yield return cell;
                        }
                    }
                    break;
                case "all":
                case "global":
                case "field":
                    foreach (var cell in map.GetCellsInScanOrder())
                    {
                        yield return cell;
                    }
                    break;
            }
        }
    }
}
