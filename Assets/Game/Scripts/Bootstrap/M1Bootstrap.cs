using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using NekogamiRanch.UI;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NekogamiRanch.Bootstrap
{
    public static class M1Bootstrap
    {
        private const string Tile1Guid = "6fad8cd33bab4d646bb9b50219084a5d";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureM1Scene()
        {
            EnsureCamera();
            EnsureEventSystem();

            var manager = Object.FindObjectOfType<RanchManager>();
            var root = manager != null ? manager.gameObject : new GameObject("M1 Ranch Demo");

            var map = Object.FindObjectOfType<RanchMap>();
            if (map == null)
            {
                map = new GameObject("RanchMap").AddComponent<RanchMap>();
                map.transform.SetParent(root.transform, false);
                map.transform.position = new Vector3(0f, -0.45f, 0f);
            }

            var uiController = Object.FindObjectOfType<RanchUIController>();
            if (uiController == null)
            {
                var hud = Object.FindObjectOfType<RanchHUD>();
                if (hud != null)
                {
                    uiController = hud.GetComponentInParent<RanchUIController>();
                    if (uiController == null)
                    {
                        uiController = hud.gameObject.AddComponent<RanchUIController>();
                    }
                }
            }

            if (manager == null)
            {
                manager = root.AddComponent<RanchManager>();
            }

            var sceneTiles = FindSceneTileRenderers();
            var tileSprite = LoadSpriteByGuid(Tile1Guid) ?? CreateSquareSprite("M1 Tile Sprite", Color.white, 64);
            var animalSprite = CreateCircleSprite("M1 Animal Sprite", new Color(0.95f, 0.69f, 0.35f), 64);
            manager.Initialize(map, CreateStartingAnimals(), tileSprite, animalSprite, sceneTiles);
            if (uiController != null)
            {
                uiController.Initialize(manager);
            }
            else
            {
                Debug.LogWarning("[M1Bootstrap] RanchUIController is missing. Game logic will run, but no UI will be updated.");
            }
        }

        private static IReadOnlyList<AnimalData> CreateStartingAnimals()
        {
            return new[]
            {
                LoadAnimalDataAsset("Assets/Game/Data/Animals/Hoofed/pig.asset"),
                LoadAnimalDataAsset("Assets/Game/Data/Animals/Hoofed/boar.asset"),
                LoadAnimalDataAsset("Assets/Game/Data/Animals/Hoofed/horse.asset")
            }.Where(animal => animal != null).ToArray();
        }

        private static AnimalData LoadAnimalDataAsset(string assetPath)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<AnimalData>(assetPath);
#else
            return null;
#endif
        }

        private static IReadOnlyList<SpriteRenderer> FindSceneTileRenderers()
        {
            return Object.FindObjectsOfType<SpriteRenderer>()
                .Where(renderer => renderer.sprite != null)
                .Where(IsLikelyTileRenderer)
                .OrderByDescending(renderer => renderer.transform.position.y)
                .ThenBy(renderer => renderer.transform.position.x)
                .ToList();
        }

        private static bool IsLikelyTileRenderer(SpriteRenderer renderer)
        {
            var size = renderer.bounds.size;
            return size.x > 0.2f && size.y > 0.2f && size.x <= 2.5f && size.y <= 2.5f;
        }

        private static Sprite LoadSpriteByGuid(string guid)
        {
#if UNITY_EDITOR
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
#else
            return null;
#endif
        }

        private static void EnsureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObj = new GameObject("Main Camera");
                cameraObj.tag = "MainCamera";
                camera = cameraObj.AddComponent<Camera>();
            }

            camera.orthographic = true;
            camera.orthographicSize = 4.4f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.82f, 0.92f, 0.96f);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static Sprite CreateSquareSprite(string spriteName, Color color, int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.name = spriteName;
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
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
