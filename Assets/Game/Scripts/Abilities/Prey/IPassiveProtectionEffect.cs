using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities.Prey
{
    public interface IPassiveProtectionEffect
    {
        ProtectionRule CreateProtectionRule(Animal protector, AbilityData abilityData);
        void OnProtected(Animal protector, Animal protectedAnimal, AbilityData abilityData);
    }
}
