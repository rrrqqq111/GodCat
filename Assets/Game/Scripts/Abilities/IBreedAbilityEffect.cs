using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public interface IBreedAbilityEffect
    {
        AnimalData ResolveOffspringAnimalData(AnimalAbilityContext context, AbilityData abilityData);
        bool TryResolveSpawnCoords(AnimalAbilityContext context, out Vector2Int spawnCoords);
    }
}
