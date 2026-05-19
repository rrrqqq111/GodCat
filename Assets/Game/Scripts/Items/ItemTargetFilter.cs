using System;
using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Items
{
    [Serializable]
    public class ItemTargetFilter
    {
        [SerializeField] private List<string> animalIds = new List<string>();
        [SerializeField] private List<string> families = new List<string>();
        [SerializeField] private List<string> tileTypes = new List<string>();
        [SerializeField] private List<string> tags = new List<string>();
        [SerializeField, Range(0, 4)] private int minRarity;
        [SerializeField, Range(0, 4)] private int maxRarity = 4;
        [SerializeField] private bool requireEmptyCell;
        [SerializeField] private bool requireOccupiedCell;
        [SerializeField] private bool requireAdjacentTarget;

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
