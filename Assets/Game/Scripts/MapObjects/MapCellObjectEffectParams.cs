using System;

namespace NekogamiRanch.MapObjects
{
    [Serializable]
    public class MapCellObjectEffectParams
    {
        public int sourceBaseMoneyMinPercent = 20;
        public int sourceBaseMoneyMaxPercent = 50;
        public int flatBaseMoneyBonus = 5;
        public int moneyMultiplier = 1;
        public int moneyBonus;
    }
}
