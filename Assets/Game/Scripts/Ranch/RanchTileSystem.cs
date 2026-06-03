using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;
using NekogamiRanch.Terrains;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchTileSystem : MonoBehaviour
    {
        [SerializeField] private string defaultTerrainId = RanchTerrainIds.Normal;
        [SerializeField] private List<RanchTerrainData> terrainData = new List<RanchTerrainData>();
        [SerializeField] private List<CellTerrainOverride> terrainOverrides = new List<CellTerrainOverride>();

        private readonly Dictionary<string, RanchTerrainData> dataById = new Dictionary<string, RanchTerrainData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Vector2Int, string> terrainIdByCoords = new Dictionary<Vector2Int, string>();
        private RanchMap map;

        public string DefaultTerrainId => NormalizeTerrainId(defaultTerrainId);
        public IReadOnlyList<RanchTerrainData> TerrainData => terrainData;

        public void Initialize(RanchMap ranchMap)
        {
            map = ranchMap;
            RebuildLookups();
            ApplyAll();
        }

        public string GetTerrainId(Vector2Int coords)
        {
            if (terrainIdByCoords.TryGetValue(coords, out var terrainId))
            {
                return NormalizeTerrainId(terrainId);
            }

            return DefaultTerrainId;
        }

        public string GetTerrainId(Animal animal)
        {
            return animal != null ? GetTerrainId(animal.Coords) : DefaultTerrainId;
        }

        public bool IsTerrain(Vector2Int coords, string terrainId)
        {
            return string.Equals(GetTerrainId(coords), NormalizeTerrainId(terrainId), StringComparison.OrdinalIgnoreCase);
        }

        public bool IsTerrain(Animal animal, string terrainId)
        {
            return animal != null && IsTerrain(animal.Coords, terrainId);
        }

        public bool TrySetTerrainId(Vector2Int coords, string terrainId)
        {
            if (map == null || !map.TryGetCell(coords, out var cell))
            {
                return false;
            }

            SetTerrainId(coords, terrainId);
            ApplyToCell(cell);
            return true;
        }

        public bool TrySetTerrainId(MapCell cell, string terrainId)
        {
            return cell != null && TrySetTerrainId(cell.Coords, terrainId);
        }

        public bool TrySetTerrainId(Animal animal, string terrainId)
        {
            return animal != null && TrySetTerrainId(animal.Coords, terrainId);
        }

        public void SetTerrainId(Vector2Int coords, string terrainId)
        {
            var normalizedId = NormalizeTerrainId(terrainId);
            terrainIdByCoords[coords] = normalizedId;
            SetSerializedTerrainOverride(coords, normalizedId);
        }

        public Sprite GetSprite(string terrainId)
        {
            if (dataById.TryGetValue(NormalizeTerrainId(terrainId), out var terrain) && terrain.TileSprite != null)
            {
                return terrain.TileSprite;
            }

            return null;
        }

        public Vector2 GetSizeMultiplier(string terrainId)
        {
            if (dataById.TryGetValue(NormalizeTerrainId(terrainId), out var terrain))
            {
                return terrain.SizeMultiplier;
            }

            return Vector2.one;
        }

        public void ApplyToCell(MapCell cell)
        {
            if (cell == null)
            {
                return;
            }

            var terrainId = GetTerrainId(cell.Coords);
            if (dataById.TryGetValue(terrainId, out var terrain) && terrain.TileSprite != null)
            {
                cell.SetTileSprite(terrain.TileSprite, terrain.SizeMultiplier, terrain.UpdateColliderSize);
                return;
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
            dataById.Clear();
            foreach (var data in terrainData)
            {
                if (data != null)
                {
                    dataById[data.Id] = data;
                }
            }

            terrainIdByCoords.Clear();
            foreach (var entry in terrainOverrides)
            {
                terrainIdByCoords[entry.coords] = NormalizeTerrainId(entry.terrainId);
            }
        }

        private void SetSerializedTerrainOverride(Vector2Int coords, string terrainId)
        {
            for (var i = 0; i < terrainOverrides.Count; i++)
            {
                if (terrainOverrides[i].coords != coords)
                {
                    continue;
                }

                var entry = terrainOverrides[i];
                entry.terrainId = terrainId;
                terrainOverrides[i] = entry;
                return;
            }

            terrainOverrides.Add(new CellTerrainOverride
            {
                coords = coords,
                terrainId = terrainId
            });
        }

        private static string NormalizeTerrainId(string terrainId)
        {
            return string.IsNullOrWhiteSpace(terrainId) ? RanchTerrainIds.Normal : terrainId.Trim();
        }

        [Serializable]
        private struct CellTerrainOverride
        {
            public Vector2Int coords;
            public string terrainId;
        }
    }
}
