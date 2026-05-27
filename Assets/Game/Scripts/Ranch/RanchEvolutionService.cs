using System;
using NekogamiRanch.Abilities;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public class RanchEvolutionService
    {
        private readonly RanchEventHub eventHub;
        private readonly Func<Animal, AnimalData, bool> evolveAnimal;

        public RanchEvolutionService(RanchEventHub eventHub, Func<Animal, AnimalData, bool> evolveAnimal)
        {
            this.eventHub = eventHub;
            this.evolveAnimal = evolveAnimal;
        }

        public void RegisterSuccessfulAbilityTrigger(Animal animal, AbilityData sourceAbility)
        {
            if (animal == null || !animal.HasEvolution)
            {
                return;
            }

            var previousProgress = animal.EvolutionProgress;
            var previousLevel = animal.EvolutionLevel;
            animal.AddEvolutionProgress();
            var context = new AnimalEvolutionContext(
                animal,
                1,
                previousProgress,
                animal.EvolutionProgress,
                previousLevel,
                animal.EvolutionLevel,
                sourceAbility != null ? sourceAbility.Id : null);

            eventHub?.NotifyAnimalEvolutionProgressed(context);
            if (context.LevelsGained > 0)
            {
                eventHub?.NotifyAnimalEvolutionLeveledUp(context);
            }

            if (animal.Data != null &&
                animal.Data.EvolutionTarget != null &&
                animal.Data.EvolutionTargetLevel > 0 &&
                animal.EvolutionLevel >= animal.Data.EvolutionTargetLevel)
            {
                evolveAnimal?.Invoke(animal, animal.Data.EvolutionTarget);
            }
        }
    }
}
