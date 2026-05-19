using System;
using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Items
{
    [Serializable]
    public class ItemEffectParams
    {
        public int money;
        public int gold;
        public int cans;
        public int count = 1;
        public int maxCount;
        public int day;
        public int tickCount;
        public int durationDays;
        public int minValue;
        public int maxValue;
        public int level;
        public int fossil;
        public float multiplier = 1f;
        public float bonus;
        [Range(0f, 1f)] public float probability = 1f;
        public string animalId;
        public string targetAnimalId;
        public string family;
        public string targetFamily;
        public string tileType;
        public string statusType;
        public string resourceType;
        public List<string> tags = new List<string>();
    }
}
