using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Items
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Item Data", fileName = "ItemData")]
    public class ItemData : ScriptableObject
    {
        [Header("基础信息")]
        [SerializeField, InspectorName("道具ID")] private string id;
        [SerializeField, InspectorName("道具名称")] private string itemName;
        [SerializeField, TextArea, InspectorName("道具描述")] private string description;
        [SerializeField, InspectorName("道具图标")] private Sprite icon;
        [SerializeField, Range(0, 4), InspectorName("稀有度")] private int rarity;

        [Header("分类")]
        [SerializeField, InspectorName("道具分类")] private ItemCategory category = ItemCategory.Relic;
        [SerializeField, InspectorName("目标类型")] private ItemTargetType targetType = ItemTargetType.Global;
        [SerializeField, InspectorName("使用方式")] private ItemUseTiming useTiming = ItemUseTiming.Passive;
        [SerializeField, InspectorName("触发时机")] private ItemTriggerType triggerType = ItemTriggerType.DayStart;
        [SerializeField, InspectorName("堆叠方式")] private ItemStackMode stackMode = ItemStackMode.Unique;

        [Header("使用规则")]
        [SerializeField, InspectorName("使用后消耗")] private bool consumeOnUse;
        [SerializeField, Min(1), InspectorName("最大堆叠数量")] private int maxStack = 1;
        [SerializeField, InspectorName("可出现在商店")] private bool canAppearInShop = true;
        [SerializeField, InspectorName("可出现在补充包")] private bool canAppearInPacks = true;
        [SerializeField, InspectorName("Demo中启用")] private bool enabledInDemo = true;

        [Header("效果")]
        [SerializeField, InspectorName("效果脚本ID")] private string effectScriptId;
        [SerializeField, InspectorName("执行优先级")] private int priority;
        [SerializeField, InspectorName("效果参数")] private ItemEffectParams effectParams = new ItemEffectParams();
        [SerializeField, InspectorName("目标筛选")] private ItemTargetFilter targetFilter = new ItemTargetFilter();
        [SerializeField, InspectorName("子道具列表")] private List<ItemData> subItems = new List<ItemData>();

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
