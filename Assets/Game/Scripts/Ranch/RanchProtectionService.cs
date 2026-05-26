using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Abilities;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchProtectionService
    {
        private readonly RanchMap ranchMap;
        private readonly Action<int> addMoney;

        public RanchProtectionService(RanchMap ranchMap, Action<int> addMoney)
        {
            this.ranchMap = ranchMap;
            this.addMoney = addMoney;
        }

        public ProtectionResult Resolve(PreyContext context, Animal target)
        {
            if (context == null || target == null)
            {
                return ProtectionResult.Unprotected(target);
            }

            foreach (var rule in ResolveRules(context))
            {
                foreach (var protector in ResolveCandidateProtectors(rule, target))
                {
                    if (!rule.CanProtect(protector, context.Predator, target))
                    {
                        continue;
                    }

                    var reason = string.IsNullOrWhiteSpace(rule.Reason) ? "Protected" : rule.Reason;
                    var result = ProtectionResult.Protected(protector, target, rule, reason);
                    ApplyPassiveProtectionReward(result);
                    return result;
                }
            }

            return ProtectionResult.Unprotected(target);
        }

        private IEnumerable<ProtectionRule> ResolveRules(PreyContext context)
        {
            if (context?.ProtectionRules != null)
            {
                foreach (var rule in context.ProtectionRules)
                {
                    if (rule != null)
                    {
                        yield return rule;
                    }
                }
            }

            if (ranchMap == null)
            {
                yield break;
            }

            foreach (var cell in ranchMap.GetCellsInScanOrder())
            {
                var protector = cell.Animal;
                var abilityData = protector?.Data?.Ability;
                if (abilityData == null ||
                    !AbilityEffectRegistry.TryGet(abilityData.EffectScriptId, out var effect) ||
                    !(effect is IPassiveProtectionEffect passiveProtection))
                {
                    continue;
                }

                var rule = passiveProtection.CreateProtectionRule(protector, abilityData);
                if (rule != null)
                {
                    yield return rule;
                }
            }
        }

        private void ApplyPassiveProtectionReward(ProtectionResult result)
        {
            var abilityData = result.Protector?.Data?.Ability;
            if (!result.Success ||
                abilityData == null ||
                !AbilityEffectRegistry.TryGet(abilityData.EffectScriptId, out var effect) ||
                !(effect is IPassiveProtectionEffect passiveProtection))
            {
                return;
            }

            passiveProtection.OnProtected(result.Protector, result.Target, abilityData, addMoney);
        }

        private IReadOnlyList<Animal> ResolveCandidateProtectors(ProtectionRule rule, Animal target)
        {
            var protectors = new List<Animal>();
            if (rule == null || target == null || ranchMap == null)
            {
                return protectors;
            }

            if (rule.Protector != null)
            {
                if (IsAnimalOnMap(rule.Protector) && ScopeContainsTarget(rule.Scope, rule.Protector, target))
                {
                    protectors.Add(rule.Protector);
                }

                return protectors;
            }

            foreach (var cell in ranchMap.GetCellsInScanOrder())
            {
                var protector = cell.Animal;
                if (protector != null && rule.MatchesProtector(protector) && ScopeContainsTarget(rule.Scope, protector, target))
                {
                    protectors.Add(protector);
                }
            }

            return protectors;
        }

        private bool ScopeContainsTarget(string scope, Animal protector, Animal target)
        {
            if (protector == null || target == null || ranchMap == null)
            {
                return false;
            }

            switch (Normalize(scope))
            {
                case "self":
                    return protector == target;
                case "adjacent":
                    return ranchMap.GetNeighbors(protector.Coords).Any(cell => cell.Animal == target);
                case "left":
                    return target.Coords == protector.Coords + Vector2Int.left;
                case "right":
                    return target.Coords == protector.Coords + Vector2Int.right;
                case "up":
                case "upper":
                case "upperadjacent":
                    return ranchMap.GetUpperNeighbors(protector.Coords).Any(cell => cell.Animal == target);
                case "down":
                case "lower":
                case "loweradjacent":
                    return ranchMap.GetLowerNeighbors(protector.Coords).Any(cell => cell.Animal == target);
                case "row":
                    return protector.Coords.y == target.Coords.y;
                case "all":
                case "global":
                case "field":
                    return true;
                default:
                    return false;
            }
        }

        private bool IsAnimalOnMap(Animal animal)
        {
            return animal != null &&
                ranchMap != null &&
                ranchMap.TryGetCell(animal.Coords, out var cell) &&
                cell.Animal == animal;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }
    }
}
