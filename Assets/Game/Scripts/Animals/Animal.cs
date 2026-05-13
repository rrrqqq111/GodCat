using NekogamiRanch.Ranch;
using NekogamiRanch.Abilities;
using UnityEngine;
using System.Collections.Generic;

namespace NekogamiRanch.Animals
{
    public class Animal
    {
        private readonly List<TemporaryIncomeMultiplier> temporaryIncomeMultipliers = new List<TemporaryIncomeMultiplier>();
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

        public void AddBaseMoneyBonus(int bonus)
        {
            baseMoneyBonus += bonus;
        }

        public void ResolveDayStartAbility(AnimalAbilityContext context)
        {
            Ability?.TryExecute(context, "DayStart");
        }

        public void ResolveDayEndAbility(AnimalAbilityContext context)
        {
            Ability?.TryExecute(context, "DayEnd");
        }

        private struct TemporaryIncomeMultiplier
        {
            public int MultiplierBonus;
            public int RemainingDays;
        }
    }
}
