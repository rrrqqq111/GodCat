using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Items
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Item Data", fileName = "ItemData")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string itemName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField, Range(0, 4)] private int rarity;

        [Header("Classification")]
        [SerializeField] private ItemCategory category = ItemCategory.Relic;
        [SerializeField] private ItemTargetType targetType = ItemTargetType.Global;
        [SerializeField] private ItemUseTiming useTiming = ItemUseTiming.Passive;
        [SerializeField] private ItemTriggerType triggerType = ItemTriggerType.DayStart;
        [SerializeField] private ItemStackMode stackMode = ItemStackMode.Unique;

        [Header("Use Rules")]
        [SerializeField] private bool consumeOnUse;
        [SerializeField, Min(1)] private int maxStack = 1;
        [SerializeField] private bool canAppearInShop = true;
        [SerializeField] private bool canAppearInPacks = true;
        [SerializeField] private bool enabledInDemo = true;

        [Header("Effect")]
        [SerializeField] private string effectScriptId;
        [SerializeField] private int priority;
        [SerializeField] private ItemEffectParams effectParams = new ItemEffectParams();
        [SerializeField] private ItemTargetFilter targetFilter = new ItemTargetFilter();
        [SerializeField] private List<ItemData> subItems = new List<ItemData>();

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
        public string Name => string.IsNullOrWhiteSpace(itemName) ? name : itemName;
        public string DisplayName => Name;
        public string Description => description;
        public Sprite Icon => icon;
        public int Rarity => rarity;
        public ItemCategory Category => category;
        public ItemTargetType TargetType => targetType;
        public ItemUseTiming UseTiming => useTiming;
        public ItemTriggerType TriggerType => triggerType;
        public ItemStackMode StackMode => stackMode;
        public bool ConsumeOnUse => consumeOnUse;
        public int MaxStack => Mathf.Max(1, maxStack);
        public bool CanAppearInShop => canAppearInShop;
        public bool CanAppearInPacks => canAppearInPacks;
        public bool EnabledInDemo => enabledInDemo;
        public string EffectScriptId => string.IsNullOrWhiteSpace(effectScriptId) ? id : effectScriptId;
        public int Priority => priority;
        public ItemEffectParams EffectParams => effectParams;
        public ItemTargetFilter TargetFilter => targetFilter;
        public IReadOnlyList<ItemData> SubItems => subItems;
    }
}
