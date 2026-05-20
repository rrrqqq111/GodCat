using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public readonly struct AnimalCooldownReductionContext
    {
        public AnimalCooldownReductionContext(
            Animal target,
            int amount,
            int previousCooldown,
            int newCooldown,
            Animal sourceAnimal = null,
            string sourceAbilityId = null,
            string reason = null)
        {
            Target = target;
            SourceAnimal = sourceAnimal;
            Amount = amount;
            PreviousCooldown = previousCooldown;
            NewCooldown = newCooldown;
            SourceAbilityId = sourceAbilityId;
            Reason = reason;
        }

        public Animal Target { get; }
        public Animal SourceAnimal { get; }
        public int Amount { get; }
        public int PreviousCooldown { get; }
        public int NewCooldown { get; }
        public string SourceAbilityId { get; }
        public string Reason { get; }
    }
}
