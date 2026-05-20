using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities.Prey
{
    public class PreyTargetRule
    {
        public PreyTargetRule(
            string impactType,
            IEnumerable<string> targetAnimalIds = null,
            IEnumerable<string> targetFamilies = null,
            IEnumerable<string> excludeAnimalIds = null,
            IEnumerable<string> excludeFamilies = null,
            int targetCount = 1,
            bool randomPick = false)
        {
            ImpactType = string.IsNullOrWhiteSpace(impactType) ? "Adjacent" : impactType;
            TargetAnimalIds = RuleText.CopyNonEmpty(targetAnimalIds);
            TargetFamilies = RuleText.CopyNonEmpty(targetFamilies);
            ExcludeAnimalIds = RuleText.CopyNonEmpty(excludeAnimalIds);
            ExcludeFamilies = RuleText.CopyNonEmpty(excludeFamilies);
            TargetCount = Math.Max(1, targetCount);
            RandomPick = randomPick;
        }

        public string ImpactType { get; }
        public IReadOnlyList<string> TargetAnimalIds { get; }
        public IReadOnlyList<string> TargetFamilies { get; }
        public IReadOnlyList<string> ExcludeAnimalIds { get; }
        public IReadOnlyList<string> ExcludeFamilies { get; }
        public int TargetCount { get; }
        public bool RandomPick { get; }

        public bool Matches(Animal animal)
        {
            if (animal?.Data == null)
            {
                return false;
            }

            if (RuleText.Contains(ExcludeAnimalIds, animal.Data.Id) ||
                RuleText.Contains(ExcludeFamilies, animal.Data.Family))
            {
                return false;
            }

            var hasTargetIdRule = TargetAnimalIds.Count > 0;
            var hasTargetFamilyRule = TargetFamilies.Count > 0;
            if (!hasTargetIdRule && !hasTargetFamilyRule)
            {
                return true;
            }

            return RuleText.Contains(TargetAnimalIds, animal.Data.Id) ||
                RuleText.Contains(TargetFamilies, animal.Data.Family);
        }
    }

    internal static class RuleText
    {
        public static IReadOnlyList<string> CopyNonEmpty(IEnumerable<string> values)
        {
            return values == null
                ? Array.Empty<string>()
                : values.Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim())
                    .ToList();
        }

        public static bool Contains(IReadOnlyList<string> values, string target)
        {
            if (values == null || values.Count == 0 || string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], target, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }
    }
}
