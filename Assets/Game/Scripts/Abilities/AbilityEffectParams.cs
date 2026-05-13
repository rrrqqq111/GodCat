using System;

namespace NekogamiRanch.Abilities
{
    [Serializable]
    public class AbilityEffectParams
    {
        public int money;
        public int delayDays;
        public int durationDays = 1;
        public int transformChancePercent = 100;
        public string type = "Flat";
        public string target = "Self";
        public string targetFamily = "None";
    }
}
