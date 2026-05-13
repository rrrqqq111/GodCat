using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchMap : MonoBehaviour
    {
        [SerializeField] private int width = 5;
        [SerializeField] private int height = 5;
        [SerializeField] private float cellSize = 1.12f;

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

            if (sceneTiles != null && sceneTiles.Count >= width * height)
            {
                BindSceneTiles(manager, sceneTiles);
            }
            else
            {
                BuildCells(manager);
            }
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
            for (var y = height - 1; y >= 0; y--)
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
            var offsets = new[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            foreach (var offset in offsets)
            {
                if (TryGetCell(coords + offset, out var cell))
                {
                    yield return cell;
                }
            }
        }

        private void BuildCells(RanchManager manager)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            cells.Clear();
            var offset = new Vector2((width - 1) * cellSize * 0.5f, (height - 1) * cellSize * 0.5f);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var coords = new Vector2Int(x, y);
                    var cellObj = new GameObject($"Cell {x},{y}");
                    cellObj.transform.SetParent(transform, false);
                    cellObj.transform.localPosition = new Vector3(x * cellSize - offset.x, y * cellSize - offset.y, 0f);

                    var cell = cellObj.AddComponent<MapCell>();
                    cell.Initialize(manager, coords, tileSprite, animalSprite);
                    cells.Add(coords, cell);
                }
            }
        }

        private void BindSceneTiles(RanchManager manager, IReadOnlyList<SpriteRenderer> sceneTiles)
        {
            cells.Clear();

            if (TryBindWithExplicitCoords(manager, sceneTiles))
            {
                return;
            }

            Debug.LogWarning("[RanchMap] Scene tiles are missing complete SceneGridCellMarker data. Falling back to position-based tile ordering.");

            var orderedTiles = sceneTiles
                .OrderByDescending(tile => tile.transform.position.y)
                .ThenBy(tile => tile.transform.position.x)
                .Take(width * height)
                .ToList();

            for (var index = 0; index < orderedTiles.Count; index++)
            {
                var x = index % width;
                var rowFromTop = index / width;
                var y = height - 1 - rowFromTop;
                var coords = new Vector2Int(x, y);
                var tile = orderedTiles[index];
                var cell = tile.GetComponent<MapCell>() ?? tile.gameObject.AddComponent<MapCell>();

                tile.transform.SetParent(transform, true);
                cell.Initialize(manager, coords, tile.sprite != null ? tile.sprite : tileSprite, animalSprite, true);
                cells.Add(coords, cell);
            }
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
