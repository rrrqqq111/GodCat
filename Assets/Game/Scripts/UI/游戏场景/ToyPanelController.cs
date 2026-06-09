using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Ranch;
using NekogamiRanch.Toys;
using UnityEngine;
using UnityEngine.UI;

namespace NekogamiRanch.UI
{
    public class ToyPanelController : MonoBehaviour
    {
        [SerializeField] private RanchManager manager;
        [SerializeField] private List<Image> toyImages = new List<Image>(10);
        [SerializeField, Min(1)] private int slotCount = 10;
        [SerializeField] private Sprite defaultIcon;
        [SerializeField] private bool autoCollectChildImages = true;

        private readonly List<string> slotToyIds = new List<string>(10);
        private Sprite generatedDefaultIcon;
        private bool subscribed;

        private void Awake()
        {
            if (autoCollectChildImages && toyImages.Count == 0)
            {
                CollectChildImages();
            }
        }

        private void Start()
        {
            if (manager == null)
            {
                manager = FindObjectOfType<RanchManager>();
            }

            Subscribe();
            Refresh();
            StartCoroutine(RefreshNextFrame());
        }

        private void OnEnable()
        {
            if (manager != null)
            {
                Subscribe();
                Refresh();
            }
        }

        public void Refresh()
        {
            EnsureSlotIdCapacity();

            var toyIds = manager != null ? manager.CurrentToyIds : System.Array.Empty<string>();
            var fallback = GetDefaultIcon();

            for (var i = 0; i < slotCount; i++)
            {
                var toyId = i < toyIds.Count ? toyIds[i] : string.Empty;
                slotToyIds[i] = toyId;

                if (i >= toyImages.Count || toyImages[i] == null)
                {
                    continue;
                }

                var icon = !string.IsNullOrWhiteSpace(toyId) && manager != null ? manager.GetToyIconById(toyId) : null;
                toyImages[i].sprite = icon != null ? icon : fallback;
                toyImages[i].enabled = true;
            }
        }

        public string GetToyIdAtSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < slotToyIds.Count ? slotToyIds[slotIndex] : string.Empty;
        }

        public string GetToyDescriptionAtSlot(int slotIndex)
        {
            var toyId = GetToyIdAtSlot(slotIndex);
            return manager != null && !string.IsNullOrWhiteSpace(toyId)
                ? manager.GetToyDescriptionById(toyId)
                : string.Empty;
        }

        public bool TryGetToyAtSlot(int slotIndex, out ToyData toy)
        {
            toy = null;
            var toyId = GetToyIdAtSlot(slotIndex);
            return manager != null && manager.TryGetToyById(toyId, out toy);
        }

        private System.Collections.IEnumerator RefreshNextFrame()
        {
            yield return null;
            Refresh();
        }

        private void CollectChildImages()
        {
            toyImages = GetComponentsInChildren<Image>(true)
                .Where(image => image.transform != transform)
                .Take(slotCount)
                .ToList();
        }

        private void EnsureSlotIdCapacity()
        {
            while (slotToyIds.Count < slotCount)
            {
                slotToyIds.Add(string.Empty);
            }
        }

        private Sprite GetDefaultIcon()
        {
            if (defaultIcon != null)
            {
                return defaultIcon;
            }

            return generatedDefaultIcon ?? (generatedDefaultIcon = CreateGeneratedDefaultIcon());
        }

        private static Sprite CreateGeneratedDefaultIcon()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var fill = new Color(0.16f, 0.14f, 0.18f, 0.85f);
            var border = new Color(0.62f, 0.55f, 0.72f, 1f);
            var mark = new Color(0.84f, 0.78f, 0.92f, 1f);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var isBorder = x < 3 || x >= size - 3 || y < 3 || y >= size - 3;
                    var isMark = (x >= 18 && x <= 45 && y >= 29 && y <= 34) || (x >= 29 && x <= 34 && y >= 18 && y <= 45);
                    texture.SetPixel(x, y, isMark ? mark : isBorder ? border : fill);
                }
            }

            texture.Apply();
            texture.name = "Generated Default Toy Icon";
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private void Subscribe()
        {
            if (manager == null || subscribed)
            {
                return;
            }

            manager.StateChanged += Refresh;
            subscribed = true;
        }

        private void OnDestroy()
        {
            if (manager != null && subscribed)
            {
                manager.StateChanged -= Refresh;
            }
        }
    }
}
