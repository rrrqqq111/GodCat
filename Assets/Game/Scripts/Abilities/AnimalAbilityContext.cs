using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public readonly struct AnimalAbilityContext
    {
        public AnimalAbilityContext(
            RanchManager ranchManager,
            Animal owner,
            Animal predator = null,
            Animal preyedAnimal = null,
            Vector2Int? removedCoords = null)
        {
            RanchManager = ranchManager;
            Owner = owner;
            Predator = predator;
            PreyedAnimal = preyedAnimal;
            RemovedCoords = removedCoords;
        }

        public RanchManager RanchManager { get; }
        public Animal Owner { get; }
        public Animal Predator { get; }
        public Animal PreyedAnimal { get; }
        public Vector2Int? RemovedCoords { get; }
    }
}
