using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchTileSystem : MonoBehaviour
    {
        [SerializeField] private RanchTileType defaultTileType = RanchTileType.Normal;
        [SerializeField] private List<TileSpriteBinding> tileSprites = new List<TileSpriteBinding>();
        [SerializeField] private List<CellTileTypeOverride> cellOverrides = new List<CellTileTypeOverride>();

        private readonly Dictionary<RanchTileType, TileSpriteBinding> visualByType = new Dictionary<RanchTileType, TileSpriteBinding>();
        private readonly Dictionary<Vector2Int, RanchTileType> typeByCoords = new Dictionary<Vector2Int, RanchTileType>();
        private RanchMap map;

        public RanchTileType DefaultTileType => defaultTileType;

        public void Initialize(RanchMap ranchMap)
        {
            map = ranchMap;
            RebuildLookups();
            ApplyAll();
        }

        public RanchTileType GetTileType(Vector2Int coords)
        {
            return typeByCoords.TryGetValue(coords, out var tileType) ? tileType : defaultTileType;
        }

        public RanchTileType GetTileType(Animal animal)
        {
            return animal != null ? GetTileType(animal.Coords) : defaultTileType;
        }

        public bool IsTileType(Vector2Int coords, RanchTileType tileType)
        {
            return GetTileType(coords) == tileType;
        }

        public bool IsTileType(Animal animal, RanchTileType tileType)
        {
            return animal != null && IsTileType(animal.Coords, tileType);
        }

        public bool TrySetTileType(Vector2Int coords, RanchTileType tileType)
        {
            if (map == null || !map.TryGetCell(coords, out var cell))
            {
                return false;
            }

            SetTileType(coords, tileType);
            ApplyToCell(cell);
            return true;
        }

        public bool TrySetTileType(MapCell cell, RanchTileType tileType)
        {
            return cell != null && TrySetTileType(cell.Coords, tileType);
        }

        public bool TrySetTileType(Animal animal, RanchTileType tileType)
        {
            return animal != null && TrySetTileType(animal.Coords, tileType);
        }

        public bool TrySetAnimalTileType(Animal animal, RanchTileType tileType)
        {
            return TrySetTileType(animal, tileType);
        }

        public bool TrySetCellTileType(MapCell cell, RanchTileType tileType)
        {
            return TrySetTileType(cell, tileType);
        }

        public void SetTileType(Vector2Int coords, RanchTileType tileType)
        {
            typeByCoords[coords] = tileType;
            SetSerializedOverride(coords, tileType);
        }

        public Sprite GetSprite(RanchTileType tileType)
        {
            return visualByType.TryGetValue(tileType, out var visual) ? visual.sprite : null;
        }

        public Vector2 GetSizeMultiplier(RanchTileType tileType)
        {
            return visualByType.TryGetValue(tileType, out var visual) ? visual.SizeMultiplier : Vector2.one;
        }

        public void ApplyToCell(MapCell cell)
        {
            if (cell == null)
            {
                return;
            }

            var tileType = GetTileType(cell.Coords);
            if (visualByType.TryGetValue(tileType, out var visual) && visual.sprite != null)
            {
                cell.SetTileSprite(visual.sprite, visual.SizeMultiplier, visual.updateColliderSize);
            }
        }

        public void ApplyAll()
        {
            if (map == null)
            {
                return;
            }

            foreach (var cell in map.GetCells())
            {
                ApplyToCell(cell);
            }
        }

        private void RebuildLookups()
        {
            visualByType.Clear();
            foreach (var binding in tileSprites)
            {
                if (binding.sprite != null)
                {
                    visualByType[binding.tileType] = binding;
                }
            }

            typeByCoords.Clear();
            foreach (var entry in cellOverrides)
            {
                typeByCoords[entry.coords] = entry.tileType;
            }
        }

        private void SetSerializedOverride(Vector2Int coords, RanchTileType tileType)
        {
            for (var i = 0; i < cellOverrides.Count; i++)
            {
                if (cellOverrides[i].coords != coords)
                {
                    continue;
                }

                var entry = cellOverrides[i];
                entry.tileType = tileType;
                cellOverrides[i] = entry;
                return;
            }

            cellOverrides.Add(new CellTileTypeOverride
            {
                coords = coords,
                tileType = tileType
            });
        }

        [Serializable]
        private struct TileSpriteBinding
        {
            public RanchTileType tileType;
            public Sprite sprite;
            public Vector2 sizeMultiplier;
            public bool updateColliderSize;

            public Vector2 SizeMultiplier
            {
                get
                {
                    return new Vector2(
                        sizeMultiplier.x > 0f ? sizeMultiplier.x : 1f,
                        sizeMultiplier.y > 0f ? sizeMultiplier.y : 1f);
                }
            }
        }

        [Serializable]
        private struct CellTileTypeOverride
        {
            public Vector2Int coords;
            public RanchTileType tileType;
        }
    }
}
