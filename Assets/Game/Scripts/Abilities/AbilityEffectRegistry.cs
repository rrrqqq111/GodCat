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
            { "CapreolusAbilityEffect", new CapreolusAbilityEffect() },
            { "RhinoAbilityEffect", new RhinoAbilityEffect() },
            { "ElkAbilityEffect", new ElkAbilityEffect() },
            { "RainbowUnicornAbilityEffect", new RainbowUnicornAbilityEffect() },
            { "GiraffeAbilityEffect", new GiraffeAbilityEffect() },
            { "SheepdogAbilityEffect", new SheepdogAbilityEffect() },
            { "SkunkAbilityEffect", new SkunkAbilityEffect() },
            { "CoyoteAbilityEffect", new CoyoteAbilityEffect() },
            { "CheetahAbilityEffect", new CheetahAbilityEffect() },
            { "BadgerAbilityEffect", new BadgerAbilityEffect() },
            { "LionessAbilityEffect", new LionessAbilityEffect() },
            { "MaleLionAbilityEffect", new MaleLionAbilityEffect() },
            { "HyenaAbilityEffect", new HyenaAbilityEffect() },
            { "RedPandaAbilityEffect", new RedPandaAbilityEffect() },
            { "PandaAbilityEffect", new PandaAbilityEffect() },
            { "CrocodileAbilityEffect", new CrocodileAbilityEffect() },
            { "SaltwaterCrocodileAbilityEffect", new SaltwaterCrocodileAbilityEffect() },
            { "BrownBearAbilityEffect", new BrownBearAbilityEffect() },
            { "SnowLeopardAbilityEffect", new SnowLeopardAbilityEffect() },
            { "OwlAbilityEffect", new OwlAbilityEffect() },
            { "TanukiAbilityEffect", new TanukiAbilityEffect() },
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
