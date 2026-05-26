using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchBoardService
    {
        private readonly RanchMap ranchMap;
        private readonly Action<Animal, Vector2Int> animalMoved;

        public RanchBoardService(RanchMap ranchMap, Action<Animal, Vector2Int> animalMoved)
        {
            this.ranchMap = ranchMap;
            this.animalMoved = animalMoved;
        }

        public bool HasEmptyCell()
        {
            return ranchMap != null && ranchMap.GetCells().Any(cell => cell != null && cell.IsEmpty);
        }

        public bool TryPlaceInRandomEmptyCell(Animal animal)
        {
            if (animal == null || ranchMap == null)
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

            return emptyCells[UnityEngine.Random.Range(0, emptyCells.Count)].TryPlaceAnimal(animal);
        }

        public bool TryPlaceAt(Animal animal, Vector2Int coords)
        {
            return ranchMap != null && ranchMap.TryPlaceAnimal(animal, coords);
        }

        public bool TryMove(Animal animal, Vector2Int targetCoords)
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
                animalMoved?.Invoke(animal, startCoords);
            }

            return true;
        }

        public bool TrySwap(Animal first, Animal second)
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
                animalMoved?.Invoke(first, firstStartCoords);
            }

            if (second.Coords != secondStartCoords)
            {
                animalMoved?.Invoke(second, secondStartCoords);
            }

            return true;
        }

        public bool Remove(Animal animal)
        {
            return ranchMap != null && ranchMap.TryRemoveAnimal(animal);
        }

        public bool IsDeployed(Animal animal)
        {
            return animal != null &&
                ranchMap != null &&
                ranchMap.TryGetCell(animal.Coords, out var cell) &&
                cell.Animal == animal;
        }

        public int CountDeployedAnimalsById(string animalId)
        {
            if (string.IsNullOrWhiteSpace(animalId) || ranchMap == null)
            {
                return 0;
            }

            return ranchMap.GetCells().Count(cell =>
                cell?.Animal?.Data != null &&
                string.Equals(cell.Animal.Data.Id, animalId, StringComparison.OrdinalIgnoreCase));
        }

        public void Clear()
        {
            if (ranchMap == null)
            {
                return;
            }

            foreach (var cell in ranchMap.GetCells())
            {
                cell?.RemoveAnimal();
            }
        }

        public void DeployRandom(IReadOnlyList<Animal> animals)
        {
            if (ranchMap == null)
            {
                return;
            }

            var cells = ranchMap.GetCells().Where(cell => cell != null).ToList();
            Clear();
            if (cells.Count == 0 || animals == null || animals.Count == 0)
            {
                return;
            }

            var shuffledCells = cells.OrderBy(_ => UnityEngine.Random.value).ToList();
            var selectedAnimals = animals
                .Where(animal => animal != null)
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(shuffledCells.Count)
                .ToList();

            for (var i = 0; i < selectedAnimals.Count; i++)
            {
                shuffledCells[i].TryPlaceAnimal(selectedAnimals[i]);
            }
        }
    }
}
