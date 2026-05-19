using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Toys
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Toy Data", fileName = "ToyData")]
    public class ToyData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string toyName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField, Range(0, 4)] private int rarity;
        [SerializeField] private bool enabledInDemo = true;

        [Header("Loadout")]
        [SerializeField] private ToySlotType slotType = ToySlotType.Normal;
        [SerializeField, Min(0)] private int equipCost;
        [SerializeField] private bool unique = true;
        [SerializeField] private bool canEquipBeforeRun = true;
        [SerializeField] private bool canUnequipDuringRun;

        [Header("Unlock")]
        [SerializeField] private ToyUnlockType unlockType = ToyUnlockType.Always;
        [SerializeField, Min(0)] private int unlockGameExp;
        [SerializeField] private string unlockFlag;

        [Header("Effect")]
        [SerializeField] private string effectScriptId;
        [SerializeField] private ToyTriggerType triggerType = ToyTriggerType.RunStart;
        [SerializeField] private int priority;
        [SerializeField] private ToyEffectParams effectParams = new ToyEffectParams();
        [SerializeField] private List<string> tags = new List<string>();

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
