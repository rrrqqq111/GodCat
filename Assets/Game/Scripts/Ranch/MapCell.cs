using NekogamiRanch.Animals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NekogamiRanch.Ranch
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class MapCell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer tileRenderer;
        [SerializeField] private SpriteRenderer animalRenderer;
        [SerializeField] private TextMesh label;

        private RanchManager manager;
        private Color normalColor;
        private Sprite defaultAnimalSprite;

        public Vector2Int Coords { get; private set; }
        public Animal Animal { get; private set; }
        public bool IsEmpty => Animal == null;

        public void Initialize(RanchManager ranchManager, Vector2Int coords, Sprite tileSprite, Sprite animalSprite, bool preserveTileSprite = false)
        {
            manager = ranchManager;
            Coords = coords;
            name = $"Cell {coords.x},{coords.y}";

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

            if (animalRenderer == null)
            {
                var animalObj = new GameObject("AnimalIcon");
                animalObj.transform.SetParent(transform, false);
                animalObj.transform.localPosition = new Vector3(0f, 0.14f, -0.1f);
                animalRenderer = animalObj.AddComponent<SpriteRenderer>();
            }

            animalRenderer.sprite = animalSprite;
            animalRenderer.sortingOrder = tileRenderer.sortingOrder + 2;
            animalRenderer.enabled = false;

            if (label == null)
            {
                var labelObj = new GameObject("Label");
                labelObj.transform.SetParent(transform, false);
                labelObj.transform.localPosition = new Vector3(0f, -0.36f, -0.2f);
                label = labelObj.AddComponent<TextMesh>();
                label.anchor = TextAnchor.MiddleCenter;
                label.alignment = TextAlignment.Center;
                label.characterSize = 0.12f;
                label.fontSize = 34;
                label.color = new Color(0.16f, 0.18f, 0.14f);
            }

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
            animalRenderer.enabled = Animal != null;
            animalRenderer.sprite = Animal?.Data.Icon != null ? Animal.Data.Icon : defaultAnimalSprite;
            FitAnimalToTile();
            label.text = Animal != null ? Animal.DisplayName : string.Empty;
        }

        private void FitAnimalToTile()
        {
            if (animalRenderer.sprite == null || tileRenderer.sprite == null)
            {
                return;
            }

            var tileSize = tileRenderer.sprite.bounds.size;
            var animalSize = animalRenderer.sprite.bounds.size;
            var maxAnimalSize = Mathf.Max(animalSize.x, animalSize.y);
            if (maxAnimalSize <= 0f)
            {
                return;
            }

            var targetSize = Mathf.Min(tileSize.x, tileSize.y) * 0.62f;
            var scale = Mathf.Min(1f, targetSize / maxAnimalSize);
            animalRenderer.transform.localScale = Vector3.one * scale;
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
