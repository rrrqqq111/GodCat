using NekogamiRanch.Ranch;
using NekogamiRanch.Abilities;
using UnityEngine;
using System.Collections.Generic;

namespace NekogamiRanch.Animals
{
    public class Animal
    {
        private readonly List<TemporaryIncomeMultiplier> temporaryIncomeMultipliers = new List<TemporaryIncomeMultiplier>();
        private readonly Dictionary<string, int> runtimeCounters = new Dictionary<string, int>();
        private EvolutionRuntimeState evolutionState = new EvolutionRuntimeState();
        private int baseMoneyBonus;
        private int extraMoneyMultiplier = 1;

        public Animal(AnimalData data, Vector2Int coords)
        {
            Data = data;
            Coords = coords;
            Ability = AnimalAbilityFactory.Create(data);
        }

        public AnimalData Data { get; }
        public IAnimalAbility Ability { get; }
        public Vector2Int Coords { get; private set; }
        public int AgeDays { get; private set; }

        public string DisplayName => Data != null ? Data.DisplayName : "Animal";
        public int BaseMoney => (Data != null ? Data.BaseMoney : 0) + baseMoneyBonus;
        public bool HasEvolution => Data != null && Data.HasEvolution;
        public int EvolutionThreshold => Data != null ? Data.EvolutionThreshold : 0;
        public int EvolutionLevel => HasEvolution ? evolutionState.Level : 0;
        public int EvolutionProgress => HasEvolution ? evolutionState.Progress : 0;

        public void SetCoords(Vector2Int coords)
        {
            Coords = coords;
        }

        public int ResolveBaseMoney(RanchMap map)
        {
            AgeDays++;
            var multiplierBonus = 0;
            for (var i = temporaryIncomeMultipliers.Count - 1; i >= 0; i--)
            {
                var entry = temporaryIncomeMultipliers[i];
                if (entry.RemainingDays <= 0)
                {
                    temporaryIncomeMultipliers.RemoveAt(i);
                    continue;
                }

                multiplierBonus += entry.MultiplierBonus;
                entry.RemainingDays--;
                if (entry.RemainingDays <= 0)
                {
                    temporaryIncomeMultipliers.RemoveAt(i);
                }
                else
                {
                    temporaryIncomeMultipliers[i] = entry;
                }
            }

            return BaseMoney * (1 + multiplierBonus);
        }

        public void AddIncomeMultiplierBonus(int bonus, int durationDays)
        {
            if (bonus <= 0 || durationDays <= 0)
            {
                return;
            }

            temporaryIncomeMultipliers.Add(new TemporaryIncomeMultiplier
            {
                MultiplierBonus = bonus,
                RemainingDays = durationDays
            });
        }

        public void AddExtraMoneyMultiplier(int multiplier, bool stackable)
        {
            if (multiplier <= 1)
            {
                return;
            }

            extraMoneyMultiplier = stackable ? extraMoneyMultiplier * multiplier : Mathf.Max(extraMoneyMultiplier, multiplier);
        }

        public int ResolveExtraMoney(int amount)
        {
            if (amount <= 0 || extraMoneyMultiplier <= 1)
            {
                return amount;
            }

            return amount * extraMoneyMultiplier;
        }

        public void ResetSettlementModifiers()
        {
            extraMoneyMultiplier = 1;
        }

        public void AddPermanentBaseMoneyBonus(int bonus)
        {
            baseMoneyBonus += bonus;
        }

        public int AddEvolutionProgress(int amount = 1)
        {
            if (!HasEvolution || amount <= 0)
            {
                return 0;
            }

            evolutionState.Progress += amount;
            var levelsGained = 0;
            while (evolutionState.Progress >= EvolutionThreshold)
            {
                evolutionState.Progress -= EvolutionThreshold;
                evolutionState.Level++;
                levelsGained++;
            }

            return levelsGained;
        }

        public void InheritEvolutionStateFrom(Animal source)
        {
            if (source != null)
            {
                evolutionState = source.evolutionState;
            }
        }

        public int GetRuntimeCounter(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && runtimeCounters.TryGetValue(key, out var value) ? value : 0;
        }

        public void SetRuntimeCounter(string key, int value)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                runtimeCounters[key] = value;
            }
        }

        public int AddRuntimeCounter(string key, int amount = 1)
        {
            var value = GetRuntimeCounter(key) + amount;
            SetRuntimeCounter(key, value);
            return value;
        }

        public AbilityExecutionResult ResolveDayStartAbility(AnimalAbilityContext context)
        {
            return Ability != null ? Ability.TryExecute(context, "DayStart") : AbilityExecutionResult.Failed(triggerType: "DayStart");
        }

        public AbilityExecutionResult ResolveDayEndAbility(AnimalAbilityContext context)
        {
            return Ability != null ? Ability.TryExecute(context, "DayEnd") : AbilityExecutionResult.Failed(triggerType: "DayEnd");
        }

        private struct TemporaryIncomeMultiplier
        {
            public int MultiplierBonus;
            public int RemainingDays;
        }

        private class EvolutionRuntimeState
        {
            public int Level = 1;
            public int Progress;
        }
    }
}
