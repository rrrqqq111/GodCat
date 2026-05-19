using System.Collections.Generic;
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
                $"年龄：{animal.AgeDays}\n" +
                $"技能：{GetAnimalAbilityText(animal)}";
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
