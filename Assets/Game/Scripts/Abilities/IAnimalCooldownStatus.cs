namespace NekogamiRanch.Abilities
{
    public interface IAnimalCooldownStatus
    {
        bool HasCooldown { get; }
        int RemainingCooldown { get; }
    }
}
