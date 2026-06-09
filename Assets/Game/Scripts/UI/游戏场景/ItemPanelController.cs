using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Items;
using NekogamiRanch.Ranch;
using UnityEngine;
using UnityEngine.UI;

namespace NekogamiRanch.UI
{
    public class ItemPanelController : MonoBehaviour
    {
        [SerializeField] private RanchManager manager;
        [SerializeField] private List<Image> itemImages = new List<Image>(10);
        [SerializeField, Min(1)] private int slotCount = 10;
        [SerializeField] private Sprite defaultIcon;
        [SerializeField] private bool autoCollectChildImages = true;

        private readonly List<string> slotItemIds = new List<string>(10);
        private Sprite generatedDefaultIcon;
        private bool subscribed;

        private void Awake()
        {
            if (autoCollectChildImages && itemImages.Count == 0)
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
        }

        public void Refresh()
        {
            EnsureSlotIdCapacity();

            var itemIds = manager != null ? manager.CurrentItemIds : System.Array.Empty<string>();
            var fallback = GetDefaultIcon();

            for (var i = 0; i < slotCount; i++)
            {
                var itemId = i < itemIds.Count ? itemIds[i] : string.Empty;
                slotItemIds[i] = itemId;

                if (i >= itemImages.Count || itemImages[i] == null)
                {
                    continue;
                }

                var icon = !string.IsNullOrWhiteSpace(itemId) && manager != null ? manager.GetItemIconById(itemId) : null;
                itemImages[i].sprite = icon != null ? icon : fallback;
                itemImages[i].enabled = true;
            }
        }

        public string GetItemIdAtSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < slotItemIds.Count ? slotItemIds[slotIndex] : string.Empty;
        }

        public string GetItemDescriptionAtSlot(int slotIndex)
        {
            var itemId = GetItemIdAtSlot(slotIndex);
            return manager != null && !string.IsNullOrWhiteSpace(itemId)
                ? manager.GetItemDescriptionById(itemId)
                : string.Empty;
        }

        public bool TryGetItemAtSlot(int slotIndex, out ItemRuntimeState item)
        {
            item = null;
            var itemId = GetItemIdAtSlot(slotIndex);
            return manager != null && manager.TryGetItemById(itemId, out item);
        }

        private void CollectChildImages()
        {
            itemImages = GetComponentsInChildren<Image>(true)
                .Where(image => image.transform != transform)
                .Take(slotCount)
                .ToList();
        }

        private void EnsureSlotIdCapacity()
        {
            while (slotItemIds.Count < slotCount)
            {
                slotItemIds.Add(string.Empty);
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
            var fill = new Color(0.13f, 0.15f, 0.18f, 0.85f);
            var border = new Color(0.55f, 0.6f, 0.66f, 1f);
            var mark = new Color(0.78f, 0.82f, 0.88f, 1f);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var isBorder = x < 3 || x >= size - 3 || y < 3 || y >= size - 3;
                    var isMark = (x >= 29 && x <= 34 && y >= 17 && y <= 38) || (x >= 29 && x <= 34 && y >= 45 && y <= 50);
                    texture.SetPixel(x, y, isMark ? mark : isBorder ? border : fill);
                }
            }

            texture.Apply();
            texture.name = "Generated Default Item Icon";
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
