using System.Collections.Generic;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchMap : MonoBehaviour
    {
        [SerializeField] private int width = 4;
        [SerializeField] private int height = 5;

        private readonly Dictionary<Vector2Int, MapCell> cells = new Dictionary<Vector2Int, MapCell>();
        private Sprite tileSprite;
        private Sprite animalSprite;

        public int Width => width;
        public int Height => height;
        public IReadOnlyDictionary<Vector2Int, MapCell> Cells => cells;

        public void Initialize(RanchManager manager, int mapWidth, int mapHeight, Sprite tile, Sprite animal, IReadOnlyList<SpriteRenderer> sceneTiles = null)
        {
            width = mapWidth;
            height = mapHeight;
            tileSprite = tile;
            animalSprite = animal;

            if (sceneTiles == null || sceneTiles.Count < width * height)
            {
                cells.Clear();
                var count = sceneTiles != null ? sceneTiles.Count : 0;
                Debug.LogError($"[RanchMap] Scene needs {width * height} tile renderers but only found {count}. Please place map tiles in the scene.");
                return;
            }

            BindSceneTiles(manager, sceneTiles);
        }

        public bool TryGetCell(Vector2Int coords, out MapCell cell)
        {
            return cells.TryGetValue(coords, out cell);
        }

        public bool TryPlaceAnimal(Animal animal, Vector2Int coords)
        {
            return TryGetCell(coords, out var cell) && cell.TryPlaceAnimal(animal);
        }

        public bool TryMoveAnimal(Animal animal, Vector2Int targetCoords)
        {
            if (animal == null || !TryGetCell(animal.Coords, out var sourceCell) || !TryGetCell(targetCoords, out var targetCell))
            {
                return false;
            }

            if (sourceCell.Animal != animal || !targetCell.IsEmpty)
            {
                return false;
            }

            sourceCell.RemoveAnimal();
            if (targetCell.TryPlaceAnimal(animal))
            {
                return true;
            }

            sourceCell.TryPlaceAnimal(animal);
            return false;
        }

        public bool TrySwapAnimals(Animal first, Animal second)
        {
            if (first == null || second == null || first == second)
            {
                return false;
            }

            if (!TryGetCell(first.Coords, out var firstCell) || !TryGetCell(second.Coords, out var secondCell))
            {
                return false;
            }

            if (firstCell.Animal != first || secondCell.Animal != second)
            {
                return false;
            }

            firstCell.RemoveAnimal();
            secondCell.RemoveAnimal();

            if (firstCell.TryPlaceAnimal(second) && secondCell.TryPlaceAnimal(first))
            {
                return true;
            }

            firstCell.RemoveAnimal();
            secondCell.RemoveAnimal();
            firstCell.TryPlaceAnimal(first);
            secondCell.TryPlaceAnimal(second);
            return false;
        }

        public bool TryRemoveAnimal(Animal animal)
        {
            if (animal == null || !TryGetCell(animal.Coords, out var cell) || cell.Animal != animal)
            {
                return false;
            }

            cell.RemoveAnimal();
            return true;
        }

        public IEnumerable<MapCell> GetCells()
        {
            return cells.Values;
        }

        public IEnumerable<MapCell> GetCellsInScanOrder()
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (TryGetCell(new Vector2Int(x, y), out var cell))
                    {
                        yield return cell;
                    }
                }
            }
        }

        public IEnumerable<MapCell> GetNeighbors(Vector2Int coords)
        {
            foreach (var neighborCoords in GetNeighborCoords(coords))
            {
                if (TryGetCell(neighborCoords, out var cell))
                {
                    yield return cell;
                }
            }
        }

        public IEnumerable<MapCell> GetUpperNeighbors(Vector2Int coords)
        {
            foreach (var neighborCoords in GetUpperNeighborCoords(coords))
            {
                if (TryGetCell(neighborCoords, out var cell))
                {
                    yield return cell;
                }
            }
        }

        public IEnumerable<MapCell> GetLowerNeighbors(Vector2Int coords)
        {
            foreach (var neighborCoords in GetLowerNeighborCoords(coords))
            {
                if (TryGetCell(neighborCoords, out var cell))
                {
                    yield return cell;
                }
            }
        }

        public MapCell GetFirstUpperNeighborWithAnimal(Vector2Int coords)
        {
            foreach (var cell in GetUpperNeighbors(coords))
            {
                if (cell.Animal != null)
                {
                    return cell;
                }
            }

            return null;
        }

        private IEnumerable<Vector2Int> GetNeighborCoords(Vector2Int coords)
        {
            yield return coords + Vector2Int.left;
            yield return coords + Vector2Int.right;

            foreach (var upperCoords in GetUpperNeighborCoords(coords))
            {
                yield return upperCoords;
            }

            foreach (var lowerCoords in GetLowerNeighborCoords(coords))
            {
                yield return lowerCoords;
            }
        }

        private IEnumerable<Vector2Int> GetUpperNeighborCoords(Vector2Int coords)
        {
            if (IsOddRow(coords.y))
            {
                yield return new Vector2Int(coords.x, coords.y - 1);
                yield return new Vector2Int(coords.x + 1, coords.y - 1);
            }
            else
            {
                yield return new Vector2Int(coords.x - 1, coords.y - 1);
                yield return new Vector2Int(coords.x, coords.y - 1);
            }
        }

        private IEnumerable<Vector2Int> GetLowerNeighborCoords(Vector2Int coords)
        {
            if (IsOddRow(coords.y))
            {
                yield return new Vector2Int(coords.x, coords.y + 1);
                yield return new Vector2Int(coords.x + 1, coords.y + 1);
            }
            else
            {
                yield return new Vector2Int(coords.x - 1, coords.y + 1);
                yield return new Vector2Int(coords.x, coords.y + 1);
            }
        }

        private static bool IsOddRow(int y)
        {
            return Mathf.Abs(y % 2) == 1;
        }

        private void BindSceneTiles(RanchManager manager, IReadOnlyList<SpriteRenderer> sceneTiles)
        {
            cells.Clear();

            if (TryBindWithExplicitCoords(manager, sceneTiles))
            {
                return;
            }

            Debug.LogError("[RanchMap] Scene tiles are missing complete SceneGridCellMarker data. Select the tile root and run Tools/Game/Assign Scene Grid Coords From Position.");
        }

        private bool TryBindWithExplicitCoords(RanchManager manager, IReadOnlyList<SpriteRenderer> sceneTiles)
        {
            var expectedCount = width * height;
            var usedCoords = new HashSet<Vector2Int>();
            var candidateCount = 0;

            foreach (var tile in sceneTiles)
            {
                var marker = tile.GetComponent<SceneGridCellMarker>();
                if (marker == null)
                {
                    continue;
                }

                candidateCount++;
                var coords = marker.GridCoords;
                if (!IsInsideBounds(coords))
                {
                    Debug.LogError($"[RanchMap] Tile '{tile.name}' has out-of-bounds coords ({coords.x},{coords.y}).");
                    cells.Clear();
                    return false;
                }

                if (!usedCoords.Add(coords))
                {
                    Debug.LogError($"[RanchMap] Duplicate grid coords detected at ({coords.x},{coords.y}).");
                    cells.Clear();
                    return false;
                }

                var cell = tile.GetComponent<MapCell>() ?? tile.gameObject.AddComponent<MapCell>();
                tile.transform.SetParent(transform, true);
                cell.Initialize(manager, coords, tile.sprite != null ? tile.sprite : tileSprite, animalSprite, true);
                cells.Add(coords, cell);
            }

            if (candidateCount < expectedCount || cells.Count != expectedCount)
            {
                cells.Clear();
                return false;
            }

            return true;
        }

        private bool IsInsideBounds(Vector2Int coords)
        {
            return coords.x >= 0 && coords.x < width && coords.y >= 0 && coords.y < height;
        }
    }
}
