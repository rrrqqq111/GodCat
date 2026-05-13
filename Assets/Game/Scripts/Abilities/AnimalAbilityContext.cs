using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;

namespace NekogamiRanch.Abilities
{
    public readonly struct AnimalAbilityContext
    {
        public AnimalAbilityContext(RanchManager ranchManager, Animal owner)
        {
            RanchManager = ranchManager;
            Owner = owner;
        }

        public RanchManager RanchManager { get; }
        public Animal Owner { get; }
    }
}
