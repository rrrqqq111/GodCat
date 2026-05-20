using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities.Prey
{
    public class PreyResult
    {
        public PreyResult(
            bool success,
            Animal predator,
            IReadOnlyList<Animal> candidateTargets,
            IReadOnlyList<ProtectionResult> protectionResults,
            IReadOnlyList<Animal> removedTargets,
            string failureReason = null)
        {
            Success = success;
            Predator = predator;
            CandidateTargets = CopyAnimals(candidateTargets);
            ProtectionResults = protectionResults != null ? protectionResults.ToList() : new List<ProtectionResult>();
            ProtectedTargets = ProtectionResults
                .Where(result => result.Success && result.Target != null)
                .Select(result => result.Target)
                .ToList();
            RemovedTargets = CopyAnimals(removedTargets);
            FailureReason = failureReason;
        }

        public bool Success { get; }
        public Animal Predator { get; }
        public IReadOnlyList<Animal> CandidateTargets { get; }
        public IReadOnlyList<Animal> ProtectedTargets { get; }
        public IReadOnlyList<ProtectionResult> ProtectionResults { get; }
        public IReadOnlyList<Animal> RemovedTargets { get; }
        public string FailureReason { get; }

        public static PreyResult Failed(
            Animal predator,
            IReadOnlyList<Animal> candidateTargets,
            IReadOnlyList<ProtectionResult> protectionResults,
            string failureReason)
        {
            return new PreyResult(false, predator, candidateTargets, protectionResults, null, failureReason);
        }

        public static PreyResult Succeeded(
            Animal predator,
            IReadOnlyList<Animal> candidateTargets,
            IReadOnlyList<ProtectionResult> protectionResults,
            IReadOnlyList<Animal> removedTargets)
        {
            return new PreyResult(true, predator, candidateTargets, protectionResults, removedTargets);
        }

        private static IReadOnlyList<Animal> CopyAnimals(IReadOnlyList<Animal> animals)
        {
            return animals != null
                ? animals.Where(animal => animal != null).ToList()
                : new List<Animal>();
        }
    }
}
