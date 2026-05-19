namespace NekogamiRanch.Abilities
{
    public readonly struct AbilityExecutionResult
    {
        public AbilityExecutionResult(bool success, string abilityId, string triggerType, int targetCount, int moneyDelta)
        {
            Success = success;
            AbilityId = abilityId;
            TriggerType = triggerType;
            TargetCount = targetCount;
            MoneyDelta = moneyDelta;
        }

        public bool Success { get; }
        public string AbilityId { get; }
        public string TriggerType { get; }
        public int TargetCount { get; }
        public int MoneyDelta { get; }

        public static AbilityExecutionResult Failed(string abilityId = "", string triggerType = "", int targetCount = 0)
        {
            return new AbilityExecutionResult(false, abilityId, triggerType, targetCount, 0);
        }

        public static AbilityExecutionResult Succeeded(string abilityId, string triggerType, int targetCount)
        {
            return new AbilityExecutionResult(true, abilityId, triggerType, targetCount, 0);
        }

        public AbilityExecutionResult WithMoneyDelta(int moneyDelta)
        {
            return new AbilityExecutionResult(Success, AbilityId, TriggerType, TargetCount, moneyDelta);
        }
    }
}
