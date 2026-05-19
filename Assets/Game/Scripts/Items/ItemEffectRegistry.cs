using System;
using System.Collections.Generic;

namespace NekogamiRanch.Items
{
    public static class ItemEffectRegistry
    {
        private static readonly Dictionary<string, Func<IItemEffect>> Factories = new Dictionary<string, Func<IItemEffect>>
        {
            { AddCansEveryNDaysItemEffect.EffectId, () => new AddCansEveryNDaysItemEffect() },
        };

        public static bool TryCreate(string effectScriptId, out IItemEffect effect)
        {
            effect = null;
            if (string.IsNullOrWhiteSpace(effectScriptId) || !Factories.TryGetValue(effectScriptId, out var factory))
            {
                return false;
            }

            effect = factory();
            return true;
        }
    }
}
