using UnityEngine;
using NekogamiRanch.Abilities;

namespace NekogamiRanch.Animals
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Animal Data", fileName = "AnimalData")]
    public class AnimalData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string animalName;
        [SerializeField] private string family = "Hoofed";
        [SerializeField, Range(0, 4)] private int rarity;
        [SerializeField] private int baseMoney = 1;
        [SerializeField, Min(0)] private int evolutionThreshold;
        [SerializeField] private AnimalData evolutionTarget;
        [SerializeField, Min(0)] private int evolutionTargetLevel;
        [SerializeField] private AbilityData ability;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private Sprite familyIcon;
        [SerializeField, Min(0.01f)] private float iconScale = 1f;

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
