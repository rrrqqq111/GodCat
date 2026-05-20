using System.Collections.Generic;

namespace NekogamiRanch.Abilities
{
    public static class AbilityEffectRegistry
    {
        private static readonly Dictionary<string, IAbilityEffect> Effects = new Dictionary<string, IAbilityEffect>
        {
            { "PigAbilityEffect", new PigAbilityEffect() },
            { "BoarAbilityEffect", new BoarAbilityEffect() },
            { "HorseAbilityEffect", new HorseAbilityEffect() },
            { "SheepAbilityEffect", new SheepAbilityEffect() },
            { "ZebraAbilityEffect", new ZebraAbilityEffect() },
            { "GazelleAbilityEffect", new GazelleAbilityEffect() },
            { "DonkeyAbilityEffect", new DonkeyAbilityEffect() },
            { "AlpacaAbilityEffect", new AlpacaAbilityEffect() },
            { "MuskOxAbilityEffect", new MuskOxAbilityEffect() },
            { "WaterBuffaloAbilityEffect", new WaterBuffaloAbilityEffect() },
            { "CamelAbilityEffect", new CamelAbilityEffect() },
            { "RaccoonAbilityEffect", new RaccoonAbilityEffect() }
        };

        public static bool TryGet(string effectScriptId, out IAbilityEffect effect)
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
