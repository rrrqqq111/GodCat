using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities.Prey
{
    public readonly struct ProtectionResult
    {
        private ProtectionResult(bool success, Animal protector, Animal target, string reason, ProtectionRule rule)
        {
            Success = success;
            Protector = protector;
            Target = target;
            Reason = reason;
            Rule = rule;
        }

        public bool Success { get; }
        public Animal Protector { get; }
        public Animal Target { get; }
        public string Reason { get; }
        public ProtectionRule Rule { get; }

        public static ProtectionResult Protected(Animal protector, Animal target, ProtectionRule rule, string reason = null)
        {
            return new ProtectionResult(true, protector, target, reason, rule);
        }

        public static ProtectionResult Unprotected(Animal target, string reason = null)
        {
            return new ProtectionResult(false, null, target, reason, null);
        }
    }
}
