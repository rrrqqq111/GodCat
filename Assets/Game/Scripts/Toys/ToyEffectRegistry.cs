using System;
using System.Collections.Generic;

namespace NekogamiRanch.Toys
{
    public static class ToyEffectRegistry
    {
        private static readonly Dictionary<string, Func<IToyEffect>> Factories = new Dictionary<string, Func<IToyEffect>>
        {
            { AddCansOnRunStartToyEffect.EffectId, () => new AddCansOnRunStartToyEffect() },
        };

        public static bool TryCreate(string effectScriptId, out IToyEffect effect)
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
