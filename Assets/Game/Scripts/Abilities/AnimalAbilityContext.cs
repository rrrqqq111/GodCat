using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;

namespace NekogamiRanch.Abilities
{
    public readonly struct AnimalAbilityContext
    {
        public AnimalAbilityContext(RanchManager ranchManager, Animal owner, Animal predator = null, Animal preyedAnimal = null)
        {
            RanchManager = ranchManager;
            Owner = owner;
            Predator = predator;
            PreyedAnimal = preyedAnimal;
        }

        public RanchManager RanchManager { get; }
        public Animal Owner { get; }
        public Animal Predator { get; }
        public Animal PreyedAnimal { get; }
    }
}
