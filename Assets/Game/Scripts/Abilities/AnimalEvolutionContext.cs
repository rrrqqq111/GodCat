using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public readonly struct AnimalEvolutionContext
    {
        public AnimalEvolutionContext(
            Animal animal,
            int progressGained,
            int previousProgress,
            int newProgress,
            int previousLevel,
            int newLevel,
            string sourceAbilityId = null)
        {
            Animal = animal;
            ProgressGained = progressGained;
            PreviousProgress = previousProgress;
            NewProgress = newProgress;
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
            SourceAbilityId = sourceAbilityId;
        }

        public Animal Animal { get; }
        public int ProgressGained { get; }
        public int PreviousProgress { get; }
        public int NewProgress { get; }
        public int PreviousLevel { get; }
        public int NewLevel { get; }
        public int LevelsGained => NewLevel - PreviousLevel;
        public string SourceAbilityId { get; }
    }
}
