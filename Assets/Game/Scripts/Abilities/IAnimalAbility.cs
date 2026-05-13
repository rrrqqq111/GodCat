namespace NekogamiRanch.Abilities
{
    public interface IAnimalAbility
    {
        string Name { get; }
        int Priority { get; }
        void TryExecute(AnimalAbilityContext context, string triggerType);
    }
}
