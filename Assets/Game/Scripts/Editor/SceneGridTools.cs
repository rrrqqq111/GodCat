using System.Linq;
using NekogamiRanch.Ranch;
using UnityEditor;
using UnityEngine;

namespace NekogamiRanch.EditorTools
{
    public static class SceneGridTools
    {
        [MenuItem("Tools/Game/Assign Scene Grid Coords From Position")]
        public static void AssignSceneGridCoordsFromPosition()
        {
            var root = Selection.activeTransform;
            if (root == null)
            {
                Debug.LogError("[SceneGridTools] Select the tile root object first.");
                return;
            }

            var tiles = root.GetComponentsInChildren<SpriteRenderer>(true)
                .OrderByDescending(tile => tile.transform.position.y)
                .ThenBy(tile => tile.transform.position.x)
                .ToList();

            if (tiles.Count == 0)
            {
                Debug.LogError("[SceneGridTools] No SpriteRenderer found under the selected root.");
                return;
            }

            var mapWidth = 4;
            var mapHeight = 5;
            var ranchMap = root.GetComponentInParent<RanchMap>();
            if (ranchMap != null)
            {
                var mapSerialized = new SerializedObject(ranchMap);
                mapWidth = mapSerialized.FindProperty("width").intValue;
                mapHeight = mapSerialized.FindProperty("height").intValue;
            }

            var expectedCount = mapWidth * mapHeight;
            if (tiles.Count < expectedCount)
            {
                Debug.LogError($"[SceneGridTools] Need at least {expectedCount} tiles but only found {tiles.Count}.");
                return;
            }

            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();

            for (var index = 0; index < expectedCount; index++)
            {
                var x = index % mapWidth;
                var y = index / mapWidth;
                var coords = new Vector2Int(x, y);
                var tile = tiles[index];
                var marker = tile.GetComponent<SceneGridCellMarker>();
                if (marker == null)
                {
                    marker = Undo.AddComponent<SceneGridCellMarker>(tile.gameObject);
                }

                Undo.RecordObject(marker, "Assign Scene Grid Coords");
                var serialized = new SerializedObject(marker);
                serialized.FindProperty("gridCoords").vector2IntValue = coords;
                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(marker);
            }

            Undo.CollapseUndoOperations(group);
            Debug.Log($"[SceneGridTools] Assigned grid coords to {expectedCount} tiles under '{root.name}' ({mapWidth}x{mapHeight}).");
        }
    }
}
