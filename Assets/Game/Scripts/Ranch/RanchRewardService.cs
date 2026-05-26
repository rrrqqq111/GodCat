using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Items;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchRewardService
    {
        private readonly RanchItemService itemService;
        private readonly IReadOnlyList<ItemData> itemRewardPool;

        public RanchRewardService(RanchItemService itemService, IReadOnlyList<ItemData> itemRewardPool)
        {
            this.itemService = itemService;
            this.itemRewardPool = itemRewardPool ?? Array.Empty<ItemData>();
        }

        public bool TryGrantRandomItem(out ItemData grantedItem)
        {
            grantedItem = null;
            if (itemService == null)
            {
                return false;
            }

            var candidates = itemRewardPool
                .Where(item => item != null && item.EnabledInDemo && item.CanAppearInPacks)
                .ToList();
            if (candidates.Count == 0)
            {
                return false;
            }

            grantedItem = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            return itemService.AddItem(grantedItem);
        }
    }
}
