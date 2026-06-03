using System;
using NekogamiRanch.Animals;
using UnityEngine;
using UnityEngine.Serialization;

namespace NekogamiRanch.Abilities
{
    [Serializable]
    public class AbilityEffectParams
    {
        [InspectorName("金币数值")]
        public int money;
        [InspectorName("数量")]
        public int count;
        [InspectorName("最大数量")]
        public int maxCount;
        [InspectorName("最小倍率")]
        public int minMultiplier;
        [InspectorName("最大倍率")]
        public int maxMultiplier;
        [InspectorName("最高稀有度")]
        public int maxRarity;
        [InspectorName("道具数量")]
        public int itemCount;
        [FormerlySerializedAs("delayDays")]
        [InspectorName("初始冷却天数")]
        public int initialCooldownDays;
        [InspectorName("冷却天数")]
        public int cooldownDays;
        [InspectorName("冷却减少量")]
        public int cooldownReductionAmount;
        [InspectorName("冷却减少地块类型")]
        public string cooldownReductionTileType = "None";
        [InspectorName("持续天数")]
        public int durationDays = 1;
        [InspectorName("变形概率百分比")]
        public int transformChancePercent = 100;
        [InspectorName("数值类型")]
        public string type = "Flat";
        [InspectorName("目标")]
        public string target = "Self";
        [InspectorName("目标家族")]
        public string targetFamily = "None";
        [InspectorName("动物数据")]
        public AnimalData animalData;
        [InspectorName("成长目标A")]
        public AnimalData growUpAnimalDataA;
        [InspectorName("成长权重A")]
        public int growUpWeightA;
        [InspectorName("成长目标B")]
        public AnimalData growUpAnimalDataB;
        [InspectorName("成长权重B")]
        public int growUpWeightB;
        [InspectorName("成长目标C")]
        public AnimalData growUpAnimalDataC;
        [InspectorName("成长权重C")]
        public int growUpWeightC;
    }
}
