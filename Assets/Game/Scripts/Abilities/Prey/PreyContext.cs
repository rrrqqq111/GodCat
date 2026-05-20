using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities.Prey
{
    public class PreyContext
    {
        public PreyContext(
            Animal predator,
            PreyTargetRule targetRule,
            int targetCount = 0,
            string sourceAbilityId = null,
            IReadOnlyList<ProtectionRule> protectionRules = null)
        {
            Predator = predator;
            TargetRule = targetRule;
            TargetCount = targetCount;
            SourceAbilityId = sourceAbilityId;
            ProtectionRules = protectionRules != null
                ? protectionRules.Where(rule => rule != null).ToList()
                : new List<ProtectionRule>();
        }

        public Animal Predator { get; }
        public PreyTargetRule TargetRule { get; }
        public int TargetCount { get; }
        public string SourceAbilityId { get; }
        public IReadOnlyList<ProtectionRule> ProtectionRules { get; }

        public int ResolveTargetCount()
        {
            if (TargetCount > 0)
            {
                return TargetCount;
            }

            return TargetRule != null ? TargetRule.TargetCount : 1;
        }
    }
}
