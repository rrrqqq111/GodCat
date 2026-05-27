using System.Collections.Generic;

namespace NekogamiRanch.MapObjects
{
    public static class MapCellObjectEffectRegistry
    {
        private static readonly Dictionary<string, IMapCellObjectEffect> Effects = new Dictionary<string, IMapCellObjectEffect>
        {
            { "PoopReward", new ConsumeMapCellObjectAbilityEffect() },
            { "LeftoverMeatReward", new ConsumeMapCellObjectAbilityEffect() }
        };

        public static bool TryGet(string effectScriptId, out IMapCellObjectEffect effect)
        {
            if (string.IsNullOrWhiteSpace(effectScriptId))
            {
                effect = null;
                return false;
            }

            return Effects.TryGetValue(effectScriptId, out effect);
        }
    }
}
