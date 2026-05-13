using System.Collections.Generic;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Ability Data", fileName = "AbilityData")]
    public class AbilityData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string abilityType = "Normal";
        [SerializeField] private int priority;
        [SerializeField] private string impactType = "None";
        [SerializeField] private bool stackable;
        [SerializeField, TextArea] private string desc;
        [SerializeField] private string triggerType = "None";
        [SerializeField] private int triggerChancePercent = 100;
        [SerializeField] private int triggerLimit;
        [SerializeField] private string effectType = "None";
        [SerializeField] private string effectScriptId;
        [SerializeField] private AbilityEffectParams effectParams = new AbilityEffectParams();
        [SerializeField] private List<AbilityData> subAbilities = new List<AbilityData>();

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
