using System;
using System.Collections.Generic;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public class RanchPreyService
    {
        private readonly RanchMap ranchMap;
        private readonly RanchAnimalLifecycleService lifecycleService;
        private readonly RanchProtectionService protectionService;
        private readonly RanchEventHub eventHub;

        public RanchPreyService(
            RanchMap ranchMap,
            RanchAnimalLifecycleService lifecycleService,
            RanchProtectionService protectionService,
            RanchEventHub eventHub)
        {
            this.ranchMap = ranchMap;
            this.lifecycleService = lifecycleService;
            this.protectionService = protectionService;
            this.eventHub = eventHub;
        }

        public PreyResult TryPrey(PreyContext context)
        {
            if (context?.Predator == null || context.TargetRule == null || ranchMap == null)
            {
                return Complete(PreyResult.Failed(
                    context != null ? context.Predator : null,
                    Array.Empty<Animal>(),
                    Array.Empty<ProtectionResult>(),
                    "InvalidPreyContext"));
            }

            eventHub?.NotifyPreyAttempted(context);
            var candidateTargets = PreyTargetResolver.Resolve(context, ranchMap);
            var protectionResults = new List<ProtectionResult>();
            if (candidateTargets.Count == 0)
            {
                return Complete(PreyResult.Failed(
                    context.Predator,
                    candidateTargets,
                    protectionResults,
                    "NoCandidateTargets"));
            }

            var removedTargets = new List<Animal>();
            foreach (var target in candidateTargets)
            {
                var protectionResult = protectionService != null
                    ? protectionService.Resolve(context, target)
                    : ProtectionResult.Unprotected(target);
                if (protectionResult.Success)
                {
                    protectionResults.Add(protectionResult);
                    eventHub?.NotifyPreyProtected(protectionResult);
                    continue;
                }

                if (lifecycleService != null &&
                    lifecycleService.TryRemove(target, AnimalRemovalReason.Preyed, context.Predator))
                {
                    removedTargets.Add(target);
                }
            }

            if (removedTargets.Count > 0)
            {
                return Complete(PreyResult.Succeeded(
                    context.Predator,
                    candidateTargets,
                    protectionResults,
                    removedTargets));
            }

            var failureReason = protectionResults.Count == candidateTargets.Count ? "AllTargetsProtected" : "NoTargetRemoved";
            return Complete(PreyResult.Failed(
                context.Predator,
                candidateTargets,
                protectionResults,
                failureReason));
        }

        private PreyResult Complete(PreyResult result)
        {
            eventHub?.NotifyPreyCompleted(result);
            return result;
        }
    }
}
