using System;
using UnityEngine;

namespace NekogamiRanch.MapObjects
{
    [Serializable]
    public class MapCellObjectEffectParams
    {
        [InspectorName("来源基础金币最小百分比")]
        public int sourceBaseMoneyMinPercent = 20;
        [InspectorName("来源基础金币最大百分比")]
        public int sourceBaseMoneyMaxPercent = 50;
        [InspectorName("固定基础金币加成")]
        public int flatBaseMoneyBonus = 5;
        [InspectorName("金币倍率")]
        public int moneyMultiplier = 1;
        [InspectorName("金币加成")]
        public int moneyBonus;
    }
}
