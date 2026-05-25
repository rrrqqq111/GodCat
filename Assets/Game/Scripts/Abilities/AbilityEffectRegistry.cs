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
            { "GoatAbilityEffect", new GoatAbilityEffect() },
            { "LambAbilityEffect", new LambAbilityEffect() },
            { "AlpacaAbilityEffect", new AlpacaAbilityEffect() },
            { "CalfAbilityEffect", new CalfAbilityEffect() },
            { "CowAbilityEffect", new CowAbilityEffect() },
            { "MuskOxAbilityEffect", new MuskOxAbilityEffect() },
            { "HippoAbilityEffect", new HippoAbilityEffect() },
            { "ReindeerAbilityEffect", new ReindeerAbilityEffect() },
            { "WaterBuffaloAbilityEffect", new WaterBuffaloAbilityEffect() },
            { "CamelAbilityEffect", new CamelAbilityEffect() },
            { "RaccoonAbilityEffect", new RaccoonAbilityEffect() },
            { "GrayWolfAbilityEffect", new GrayWolfAbilityEffect() },
            { "TigerAbilityEffect", new TigerAbilityEffect() }
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
