using System;
using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Items
{
    [Serializable]
    public class ItemTargetFilter
    {
        [SerializeField, InspectorName("动物ID列表")] private List<string> animalIds = new List<string>();
        [SerializeField, InspectorName("家族列表")] private List<string> families = new List<string>();
        [SerializeField, InspectorName("地块类型列表")] private List<string> tileTypes = new List<string>();
        [SerializeField, InspectorName("标签列表")] private List<string> tags = new List<string>();
        [SerializeField, Range(0, 4), InspectorName("最低稀有度")] private int minRarity;
        [SerializeField, Range(0, 4), InspectorName("最高稀有度")] private int maxRarity = 4;
        [SerializeField, InspectorName("要求空地块")] private bool requireEmptyCell;
        [SerializeField, InspectorName("要求有动物地块")] private bool requireOccupiedCell;
        [SerializeField, InspectorName("要求相邻目标")] private bool requireAdjacentTarget;

        public IReadOnlyList<string> AnimalIds => animalIds;
        public IReadOnlyList<string> Families => families;
        public IReadOnlyList<string> TileTypes => tileTypes;
        public IReadOnlyList<string> Tags => tags;
        public int MinRarity => minRarity;
        public int MaxRarity => maxRarity;
        public bool RequireEmptyCell => requireEmptyCell;
        public bool RequireOccupiedCell => requireOccupiedCell;
        public bool RequireAdjacentTarget => requireAdjacentTarget;
    }
}
