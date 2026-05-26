using System;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public class RanchAnimalLifecycleService
    {
        private readonly RanchAnimalService animalService;
        private readonly RanchSettlementService settlementService;
        private readonly RanchMap ranchMap;
        private readonly Action<Animal> animalRemoved;
        private readonly Action<Animal> animalSold;
        private readonly Action<Animal, Animal> animalPreyed;

        public RanchAnimalLifecycleService(
            RanchAnimalService animalService,
            RanchSettlementService settlementService,
            RanchMap ranchMap,
            Action<Animal> animalRemoved,
            Action<Animal> animalSold,
            Action<Animal, Animal> animalPreyed)
        {
            this.animalService = animalService;
            this.settlementService = settlementService;
            this.ranchMap = ranchMap;
            this.animalRemoved = animalRemoved;
            this.animalSold = animalSold;
            this.animalPreyed = animalPreyed;
        }

        public bool TryRemove(Animal animal, AnimalRemovalReason reason = AnimalRemovalReason.Manual, Animal source = null)
        {
            if (animal == null || animalService == null || !animalService.IsAnimalOnMap(animal))
            {
                return false;
            }

            if (reason == AnimalRemovalReason.Preyed && source == null)
            {
                return false;
            }

            var removedCoords = animal.Coords;
            if (reason == AnimalRemovalReason.Sold)
            {
                animalSold?.Invoke(animal);
            }
            else if (reason == AnimalRemovalReason.Preyed)
            {
                animalPreyed?.Invoke(source, animal);
                settlementService?.ResolveAnimalPreyedAbilities(source, animal, ranchMap);
            }

            if (!animalService.AnimalRemoved(animal))
            {
                return false;
            }

            settlementService?.ResolveAdjacentAnimalRemovedAbilities(animal, removedCoords, ranchMap);
            settlementService?.ResolveAnimalRemovedAbility(animal, removedCoords);
            animalRemoved?.Invoke(animal);
            return true;
        }
    }
}
