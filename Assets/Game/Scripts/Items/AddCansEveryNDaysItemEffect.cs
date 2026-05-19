using UnityEngine;

namespace NekogamiRanch.Items
{
    public class AddCansEveryNDaysItemEffect : IItemEffect
    {
        public const string EffectId = "add_cans_every_n_days";

        public ItemUseResult TryExecute(ItemRuntimeState item, ItemUseContext context)
        {
            if (item?.Data == null || context.Economy == null || context.TriggerType != ItemTriggerType.DayStart)
            {
                return ItemUseResult.Failed();
            }

            var parameters = item.Data.EffectParams;
            var intervalDays = Mathf.Max(1, parameters.day > 0 ? parameters.day : parameters.tickCount);
            var cans = Mathf.Max(1, parameters.cans);

            item.AddTick();
            if (item.Tick < intervalDays)
            {
                return ItemUseResult.Succeeded();
            }

            item.ResetTick();
            context.Economy.AddCans(cans * Mathf.Max(1, item.Count));
            return ItemUseResult.Succeeded($"{item.Data.DisplayName} +{cans}罐头");
        }
    }
}
