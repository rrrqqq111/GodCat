using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities.Prey
{
    public class ProtectionContext
    {
        public ProtectionContext(
            Animal predator,
            Animal target,
            IReadOnlyList<Animal> candidateProtectors,
            string sourceAbilityId = null)
        {
            Predator = predator;
            Target = target;
            CandidateProtectors = candidateProtectors != null
                ? candidateProtectors.Where(animal => animal != null).ToList()
                : new List<Animal>();
            SourceAbilityId = sourceAbilityId;
        }

        public Animal Predator { get; }
        public Animal Target { get; }
        public IReadOnlyList<Animal> CandidateProtectors { get; }
        public string SourceAbilityId { get; }
    }
}
