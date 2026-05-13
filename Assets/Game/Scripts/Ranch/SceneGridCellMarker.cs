using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class SceneGridCellMarker : MonoBehaviour
    {
        [SerializeField] private Vector2Int gridCoords;

        public Vector2Int GridCoords => gridCoords;
    }
}
