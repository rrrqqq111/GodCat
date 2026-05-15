using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class AnimalView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private Vector3 iconLocalPosition = new Vector3(0f, 0.14f, -0.1f);
        [SerializeField, Range(0.1f, 1f)] private float tileFill = 0.62f;

        public Animal Animal { get; private set; }

        public void Initialize()
        {
            EnsureIconRenderer();
        }

        public void Refresh(Animal animal, Sprite fallbackSprite, Sprite tileSprite, int sortingOrder)
        {
            Initialize();

            Animal = animal;
            var hasAnimal = animal != null;
            iconRenderer.enabled = hasAnimal;
            iconRenderer.sprite = animal?.Data.Icon != null ? animal.Data.Icon : fallbackSprite;
            iconRenderer.sortingOrder = sortingOrder;

            FitToTile(tileSprite);
        }

        public void PlayAbilityFeedback()
        {
            // Reserved for later animation hooks.
        }

        public void PlayMoveFeedback()
        {
            // Reserved for later animation hooks.
        }

        private void EnsureIconRenderer()
        {
            if (iconRenderer != null)
            {
                return;
            }

            iconRenderer = GetComponentInChildren<SpriteRenderer>(true);
            if (iconRenderer == null)
            {
                var iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(transform, false);
                iconRenderer = iconObj.AddComponent<SpriteRenderer>();
            }

            iconRenderer.transform.localPosition = iconLocalPosition;
        }

        private void FitToTile(Sprite tileSprite)
        {
            if (iconRenderer.sprite == null || tileSprite == null)
            {
                return;
            }

            var tileSize = tileSprite.bounds.size;
            var iconSize = iconRenderer.sprite.bounds.size;
            var maxIconSize = Mathf.Max(iconSize.x, iconSize.y);
            if (maxIconSize <= 0f)
            {
                return;
            }

            var targetSize = Mathf.Min(tileSize.x, tileSize.y) * tileFill;
            var scale = Mathf.Min(1f, targetSize / maxIconSize);
            iconRenderer.transform.localScale = Vector3.one * scale;
        }
    }
}
