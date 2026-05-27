using System;
using NekogamiRanch.Abilities;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;
using NekogamiRanch.MapObjects;

namespace NekogamiRanch.Ranch
{
    public class RanchEventHub
    {
        public event Action StateChanged;
        public event Action<PreyContext> PreyAttempted;
        public event Action<ProtectionResult> PreyProtected;
        public event Action<PreyResult> PreySucceeded;
        public event Action<PreyResult> PreyFailed;
        public event Action<Animal, Animal> AnimalPreyed;
        public event Action<Animal> AnimalRemoved;
        public event Action<Animal> AnimalSold;
        public event Action<Animal, AnimalData> AnimalGrown;
        public event Action<Animal, AnimalData> AnimalTransformed;
        public event Action<AnimalCooldownReductionContext> AnimalCooldownReduced;
        public event Action<AnimalEvolutionContext> AnimalEvolutionProgressed;
        public event Action<AnimalEvolutionContext> AnimalEvolutionLeveledUp;
        public event Action<MapCellObjectRuntime> MapObjectAdded;
        public event Action<MapCellObjectRuntime> MapObjectRemoved;
        public event Action<MapCellObjectUseContext, MapCellObjectUseResult> MapObjectConsumed;

        public void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }

        public void NotifyPreyAttempted(PreyContext context)
        {
            PreyAttempted?.Invoke(context);
        }

        public void NotifyPreyProtected(ProtectionResult result)
        {
            PreyProtected?.Invoke(result);
        }

        public void NotifyPreyCompleted(PreyResult result)
        {
            if (result != null && result.Success)
            {
                PreySucceeded?.Invoke(result);
            }
            else
            {
                PreyFailed?.Invoke(result);
            }
        }

        public void NotifyAnimalPreyed(Animal predator, Animal animal)
        {
            AnimalPreyed?.Invoke(predator, animal);
        }

        public void NotifyAnimalRemoved(Animal animal)
        {
            AnimalRemoved?.Invoke(animal);
        }

        public void NotifyAnimalSold(Animal animal)
        {
            AnimalSold?.Invoke(animal);
        }

        public void NotifyAnimalGrown(Animal animal, AnimalData result)
        {
            AnimalGrown?.Invoke(animal, result);
        }

        public void NotifyAnimalTransformed(Animal animal, AnimalData result)
        {
            AnimalTransformed?.Invoke(animal, result);
        }

        public void NotifyAnimalCooldownReduced(AnimalCooldownReductionContext context)
        {
            AnimalCooldownReduced?.Invoke(context);
        }

        public void NotifyAnimalEvolutionProgressed(AnimalEvolutionContext context)
        {
            AnimalEvolutionProgressed?.Invoke(context);
        }

        public void NotifyAnimalEvolutionLeveledUp(AnimalEvolutionContext context)
        {
            AnimalEvolutionLeveledUp?.Invoke(context);
        }

        public void NotifyMapObjectAdded(MapCellObjectRuntime mapObject)
        {
            MapObjectAdded?.Invoke(mapObject);
        }

        public void NotifyMapObjectRemoved(MapCellObjectRuntime mapObject)
        {
            MapObjectRemoved?.Invoke(mapObject);
        }

        public void NotifyMapObjectConsumed(MapCellObjectUseContext context, MapCellObjectUseResult result)
        {
            MapObjectConsumed?.Invoke(context, result);
        }
    }
}
