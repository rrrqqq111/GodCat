using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities.Prey
{
    public class ProtectionRule
    {
        public ProtectionRule(
            string scope,
            IEnumerable<string> protectedAnimalIds = null,
            IEnumerable<string> protectedFamilies = null,
            IEnumerable<string> againstPredatorIds = null,
            IEnumerable<string> againstPredatorFamilies = null,
            Animal protector = null,
            IEnumerable<string> protectorAnimalIds = null,
            IEnumerable<string> protectorFamilies = null,
            string reason = null)
        {
            Scope = string.IsNullOrWhiteSpace(scope) ? "Adjacent" : scope;
            ProtectedAnimalIds = RuleText.CopyNonEmpty(protectedAnimalIds);
            ProtectedFamilies = RuleText.CopyNonEmpty(protectedFamilies);
            AgainstPredatorIds = RuleText.CopyNonEmpty(againstPredatorIds);
            AgainstPredatorFamilies = RuleText.CopyNonEmpty(againstPredatorFamilies);
            Protector = protector;
            ProtectorAnimalIds = RuleText.CopyNonEmpty(protectorAnimalIds);
            ProtectorFamilies = RuleText.CopyNonEmpty(protectorFamilies);
            Reason = reason;
        }

        public string Scope { get; }
        public IReadOnlyList<string> ProtectedAnimalIds { get; }
        public IReadOnlyList<string> ProtectedFamilies { get; }
        public IReadOnlyList<string> AgainstPredatorIds { get; }
        public IReadOnlyList<string> AgainstPredatorFamilies { get; }
        public Animal Protector { get; }
        public IReadOnlyList<string> ProtectorAnimalIds { get; }
        public IReadOnlyList<string> ProtectorFamilies { get; }
        public string Reason { get; }

        public bool MatchesProtector(Animal protector)
        {
            if (protector?.Data == null)
            {
                return false;
            }

            if (Protector != null && Protector != protector)
            {
                return false;
            }

            var hasProtectorIdRule = ProtectorAnimalIds.Count > 0;
            var hasProtectorFamilyRule = ProtectorFamilies.Count > 0;
            if (!hasProtectorIdRule && !hasProtectorFamilyRule)
            {
                return true;
            }

            return RuleText.Contains(ProtectorAnimalIds, protector.Data.Id) ||
                RuleText.Contains(ProtectorFamilies, protector.Data.Family);
        }

        public bool CanProtect(Animal protector, Animal predator, Animal target)
        {
            return MatchesProtector(protector) &&
                MatchesProtectedTarget(target) &&
                MatchesPredator(predator);
        }

        private bool MatchesProtectedTarget(Animal target)
        {
            if (target?.Data == null)
            {
                return false;
            }

            var hasProtectedIdRule = ProtectedAnimalIds.Count > 0;
            var hasProtectedFamilyRule = ProtectedFamilies.Count > 0;
            if (!hasProtectedIdRule && !hasProtectedFamilyRule)
            {
                return true;
            }

            return RuleText.Contains(ProtectedAnimalIds, target.Data.Id) ||
                RuleText.Contains(ProtectedFamilies, target.Data.Family);
        }

        private bool MatchesPredator(Animal predator)
        {
            if (predator?.Data == null)
            {
                return false;
            }

            var hasPredatorIdRule = AgainstPredatorIds.Count > 0;
            var hasPredatorFamilyRule = AgainstPredatorFamilies.Count > 0;
            if (!hasPredatorIdRule && !hasPredatorFamilyRule)
            {
                return true;
            }

            return RuleText.Contains(AgainstPredatorIds, predator.Data.Id) ||
                RuleText.Contains(AgainstPredatorFamilies, predator.Data.Family);
        }
    }
}
