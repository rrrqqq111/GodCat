using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public static class AnimalAbilityFactory
    {
        public static IAnimalAbility Create(AnimalData data)
        {
            if (data == null || data.Ability == null || string.IsNullOrWhiteSpace(data.Ability.Id))
            {
                return null;
            }

            return new ConfiguredAnimalAbility(data.Ability);
        }
    }
}
