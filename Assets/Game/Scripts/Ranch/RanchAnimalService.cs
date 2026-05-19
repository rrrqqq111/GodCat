using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchAnimalService
    {
        private readonly List<Animal> animals = new List<Animal>();
        private readonly Action<Animal> animalMoved;
        private RanchMap ranchMap;

        public RanchAnimalService(RanchMap ranchMap, Action<Animal> animalMoved)
        {
            this.ranchMap = ranchMap;
            this.animalMoved = animalMoved;
        }

        public IReadOnlyList<Animal> Animals => animals;

        public void SetMap(RanchMap map)
        {
            ranchMap = map;
        }

        public void SeedAnimals(IReadOnlyList<AnimalData> startingAnimals)
        {
            animals.Clear();
            if (startingAnimals == null || startingAnimals.Count == 0)
            {
                return;
            }

            for (var i = 0; i < startingAnimals.Count; i++)
            {
                TryAddAnimalToRandomEmptyCell(startingAnimals[i]);
            }
        }

        public bool TryAddAnimalToRandomEmptyCell(AnimalData data)
        {
            if (data == null || ranchMap == null)
            {
                return false;
            }

            var emptyCells = ranchMap.GetCells()
                .Where(cell => cell != null && cell.IsEmpty)
                .ToList();
            if (emptyCells.Count == 0)
            {
                return false;
            }

            var cell = emptyCells[UnityEngine.Random.Range(0, emptyCells.Count)];
            var animal = new Animal(data, cell.Coords);
            if (!cell.TryPlaceAnimal(animal))
            {
                return false;
            }

            animals.Add(animal);
            return true;
        }

        public bool TryMoveAnimal(Animal animal, Vector2Int targetCoords)
        {
            if (animal == null || ranchMap == null)
            {
                return false;
            }

            var startCoords = animal.Coords;
            if (!ranchMap.TryMoveAnimal(animal, targetCoords))
            {
                return false;
            }

            if (animal.Coords != startCoords)
            {
                animalMoved?.Invoke(animal);
            }

            return true;
        }

        public bool TrySwapAnimals(Animal first, Animal second)
        {
            if (first == null || second == null || ranchMap == null)
            {
                return false;
            }

            var firstStartCoords = first.Coords;
            var secondStartCoords = second.Coords;
            if (!ranchMap.TrySwapAnimals(first, second))
            {
                return false;
            }

            if (first.Coords != firstStartCoords)
            {
                animalMoved?.Invoke(first);
            }

            if (second.Coords != secondStartCoords)
            {
                animalMoved?.Invoke(second);
            }

            return true;
        }

        public bool RemoveAnimal(Animal animal)
        {
            if (animal == null)
            {
                return false;
            }

            var removedFromMap = ranchMap != null && ranchMap.TryRemoveAnimal(animal);
            var removedFromList = animals.Remove(animal);
            return removedFromMap || removedFromList;
        }

        public bool ReplaceAnimal(Animal oldAnimal, AnimalData newAnimalData)
        {
            if (oldAnimal == null || newAnimalData == null || ranchMap == null)
            {
                return false;
            }

            var coords = oldAnimal.Coords;
            if (!RemoveAnimal(oldAnimal))
            {
                return false;
            }

            var newAnimal = new Animal(newAnimalData, coords);
            if (!ranchMap.TryPlaceAnimal(newAnimal, coords))
            {
                return false;
            }

            animals.Add(newAnimal);
            return true;
        }

        public bool TrySetAnimalAt(Vector2Int coords, AnimalData animalData)
        {
            if (ranchMap == null || animalData == null || !ranchMap.TryGetCell(coords, out var cell))
            {
                return false;
            }

            if (cell.Animal != null)
            {
                RemoveAnimal(cell.Animal);
            }

            var animal = new Animal(animalData, coords);
            if (!ranchMap.TryPlaceAnimal(animal, coords))
            {
                return false;
            }

            animals.Add(animal);
            return true;
        }

        public bool TryClearAnimalAt(Vector2Int coords)
        {
            if (ranchMap == null || !ranchMap.TryGetCell(coords, out var cell) || cell.Animal == null)
            {
                return false;
            }

            return RemoveAnimal(cell.Animal);
        }

        public void ClearAllAnimals()
        {
            if (ranchMap != null)
            {
                foreach (var cell in ranchMap.GetCells())
                {
                    cell.RemoveAnimal();
                }
            }

            animals.Clear();
        }

        public int CountAnimalsById(string animalId)
        {
            if (string.IsNullOrWhiteSpace(animalId))
            {
                return 0;
            }

            return animals.Count(animal => animal.Data != null && string.Equals(animal.Data.Id, animalId, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasAnimalById(string animalId)
        {
            return CountAnimalsById(animalId) > 0;
        }

        public void RandomizeAnimalPositions()
        {
            if (ranchMap == null || animals.Count == 0)
            {
                return;
            }

            var allCells = ranchMap.GetCells().Where(cell => cell != null).ToList();
            if (allCells.Count <= 1)
            {
                return;
            }

            foreach (var cell in allCells)
            {
                if (!cell.IsEmpty)
                {
                    cell.RemoveAnimal();
                }
            }

            var shuffledCells = allCells.OrderBy(_ => UnityEngine.Random.value).ToList();
            var maxPlaceCount = Mathf.Min(animals.Count, shuffledCells.Count);
            for (var i = 0; i < maxPlaceCount; i++)
            {
                shuffledCells[i].TryPlaceAnimal(animals[i]);
            }
        }
    }
}
