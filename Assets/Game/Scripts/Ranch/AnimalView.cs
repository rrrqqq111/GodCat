using System.Collections;
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
        [SerializeField, Min(0f)] private float defaultJumpHeight = 0.28f;
        [SerializeField, Min(0.01f)] private float defaultJumpDuration = 0.22f;

        private Vector3 iconBaseLocalScale = Vector3.one;
        private bool hasIconBaseLocalScale;
        private BobMotion bobMotion;

        public Animal Animal { get; private set; }
        public Vector3 TargetWorldPosition => transform.parent != null
            ? transform.parent.TransformPoint(viewLocalOffset)
            : transform.position;

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
            StartCoroutine(PlayAbilityJump(defaultJumpHeight, defaultJumpDuration));
        }

        public void PlayMoveFeedback()
        {
            StartCoroutine(PlayAbilityJump(defaultJumpHeight * 0.5f, defaultJumpDuration));
        }

        public void SetVisible(bool visible)
        {
            EnsureIconRenderer();
            if (iconRenderer != null)
            {
                iconRenderer.enabled = visible && Animal != null;
            }
        }

        public IEnumerator PlayEnterFrom(Vector3 worldStart, Vector3 worldEnd, float duration)
        {
            EnsureIconRenderer();
            SetVisible(true);

            transform.position = worldStart;
            if (duration <= 0f)
            {
                transform.position = worldEnd;
                ResetBobMotion();
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                transform.position = Vector3.Lerp(worldStart, worldEnd, t);
                yield return null;
            }

            transform.position = worldEnd;
            ResetBobMotion();
        }

        public IEnumerator PlayEnterFrom(Vector3 worldStart, float duration)
        {
            yield return PlayEnterFrom(worldStart, TargetWorldPosition, duration);
        }

        public IEnumerator PlayAbilityJump(float height, float duration)
        {
            EnsureIconRenderer();
            var start = transform.localPosition;
            if (duration <= 0f || height <= 0f)
            {
                transform.localPosition = start;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var yOffset = Mathf.Sin(t * Mathf.PI) * height;
                transform.localPosition = start + Vector3.up * yOffset;
                yield return null;
            }

            transform.localPosition = start;
            ResetBobMotion();
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

        private void ResetBobMotion()
        {
            bobMotion?.ResetBaseTransform();
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
