using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchAnimalCommandService
    {
        private readonly RanchMap ranchMap;
        private readonly RanchAnimalService animalService;
        private readonly RanchAnimalLifecycleService lifecycleService;
        private readonly RanchAnimalSpawnService spawnService;
        private readonly RanchSelectionService selectionService;
        private readonly RanchEventHub eventHub;
        private readonly System.Action stateChanged;

        public RanchAnimalCommandService(
            RanchMap ranchMap,
            RanchAnimalService animalService,
            RanchAnimalLifecycleService lifecycleService,
            RanchAnimalSpawnService spawnService,
            RanchSelectionService selectionService,
            RanchEventHub eventHub,
            System.Action stateChanged)
        {
            this.ranchMap = ranchMap;
            this.animalService = animalService;
            this.lifecycleService = lifecycleService;
            this.spawnService = spawnService;
            this.selectionService = selectionService;
            this.eventHub = eventHub;
            this.stateChanged = stateChanged;
        }

        public bool RemoveAnimal(Animal animal)
        {
            if (animalService == null || lifecycleService == null || animal == null)
            {
                return false;
            }

            if (!lifecycleService.TryRemove(animal))
            {
                return false;
            }

            NotifyAfterAnimalRemoval();
            return true;
        }

        public bool SellAnimal(Animal animal)
        {
            if (animalService == null || lifecycleService == null || animal == null)
            {
                return false;
            }

            if (!lifecycleService.TryRemove(animal, AnimalRemovalReason.Sold))
            {
                return false;
            }

            NotifyAfterAnimalRemoval();
            return true;
        }

        public bool TransformAnimal(Animal oldAnimal, AnimalData newAnimalData)
        {
            if (animalService == null || oldAnimal == null || newAnimalData == null)
            {
                return false;
            }

            var wasSelectedAnimal = selectionService != null && selectionService.IsSelectedAnimal(oldAnimal);
            eventHub?.NotifyAnimalTransformed(oldAnimal, newAnimalData);
            var replaced = animalService.ReplaceAnimal(oldAnimal, newAnimalData);
            if (replaced && wasSelectedAnimal)
            {
                selectionService.SelectCell(null);
            }
            else if (replaced)
            {
                stateChanged?.Invoke();
            }

            return replaced;
        }

        public bool EvolveAnimalSilently(Animal oldAnimal, AnimalData newAnimalData)
        {
            if (animalService == null || oldAnimal == null || newAnimalData == null)
            {
                return false;
            }

            var replaced = animalService.ReplaceAnimal(oldAnimal, newAnimalData, inheritEvolutionState: false);
            if (replaced)
            {
                stateChanged?.Invoke();
            }

            return replaced;
        }

        public bool GrowAnimal(Animal youngAnimal, AnimalData grownAnimalData)
        {
            if (animalService == null || youngAnimal == null || grownAnimalData == null)
            {
                return false;
            }

            eventHub?.NotifyAnimalGrown(youngAnimal, grownAnimalData);
            if (!animalService.ReplaceAnimal(youngAnimal, grownAnimalData))
            {
                return false;
            }

            if (selectionService != null && selectionService.IsSelectedAnimal(youngAnimal))
            {
                selectionService.SelectCell(null);
            }
            else
            {
                stateChanged?.Invoke();
            }

            return true;
        }

        public bool TrySetAnimalAt(Vector2Int coords, AnimalData animalData)
        {
            if (animalService == null || ranchMap == null || !ranchMap.TryGetCell(coords, out var cell))
            {
                return false;
            }

            if (!animalService.TrySetAnimalAt(coords, animalData))
            {
                return false;
            }

            selectionService?.SelectCell(cell);
            stateChanged?.Invoke();
            return true;
        }

        public bool TryAddAnimalToRandomEmptyCell(AnimalData animalData)
        {
            if (animalService == null || animalData == null)
            {
                return false;
            }

            var added = animalService.TryAddAnimalToRandomEmptyCell(animalData);
            if (added)
            {
                stateChanged?.Invoke();
            }

            return added;
        }

        public bool TryAddAnimalAtEmptyCell(Vector2Int coords, AnimalData animalData, out Animal addedAnimal)
        {
            addedAnimal = null;
            if (animalService == null ||
                ranchMap == null ||
                animalData == null ||
                !ranchMap.TryGetCell(coords, out var cell) ||
                !cell.IsEmpty)
            {
                return false;
            }

            var added = animalService.TryAddAnimalAtEmptyCell(animalData, coords, out addedAnimal);
            if (added)
            {
                stateChanged?.Invoke();
            }

            return added;
        }

        public bool TryAddRandomAnimalFromFamily(string family, int baseMoneyBonus, out Animal addedAnimal)
        {
            addedAnimal = null;
            if (spawnService == null || !spawnService.TrySpawnRandomFromFamily(family, baseMoneyBonus, out addedAnimal))
            {
                return false;
            }

            stateChanged?.Invoke();
            return true;
        }

        public bool TryAddRandomAnimalOutsideFamilyAt(string excludedFamily, Vector2Int coords, out Animal addedAnimal)
        {
            addedAnimal = null;
            if (spawnService == null || !spawnService.TrySpawnRandomOutsideFamilyAt(excludedFamily, coords, out addedAnimal))
            {
                return false;
            }

            stateChanged?.Invoke();
            return true;
        }

        public bool TryReplaceAnimalWithRandomRaritySilently(Animal target, int minRarity, int maxRarity)
        {
            if (spawnService == null || !spawnService.TryReplaceWithRandomRarity(target, minRarity, maxRarity))
            {
                return false;
            }

            stateChanged?.Invoke();
            return true;
        }

        public bool TryClearAnimalAt(Vector2Int coords)
        {
            if (animalService == null || lifecycleService == null || ranchMap == null || !ranchMap.TryGetCell(coords, out var cell))
            {
                return false;
            }

            var removedAnimal = cell.Animal;
            if (removedAnimal == null || !lifecycleService.TryRemove(removedAnimal))
            {
                return false;
            }

            selectionService?.SelectCell(cell);
            stateChanged?.Invoke();
            return true;
        }

        public bool DeleteAnimalAtForTest(Vector2Int coords)
        {
            if (animalService == null || ranchMap == null || !ranchMap.TryGetCell(coords, out var cell))
            {
                return false;
            }

            var removedAnimal = cell.Animal;
            if (removedAnimal == null || !animalService.AnimalRemovedFromCell(cell))
            {
                return false;
            }

            selectionService?.SelectCell(cell);
            stateChanged?.Invoke();
            return true;
        }

        public void ClearAllAnimals()
        {
            animalService?.ClearAllAnimals();
            selectionService?.SelectCell(null);
            stateChanged?.Invoke();
        }

        private void NotifyAfterAnimalRemoval()
        {
            if (selectionService == null || !selectionService.ClearIfSelectionLostAnimal())
            {
                stateChanged?.Invoke();
            }
        }
    }
}
