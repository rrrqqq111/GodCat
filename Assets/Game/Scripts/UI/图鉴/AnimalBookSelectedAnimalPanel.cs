using System.Collections.Generic;
using NekogamiRanch.Animals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NekogamiRanch.UI
{
    public class AnimalBookSelectedAnimalPanel : MonoBehaviour
    {
        [SerializeField, InspectorName("动物名称文本")] private TMP_Text animalNameText;
        [SerializeField, InspectorName("家族图标图像")] private Image familyIconImage;
        [SerializeField, InspectorName("基础属性文本")] private TMP_Text baseMoneyText;
        [SerializeField, InspectorName("能力描述文本")] private TMP_Text abilityText;
        [SerializeField, InspectorName("动物图标图像")] private Image animalIconImage;
        [SerializeField, InspectorName("稀有度变色图像")] private List<Image> rarityColorImages = new List<Image>();
        [SerializeField, InspectorName("稀有度颜色表")] private List<AnimalBookRarityColor> rarityColors = new List<AnimalBookRarityColor>();
        [SerializeField, InspectorName("默认稀有度颜色")] private Color fallbackRarityColor = Color.white;

        public void Refresh(AnimalData data)
        {
            if (data == null)
            {
                return;
            }

            if (animalNameText != null)
            {
                animalNameText.text = data.DisplayName;
            }

            if (baseMoneyText != null)
            {
                baseMoneyText.text = data.BaseMoney.ToString("+#;-#;0");
            }

            if (abilityText != null)
            {
                abilityText.text = FormatAbilityText(data);
            }

            ApplyImage(familyIconImage, data.FamilyIcon);
            ApplyImage(animalIconImage, data.Icon);
            ApplyRarityColors(data.Rarity);
        }

        private static void ApplyImage(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
            image.preserveAspect = true;
        }

        private void ApplyRarityColors(int rarity)
        {
            for (var i = 0; i < rarityColorImages.Count; i++)
            {
                AnimalBookRarityColorUtility.Apply(rarityColorImages[i], rarity, rarityColors, fallbackRarityColor);
            }
        }

        private static string FormatAbilityText(AnimalData data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            var ability = data.Ability;
            if (ability == null)
            {
                return string.IsNullOrWhiteSpace(data.Description) ? "\u65e0\u80fd\u529b" : data.Description;
            }

            var descriptions = new List<string>();
            if (!string.IsNullOrWhiteSpace(ability.Desc))
            {
                descriptions.Add(ability.Desc);
            }

            if (ability.SubAbilities != null)
            {
                foreach (var subAbility in ability.SubAbilities)
                {
                    if (subAbility != null && !string.IsNullOrWhiteSpace(subAbility.Desc))
                    {
                        descriptions.Add(subAbility.Desc);
                    }
                }
            }

            if (descriptions.Count > 0)
            {
                return string.Join("\n", descriptions);
            }

            return string.IsNullOrWhiteSpace(data.Description) ? "\u65e0\u80fd\u529b" : data.Description;
        }
    }
}
