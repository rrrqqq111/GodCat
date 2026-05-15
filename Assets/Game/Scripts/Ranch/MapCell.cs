using NekogamiRanch.Animals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NekogamiRanch.Ranch
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class MapCell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer tileRenderer;
        [SerializeField] private AnimalView animalView;

        private RanchManager manager;
        private Color normalColor;
        private Sprite defaultAnimalSprite;
        private AnimalView animalViewPrefab;
        private bool missingAnimalViewWarningLogged;

        public Vector2Int Coords { get; private set; }
        public Animal Animal { get; private set; }
        public bool IsEmpty => Animal == null;

        public void Initialize(RanchManager ranchManager, Vector2Int coords, Sprite tileSprite, Sprite animalSprite, bool preserveTileSprite = false, AnimalView viewPrefab = null)
        {
            manager = ranchManager;
            Coords = coords;
            name = $"Cell {coords.x},{coords.y}";
            animalViewPrefab = viewPrefab;

            tileRenderer ??= GetComponent<SpriteRenderer>();
            tileRenderer ??= gameObject.AddComponent<SpriteRenderer>();
            if (!preserveTileSprite)
            {
                tileRenderer.sprite = tileSprite;
                tileRenderer.sortingOrder = 0;
                tileRenderer.color = Color.white;
            }

            normalColor = tileRenderer.color;
            defaultAnimalSprite = animalSprite;

            var collider2d = GetComponent<BoxCollider2D>();
            collider2d.size = tileRenderer.sprite != null ? (Vector2)tileRenderer.sprite.bounds.size : Vector2.one * 0.95f;

            RefreshView();
        }

        public bool TryPlaceAnimal(Animal animal)
        {
            if (!IsEmpty || animal == null)
            {
                return false;
            }

            Animal = animal;
            Animal.SetCoords(Coords);
            RefreshView();
            return true;
        }

        public Animal RemoveAnimal()
        {
            var animal = Animal;
            Animal = null;
            RefreshView();
            return animal;
        }

        public void SetSelected(bool selected)
        {
            tileRenderer.color = selected ? new Color(1f, 0.92f, 0.48f) : normalColor;
        }

        private void RefreshView()
        {
            if (Animal == null && animalView == null)
            {
                return;
            }

            if (!EnsureAnimalView())
            {
                return;
            }

            animalView.Refresh(Animal, defaultAnimalSprite, tileRenderer.sprite, tileRenderer.sortingOrder + 2);
        }

        private bool EnsureAnimalView()
        {
            if (animalView != null)
            {
                return true;
            }

            if (animalViewPrefab == null)
            {
                if (!missingAnimalViewWarningLogged)
                {
                    Debug.LogWarning($"[MapCell] AnimalView prefab is missing for {name}. Assign it on RanchManager.");
                    missingAnimalViewWarningLogged = true;
                }

                return false;
            }

            animalView = Instantiate(animalViewPrefab, transform, false);
            animalView.Initialize();
            return true;
        }

        private void OnMouseDown()
        {
            if (IsPointerOverUi())
            {
                return;
            }

            manager?.SelectCell(this);
        }

        private static bool IsPointerOverUi()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            for (var i = 0; i < Input.touchCount; i++)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
