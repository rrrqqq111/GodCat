using System;
using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Toys
{
    [Serializable]
    public class ToyEffectParams
    {
        public int money;
        public int cans;
        public int count = 1;
        public int tickCount;
        public int stage;
        public int animalOfferDelta;
        public int freeAnimalRefreshCount;
        public int freeShopRefreshCount;
        public int interestStep;
        public int interestMoney;
        public int rarity;
        public float multiplier = 1f;
        [Range(0f, 1f)] public float probability = 1f;
        public string animalId;
        public string animalFamily;
        public string animalRarity;
        public string itemId;
        public string itemRarity;
        public string blockedAnimalRarity;
        public string blockedItemRarity;
        public List<string> targetTags = new List<string>();
    }
}
