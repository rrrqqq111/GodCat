using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Toys
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Toy Data", fileName = "ToyData")]
    public class ToyData : ScriptableObject
    {
        [Header("基础信息")]
        [SerializeField, InspectorName("玩具ID")] private string id;
        [SerializeField, InspectorName("玩具名称")] private string toyName;
        [SerializeField, TextArea, InspectorName("玩具描述")] private string description;
        [SerializeField, InspectorName("玩具图标")] private Sprite icon;
        [SerializeField, Range(0, 4), InspectorName("稀有度")] private int rarity;
        [SerializeField, InspectorName("Demo中启用")] private bool enabledInDemo = true;

        [Header("装备")]
        [SerializeField, InspectorName("槽位类型")] private ToySlotType slotType = ToySlotType.Normal;
        [SerializeField, Min(0), InspectorName("装备消耗")] private int equipCost;
        [SerializeField, InspectorName("唯一装备")] private bool unique = true;
        [SerializeField, InspectorName("开局前可装备")] private bool canEquipBeforeRun = true;
        [SerializeField, InspectorName("局内可卸下")] private bool canUnequipDuringRun;

        [Header("解锁")]
        [SerializeField, InspectorName("解锁类型")] private ToyUnlockType unlockType = ToyUnlockType.Always;
        [SerializeField, Min(0), InspectorName("解锁所需游戏经验")] private int unlockGameExp;
        [SerializeField, InspectorName("解锁标记")] private string unlockFlag;

        [Header("效果")]
        [SerializeField, InspectorName("效果脚本ID")] private string effectScriptId;
        [SerializeField, InspectorName("触发时机")] private ToyTriggerType triggerType = ToyTriggerType.RunStart;
        [SerializeField, InspectorName("执行优先级")] private int priority;
        [SerializeField, InspectorName("效果参数")] private ToyEffectParams effectParams = new ToyEffectParams();
        [SerializeField, InspectorName("标签")] private List<string> tags = new List<string>();

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
        public string Name => string.IsNullOrWhiteSpace(toyName) ? name : toyName;
        public string DisplayName => Name;
        public string Description => description;
        public Sprite Icon => icon;
        public int Rarity => rarity;
        public bool EnabledInDemo => enabledInDemo;
        public ToySlotType SlotType => slotType;
        public int EquipCost => Mathf.Max(0, equipCost);
        public bool Unique => unique;
        public bool CanEquipBeforeRun => canEquipBeforeRun;
        public bool CanUnequipDuringRun => canUnequipDuringRun;
        public ToyUnlockType UnlockType => unlockType;
        public int UnlockGameExp => Mathf.Max(0, unlockGameExp);
        public string UnlockFlag => unlockFlag;
        public string EffectScriptId => effectScriptId;
        public ToyTriggerType TriggerType => triggerType;
        public int Priority => priority;
        public ToyEffectParams EffectParams => effectParams;
        public IReadOnlyList<string> Tags => tags;
    }
}
