using UnityEngine;
using NekogamiRanch.Abilities;

namespace NekogamiRanch.Animals
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Animal Data", fileName = "AnimalData")]
    public class AnimalData : ScriptableObject
    {
        [SerializeField, InspectorName("动物ID")] private string id;
        [SerializeField, InspectorName("动物名称")] private string animalName;
        [SerializeField, InspectorName("动物家族")] private string family = "Hoofed";
        [SerializeField, Range(0, 4), InspectorName("稀有度")] private int rarity;
        [SerializeField, InspectorName("基础金币")] private int baseMoney = 1;
        [SerializeField, Min(0), InspectorName("进化所需次数")] private int evolutionThreshold;
        [SerializeField, InspectorName("进化目标动物")] private AnimalData evolutionTarget;
        [SerializeField, Min(0), InspectorName("进化目标等级")] private int evolutionTargetLevel;
        [SerializeField, InspectorName("动物能力")] private AbilityData ability;
        [SerializeField, InspectorName("默认能力音效")] private AudioClip abilitySound;
        [SerializeField, TextArea, InspectorName("描述")] private string description;
        [SerializeField, InspectorName("动物图标")] private Sprite icon;
        [SerializeField, InspectorName("家族图标")] private Sprite familyIcon;
        [SerializeField, Min(0.01f), InspectorName("图标缩放")] private float iconScale = 1f;

        public string Id => id;
        public string Name => string.IsNullOrWhiteSpace(animalName) ? name : animalName;
        public string Family => family;
        public int Rarity => rarity;
        public int BaseMoney => baseMoney;
        public int EvolutionThreshold => Mathf.Max(0, evolutionThreshold);
        public bool HasEvolution => EvolutionThreshold > 0;
        public AnimalData EvolutionTarget => evolutionTarget;
        public int EvolutionTargetLevel => Mathf.Max(0, evolutionTargetLevel);
        public AbilityData Ability => ability;
        public AudioClip AbilitySound => abilitySound;
        public string Description => description;
        public Sprite Icon => icon;
        public Sprite FamilyIcon => familyIcon;
        public float IconScale => iconScale;

        public string AnimalId => Id;
        public string DisplayName => Name;
        public string AbilityDescription => ability != null && !string.IsNullOrWhiteSpace(ability.Desc) ? ability.Desc : description;

        public void Initialize(string animalId, string displayName, string animalFamily, int animalRarity, int money, AbilityData animalAbility, string animalDescription, Sprite animalIcon = null, Sprite animalFamilyIcon = null, float animalIconScale = 1f, int animalEvolutionThreshold = 0)
        {
            id = animalId;
            animalName = displayName;
            family = animalFamily;
            rarity = Mathf.Clamp(animalRarity, 0, 4);
            baseMoney = money;
            evolutionThreshold = Mathf.Max(0, animalEvolutionThreshold);
            ability = animalAbility;
            description = animalDescription;
            icon = animalIcon;
            familyIcon = animalFamilyIcon;
            iconScale = Mathf.Max(0.01f, animalIconScale);
        }
    }
}
