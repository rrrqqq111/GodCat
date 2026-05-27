using NekogamiRanch.Items;
using NekogamiRanch.Toys;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchInventoryCommandService
    {
        private readonly RanchItemService itemService;
        private readonly RanchRewardService rewardService;
        private readonly RanchToyService toyService;
        private readonly System.Action stateChanged;

        public RanchInventoryCommandService(
            RanchItemService itemService,
            RanchRewardService rewardService,
            RanchToyService toyService,
            System.Action stateChanged)
        {
            this.itemService = itemService;
            this.rewardService = rewardService;
            this.toyService = toyService;
            this.stateChanged = stateChanged;
        }

        public bool AddItem(ItemData itemData, int count = 1)
        {
            var added = itemService != null && itemService.AddItem(itemData, count);
            if (added)
            {
                stateChanged?.Invoke();
            }

            return added;
        }

        public bool TryAddRandomItem()
        {
            if (rewardService == null || !rewardService.TryGrantRandomItem(out _))
            {
                return false;
            }

            stateChanged?.Invoke();
            return true;
        }

        public bool TryGetItemById(string itemId, out ItemRuntimeState item)
        {
            item = null;
            return itemService != null && itemService.TryGetItemById(itemId, out item);
        }

        public Sprite GetItemIconById(string itemId)
        {
            return TryGetItemById(itemId, out var item) && item.Data != null ? item.Data.Icon : null;
        }

        public string GetItemDescriptionById(string itemId)
        {
            return TryGetItemById(itemId, out var item) && item.Data != null ? item.Data.Description : string.Empty;
        }

        public bool TryGetToyById(string toyId, out ToyData toy)
        {
            toy = null;
            return toyService != null && toyService.TryGetToyById(toyId, out toy);
        }

        public Sprite GetToyIconById(string toyId)
        {
            return TryGetToyById(toyId, out var toy) ? toy.Icon : null;
        }

        public string GetToyDescriptionById(string toyId)
        {
            return TryGetToyById(toyId, out var toy) ? toy.Description : string.Empty;
        }

        public bool RegisterToy(ToyData toyData)
        {
            var registered = toyService != null && toyService.Register(toyData);
            if (registered)
            {
                stateChanged?.Invoke();
            }

            return registered;
        }
    }
}
