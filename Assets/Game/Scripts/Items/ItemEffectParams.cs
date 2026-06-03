using System;
using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Items
{
    [Serializable]
    public class ItemEffectParams
    {
        [InspectorName("金币数值")]
        public int money;
        [InspectorName("金子数值")]
        public int gold;
        [InspectorName("罐头数值")]
        public int cans;
        [InspectorName("数量")]
        public int count = 1;
        [InspectorName("最大数量")]
        public int maxCount;
        [InspectorName("天数")]
        public int day;
        [InspectorName("触发次数")]
        public int tickCount;
        [InspectorName("持续天数")]
        public int durationDays;
        [InspectorName("最小值")]
        public int minValue;
        [InspectorName("最大值")]
        public int maxValue;
        [InspectorName("等级")]
        public int level;
        [InspectorName("化石数值")]
        public int fossil;
        [InspectorName("倍率")]
        public float multiplier = 1f;
        [InspectorName("额外加成")]
        public float bonus;
        [Range(0f, 1f), InspectorName("概率")]
        public float probability = 1f;
        [InspectorName("动物ID")]
        public string animalId;
        [InspectorName("目标动物ID")]
        public string targetAnimalId;
        [InspectorName("动物家族")]
        public string family;
        [InspectorName("目标家族")]
        public string targetFamily;
        [InspectorName("地块类型")]
        public string tileType;
        [InspectorName("状态类型")]
        public string statusType;
        [InspectorName("资源类型")]
        public string resourceType;
        [InspectorName("标签")]
        public List<string> tags = new List<string>();
    }
}
