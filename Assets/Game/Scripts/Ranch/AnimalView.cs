using NekogamiRanch.Animals;
using NekogamiRanch.Effects;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class AnimalView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private Vector3 viewLocalOffset;
        [SerializeField] private Vector3 iconLocalPosition = new Vector3(0f, 0.14f, -0.1f);
        [SerializeField, Min(0.01f)] private float tileFill = 1.25f;
        [SerializeField] private bool fitIconToTile = true;

        private Vector3 iconBaseLocalScale = Vector3.one;
        private bool hasIconBaseLocalScale;
        private BobMotion bobMotion;

        public Animal Animal { get; private set; }

        public void Initialize()
        {
            transform.localPosition = viewLocalOffset;
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

            ApplyIconScale(tileSprite, animal?.Data.IconScale ?? 1f);
            bobMotion?.ResetBaseTransform();
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
            iconRenderer ??= GetComponentInChildren<SpriteRenderer>(true);
            bobMotion ??= GetComponentInChildren<BobMotion>(true);
            if (iconRenderer == null)
            {
                var iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(transform, false);
                iconRenderer = iconObj.AddComponent<SpriteRenderer>();
            }

            iconRenderer.transform.localPosition = iconLocalPosition;
            if (!hasIconBaseLocalScale)
            {
                iconBaseLocalScale = iconRenderer.transform.localScale;
                hasIconBaseLocalScale = true;
            }
        }

        private void ApplyIconScale(Sprite tileSprite, float animalIconScale)
        {
            var scale = fitIconToTile ? GetFitToTileScale(tileSprite) : 1f;
            iconRenderer.transform.localScale = iconBaseLocalScale * (scale * animalIconScale);
        }

        private float GetFitToTileScale(Sprite tileSprite)
        {
            if (iconRenderer.sprite == null || tileSprite == null)
            {
                return 1f;
            }

            var tileSize = tileSprite.bounds.size;
            var iconSize = iconRenderer.sprite.bounds.size;
            var maxIconSize = Mathf.Max(iconSize.x, iconSize.y);
            if (maxIconSize <= 0f)
            {
                return 1f;
            }

            var targetSize = Mathf.Min(tileSize.x, tileSize.y) * tileFill;
            return targetSize / maxIconSize;
        }
    }
}
