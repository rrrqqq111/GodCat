using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public static class RanchSceneBinder
    {
        public static RanchMap ResolveMap(RanchMap assignedMap)
        {
            return assignedMap != null ? assignedMap : Object.FindObjectOfType<RanchMap>();
        }

        public static IReadOnlyList<SpriteRenderer> FindSceneTileRenderers()
        {
            return Object.FindObjectsOfType<SceneGridCellMarker>()
                .Select(marker => marker.GetComponent<SpriteRenderer>())
                .Where(renderer => renderer != null)
                .OrderByDescending(renderer => renderer.transform.position.y)
                .ThenBy(renderer => renderer.transform.position.x)
                .ToList();
        }

        public static Sprite GetFallbackAnimalSprite(Sprite assignedSprite)
        {
            return assignedSprite != null
                ? assignedSprite
                : CreateCircleSprite("Fallback Animal Sprite", new Color(0.95f, 0.69f, 0.35f), 64);
        }

        private static Sprite CreateCircleSprite(string spriteName, Color color, int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var radius = size * 0.42f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= radius ? color : Color.clear);
                }
            }

            texture.Apply();
            texture.name = spriteName;
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
