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
        [SerializeField] private AbilityData ability;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;

        public string Id => id;
        public string Name => string.IsNullOrWhiteSpace(animalName) ? name : animalName;
        public string Family => family;
        public int Rarity => rarity;
        public int BaseMoney => baseMoney;
        public AbilityData Ability => ability;
        public string Description => description;
        public Sprite Icon => icon;

        public string AnimalId => Id;
        public string DisplayName => Name;
        public string AbilityDescription => Description;

        public void Initialize(string animalId, string displayName, string animalFamily, int animalRarity, int money, AbilityData animalAbility, string animalDescription, Sprite animalIcon = null)
        {
            id = animalId;
            animalName = displayName;
            family = animalFamily;
            rarity = Mathf.Clamp(animalRarity, 0, 4);
            baseMoney = money;
            ability = animalAbility;
            description = animalDescription;
            icon = animalIcon;
        }
    }
}
