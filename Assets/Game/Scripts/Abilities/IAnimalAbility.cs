namespace NekogamiRanch.Abilities
{
    public interface IAnimalAbility
    {
        string Name { get; }
        int Priority { get; }
        AbilityExecutionResult TryExecute(AnimalAbilityContext context, string triggerType);
    }
}
