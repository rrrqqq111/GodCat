using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Ability Data", fileName = "AbilityData")]
    public class AbilityData : ScriptableObject
    {
        [SerializeField, InspectorName("能力ID")] private string id;
        [SerializeField, InspectorName("能力类型")] private string abilityType = "Normal";
        [SerializeField, InspectorName("执行优先级")] private int priority;
        [SerializeField, InspectorName("作用范围")] private string impactType = "None";
        [SerializeField, InspectorName("是否可叠加")] private bool stackable;
        [SerializeField, TextArea, InspectorName("能力描述")] private string desc;
        [SerializeField, InspectorName("触发时机")] private string triggerType = "None";
        [SerializeField, InspectorName("触发概率百分比")] private int triggerChancePercent = 100;
        [SerializeField, InspectorName("触发次数上限")] private int triggerLimit;
        [SerializeField, InspectorName("效果类型")] private string effectType = "None";
        [SerializeField, InspectorName("效果脚本ID")] private string effectScriptId;
        [SerializeField, InspectorName("效果参数")] private AbilityEffectParams effectParams = new AbilityEffectParams();
        [SerializeField, InspectorName("子能力列表")] private List<AbilityData> subAbilities = new List<AbilityData>();

        public string Id => id;
        public string AbilityType => abilityType;
        public int Priority => priority;
        public string ImpactType => impactType;
        public bool Stackable => stackable;
        public string Desc => desc;
        public string TriggerType => triggerType;
        public int TriggerChancePercent => triggerChancePercent;
        public int TriggerLimit => triggerLimit;
        public string EffectType => effectType;
        public string EffectScriptId => string.IsNullOrWhiteSpace(effectScriptId) ? id : effectScriptId;
        public AbilityEffectParams EffectParams => effectParams;
        public IReadOnlyList<AbilityData> SubAbilities => subAbilities;
    }
}
