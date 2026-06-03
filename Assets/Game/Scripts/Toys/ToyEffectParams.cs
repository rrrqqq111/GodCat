using System;
using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Toys
{
    [Serializable]
    public class ToyEffectParams
    {
        [InspectorName("金币数值")]
        public int money;
        [InspectorName("罐头数值")]
        public int cans;
        [InspectorName("数量")]
        public int count = 1;
        [InspectorName("触发次数")]
        public int tickCount;
        [InspectorName("阶段")]
        public int stage;
        [InspectorName("动物选项变化量")]
        public int animalOfferDelta;
        [InspectorName("免费动物刷新次数")]
        public int freeAnimalRefreshCount;
        [InspectorName("免费商店刷新次数")]
        public int freeShopRefreshCount;
        [InspectorName("利息步长")]
        public int interestStep;
        [InspectorName("利息金币")]
        public int interestMoney;
        [InspectorName("稀有度")]
        public int rarity;
        [InspectorName("倍率")]
        public float multiplier = 1f;
        [Range(0f, 1f), InspectorName("概率")]
        public float probability = 1f;
        [InspectorName("动物ID")]
        public string animalId;
        [InspectorName("动物家族")]
        public string animalFamily;
        [InspectorName("动物稀有度")]
        public string animalRarity;
        [InspectorName("道具ID")]
        public string itemId;
        [InspectorName("道具稀有度")]
        public string itemRarity;
        [InspectorName("屏蔽动物稀有度")]
        public string blockedAnimalRarity;
        [InspectorName("屏蔽道具稀有度")]
        public string blockedItemRarity;
        [InspectorName("目标标签")]
        public List<string> targetTags = new List<string>();
    }
}
