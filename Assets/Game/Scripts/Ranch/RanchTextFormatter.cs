using System.Collections.Generic;
using NekogamiRanch.Abilities;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public static class RanchTextFormatter
    {
        public static string GetSelectedCellText(MapCell selectedCell)
        {
            if (selectedCell == null)
            {
                return "未选择地块";
            }

            if (selectedCell.Animal == null)
            {
                return $"地块 ({selectedCell.Coords.x},{selectedCell.Coords.y})：空";
            }

            var animal = selectedCell.Animal;
            return $"{animal.DisplayName}\n" +
                $"基础收益：{animal.BaseMoney:+#;-#;0}\n" +
                $"技能CD：{GetAnimalCooldownText(animal)}\n" +
                $"技能：{GetAnimalAbilityText(animal)}";
        }

        private static string GetAnimalCooldownText(Animal animal)
        {
            if (animal?.Ability is IAnimalCooldownStatus cooldownStatus && cooldownStatus.HasCooldown)
            {
                return cooldownStatus.RemainingCooldown.ToString();
            }

            return "无";
        }

        public static string GetAnimalAbilityText(Animal animal)
        {
            var ability = animal?.Data?.Ability;
            if (ability == null)
            {
                return "无";
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

            return !string.IsNullOrWhiteSpace(animal.Data.Description) ? animal.Data.Description : "无";
        }
    }
}
