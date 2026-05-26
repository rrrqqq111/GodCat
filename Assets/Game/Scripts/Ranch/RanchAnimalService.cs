using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchAnimalService
    {
        private readonly RanchRosterService rosterService = new RanchRosterService();
        private readonly RanchBoardService boardService;

        public RanchAnimalService(RanchMap ranchMap, Action<Animal, Vector2Int> animalMoved)
        {
            boardService = new RanchBoardService(ranchMap, animalMoved);
        }

        public IReadOnlyList<Animal> Animals => rosterService.Animals;

        public void SeedAnimals(IReadOnlyList<AnimalData> startingAnimals)
        {
            ClearAllAnimals();
            if (startingAnimals == null)
            {
                return;
            }

            for (var i = 0; i < startingAnimals.Count; i++)
            {
                TryAddAnimalToRandomEmptyCell(startingAnimals[i]);
            }

            RandomizeAnimalPositions();
        }

        public bool TryAddAnimalToRandomEmptyCell(AnimalData data)
        {
            return TryAddAnimalToRandomEmptyCell(data, out _);
        }

        public bool TryAddAnimalToRandomEmptyCell(AnimalData data, out Animal animal)
        {
            animal = null;
            if (data == null)
            {
                return false;
            }

            animal = new Animal(data, Vector2Int.zero);
            rosterService.Add(animal);

            if (!boardService.HasEmptyCell())
            {
                return true;
            }

            if (boardService.TryPlaceInRandomEmptyCell(animal))
            {
                return true;
            }

            rosterService.Remove(animal);
            animal = null;
            return false;
        }

        public bool TryMoveAnimal(Animal animal, Vector2Int targetCoords)
        {
            return boardService.TryMove(animal, targetCoords);
        }

        public bool TrySwapAnimals(Animal first, Animal second)
        {
            return boardService.TrySwap(first, second);
        }

        public bool AnimalRemoved(Animal animal)
        {
            if (animal == null)
            {
                return false;
            }

            var removedFromBoard = boardService.Remove(animal);
            var removedFromRoster = rosterService.Remove(animal);
            return removedFromBoard || removedFromRoster;
        }

        public bool AnimalRemovedAt(Vector2Int coords)
        {
            return TryGetAnimalAt(coords, out var animal) && AnimalRemoved(animal);
        }

        public bool AnimalRemovedFromCell(MapCell cell)
        {
            return cell != null && cell.Animal != null && AnimalRemoved(cell.Animal);
        }

        public bool ReplaceAnimal(Animal oldAnimal, AnimalData newAnimalData)
        {
            if (oldAnimal == null || newAnimalData == null || !boardService.IsDeployed(oldAnimal))
            {
                return false;
            }

            var coords = oldAnimal.Coords;
            if (!AnimalRemoved(oldAnimal))
            {
                return false;
            }

            var newAnimal = new Animal(newAnimalData, coords);
            if (!boardService.TryPlaceAt(newAnimal, coords))
            {
                return false;
            }

            rosterService.Add(newAnimal);
            return true;
        }

        public bool TrySetAnimalAt(Vector2Int coords, AnimalData animalData)
        {
            if (animalData == null)
            {
                return false;
            }

            if (TryGetAnimalAt(coords, out var existingAnimal))
            {
                AnimalRemoved(existingAnimal);
            }

            var animal = new Animal(animalData, coords);
            if (!boardService.TryPlaceAt(animal, coords))
            {
                return false;
            }

            rosterService.Add(animal);
            return true;
        }

        public void ClearAllAnimals()
        {
            boardService.Clear();
            rosterService.Clear();
        }

        public int CountAnimalsById(string animalId)
        {
            return boardService.CountDeployedAnimalsById(animalId);
        }

        public bool HasAnimalById(string animalId)
        {
            return CountAnimalsById(animalId) > 0;
        }

        public void RandomizeAnimalPositions()
        {
            boardService.DeployRandom(rosterService.Animals);
        }

        public bool IsAnimalOnMap(Animal animal)
        {
            return boardService.IsDeployed(animal);
        }

        private bool TryGetAnimalAt(Vector2Int coords, out Animal animal)
        {
            animal = null;
            foreach (var candidate in rosterService.Animals)
            {
                if (candidate != null && boardService.IsDeployed(candidate) && candidate.Coords == coords)
                {
                    animal = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
