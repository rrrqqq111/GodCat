using System.Linq;
using NekogamiRanch.Abilities;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public class RanchBaseMoneyBonusTriggerService
    {
        public const string AdjacentTriggerType = "BaseMoneyIncreasedAdjacent";
        public const string IncreasedAmountCounterKey = "AdjacentBaseMoneyIncreaseAmount";

        private readonly RanchMap ranchMap;
        private readonly System.Func<Animal, string, AbilityExecutionResult> triggerAbility;

        public RanchBaseMoneyBonusTriggerService(
            RanchMap ranchMap,
            System.Func<Animal, string, AbilityExecutionResult> triggerAbility)
        {
            this.ranchMap = ranchMap;
            this.triggerAbility = triggerAbility;
        }

        public void AddBaseMoneyBonus(Animal target, int bonus)
        {
            if (target == null || bonus == 0)
            {
                return;
            }

            target.AddPermanentBaseMoneyBonus(bonus);
            if (bonus <= 0 ||
                ranchMap == null ||
                triggerAbility == null ||
                !ranchMap.TryGetCell(target.Coords, out var targetCell) ||
                targetCell.Animal != target)
            {
                return;
            }

            var observers = ranchMap.GetNeighbors(target.Coords)
                .Select(cell => cell.Animal)
                .Where(animal => animal != null && animal != target)
                .Distinct()
                .ToList();

            foreach (var observer in observers)
            {
                observer.SetRuntimeCounter(IncreasedAmountCounterKey, bonus);
                try
                {
                    triggerAbility(observer, AdjacentTriggerType);
                }
                finally
                {
                    observer.SetRuntimeCounter(IncreasedAmountCounterKey, 0);
                }
            }
        }
    }
}
