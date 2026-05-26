using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public class RanchAnimalLifecycleService
    {
        private readonly RanchAnimalService animalService;
        private readonly RanchSettlementService settlementService;
        private readonly RanchMap ranchMap;
        private readonly RanchEventHub eventHub;

        public RanchAnimalLifecycleService(
            RanchAnimalService animalService,
            RanchSettlementService settlementService,
            RanchMap ranchMap,
            RanchEventHub eventHub)
        {
            this.animalService = animalService;
            this.settlementService = settlementService;
            this.ranchMap = ranchMap;
            this.eventHub = eventHub;
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
                eventHub?.NotifyAnimalSold(animal);
            }
            else if (reason == AnimalRemovalReason.Preyed)
            {
                eventHub?.NotifyAnimalPreyed(source, animal);
                settlementService?.ResolveAnimalPreyedAbilities(source, animal, ranchMap);
                settlementService?.ResolveGlobalAnimalPreyedAbilities(source, animal, ranchMap);
            }

            if (!animalService.AnimalRemoved(animal))
            {
                return false;
            }

            if (reason != AnimalRemovalReason.Manual)
            {
                return true;
            }

            settlementService?.ResolveAdjacentAnimalRemovedAbilities(animal, removedCoords, ranchMap);
            settlementService?.ResolveAnimalRemovedAbility(animal, removedCoords);
            eventHub?.NotifyAnimalRemoved(animal);
            return true;
        }
    }
}
