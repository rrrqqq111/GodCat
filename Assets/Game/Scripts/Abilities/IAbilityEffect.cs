using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public interface IAbilityEffect
    {
        bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets);
    }
}
