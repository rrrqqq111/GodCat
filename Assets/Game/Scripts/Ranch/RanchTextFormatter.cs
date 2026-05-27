using System.Collections.Generic;
using NekogamiRanch.Abilities;
using NekogamiRanch.Animals;
using NekogamiRanch.MapObjects;

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

            if (selectedCell.Animal == null && selectedCell.MapObject == null)
            {
                return $"地块 ({selectedCell.Coords.x},{selectedCell.Coords.y})：空";
            }

            var lines = new List<string>();
            if (selectedCell.Animal != null)
            {
                var animal = selectedCell.Animal;
                lines.Add(animal.DisplayName);
                lines.Add($"基础收益：{animal.BaseMoney:+#;-#;0}");

                var evolutionText = GetEvolutionText(animal);
                if (!string.IsNullOrWhiteSpace(evolutionText))
                {
                    lines.Add(evolutionText.TrimEnd('\n'));
                }

                lines.Add($"技能CD：{GetAnimalCooldownText(animal)}");
                lines.Add($"技能：{GetAnimalAbilityText(animal)}");
            }

            if (selectedCell.MapObject != null)
            {
                var mapObject = selectedCell.MapObject;
                lines.Add($"地块物体：{mapObject.DisplayName}");
                lines.Add($"来源基础收益：{mapObject.SourceBaseMoney:+#;-#;0}");
            }

            return string.Join("\n", lines);
        }

        private static string GetEvolutionText(Animal animal)
        {
            return animal != null && animal.HasEvolution
                ? $"进化：Lv.{animal.EvolutionLevel} {animal.EvolutionProgress}/{animal.EvolutionThreshold}\n"
                : string.Empty;
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
