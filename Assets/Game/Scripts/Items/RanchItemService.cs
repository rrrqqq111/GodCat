using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.Items
{
    public class RanchItemService
    {
        private readonly RanchManager manager;
        private readonly RanchEconomyService economy;
        private readonly List<ItemRuntimeState> items = new List<ItemRuntimeState>();

        public RanchItemService(RanchManager manager, RanchEconomyService economy, IReadOnlyList<ItemData> startingItems)
        {
            this.manager = manager;
            this.economy = economy;
            AddItems(startingItems);
        }

        public IReadOnlyList<ItemRuntimeState> Items => items;

        public IReadOnlyList<string> ItemIds => items
            .Where(item => item?.Data != null)
            .Select(item => item.Data.Id)
            .ToList();

        public bool TryGetItemById(string itemId, out ItemRuntimeState item)
        {
            item = null;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            item = items.FirstOrDefault(candidate => candidate?.Data != null && candidate.Data.Id == itemId);
            return item != null;
        }

        public void AddItems(IReadOnlyList<ItemData> itemData)
        {
            if (itemData == null)
            {
                return;
            }

            foreach (var data in itemData)
            {
                AddItem(data);
            }
        }

        public bool AddItem(ItemData data, int count = 1)
        {
            if (data == null || !data.EnabledInDemo)
            {
                return false;
            }

            var existing = items.FirstOrDefault(item => item.Data == data);
            if (existing != null)
            {
                existing.AddCount(Mathf.Max(1, count));
                return true;
            }

            items.Add(new ItemRuntimeState(data, Mathf.Max(1, count)));
            return true;
        }

        public void Trigger(ItemTriggerType triggerType, int day)
        {
            var context = new ItemUseContext(manager, economy, day, triggerType);
            foreach (var item in items
                .Where(item => item?.Data != null && item.Data.TriggerType == triggerType)
                .OrderBy(item => item.Data.Priority))
            {
                if (!ItemEffectRegistry.TryCreate(item.Data.EffectScriptId, out var effect))
                {
                    Debug.LogWarning($"[RanchItemService] Missing item effect: {item.Data.EffectScriptId}");
                    continue;
                }

                var result = effect.TryExecute(item, context);
                if (result.Success && !string.IsNullOrWhiteSpace(result.Message))
                {
                    Debug.Log($"[RanchItemService] {result.Message}");
                }
            }
        }
    }
}
