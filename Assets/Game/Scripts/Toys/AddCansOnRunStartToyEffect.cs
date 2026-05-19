using UnityEngine;

namespace NekogamiRanch.Toys
{
    public class AddCansOnRunStartToyEffect : IToyEffect
    {
        public const string EffectId = "add_cans_on_run_start";

        public ToyUseResult TryExecute(ToyData toy, ToyUseContext context)
        {
            if (toy == null || context.Economy == null || context.TriggerType != ToyTriggerType.RunStart)
            {
                return ToyUseResult.Failed();
            }

            var cans = Mathf.Max(0, toy.EffectParams.cans);
            if (cans <= 0)
            {
                return ToyUseResult.Succeeded();
            }

            context.Economy.AddCans(cans);
            return ToyUseResult.Succeeded($"{toy.DisplayName} +{cans}罐头");
        }
    }
}
