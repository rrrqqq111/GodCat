using System.Collections.Generic;
using NekogamiRanch.Abilities;
using NekogamiRanch.Animals;
using NekogamiRanch.Items;
using NekogamiRanch.MapObjects;
using NekogamiRanch.Terrains;
using NekogamiRanch.Toys;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public abstract class ResourceConfigEditorBase : Editor
    {
        protected abstract IReadOnlyDictionary<string, string> Labels { get; }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var iterator = serializedObject.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                using (new EditorGUI.DisabledScope(iterator.propertyPath == "m_Script"))
                {
                    EditorGUILayout.PropertyField(iterator, GetLabel(iterator), true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private GUIContent GetLabel(SerializedProperty property)
        {
            if (property.propertyPath == "m_Script")
            {
                return ResourceConfigInspectorLabels.ScriptLabel;
            }

            return Labels.TryGetValue(property.propertyPath, out var label)
                ? new GUIContent(label)
                : new GUIContent(property.displayName);
        }
    }

    [CustomEditor(typeof(AnimalData))]
    public class AnimalDataInspector : ResourceConfigEditorBase
    {
        protected override IReadOnlyDictionary<string, string> Labels => ResourceConfigInspectorLabels.AnimalData;
    }

    [CustomEditor(typeof(AbilityData))]
    public class AbilityDataInspector : ResourceConfigEditorBase
    {
        protected override IReadOnlyDictionary<string, string> Labels => ResourceConfigInspectorLabels.AbilityData;
    }

    [CustomEditor(typeof(ItemData))]
    public class ItemDataInspector : ResourceConfigEditorBase
    {
        protected override IReadOnlyDictionary<string, string> Labels => ResourceConfigInspectorLabels.ItemData;
    }

    [CustomEditor(typeof(ToyData))]
    public class ToyDataInspector : ResourceConfigEditorBase
    {
        protected override IReadOnlyDictionary<string, string> Labels => ResourceConfigInspectorLabels.ToyData;
    }

    [CustomEditor(typeof(MapCellObjectData))]
    public class MapCellObjectDataInspector : ResourceConfigEditorBase
    {
        protected override IReadOnlyDictionary<string, string> Labels => ResourceConfigInspectorLabels.MapCellObjectData;
    }

    [CustomEditor(typeof(RanchTerrainData))]
    public class RanchTerrainDataInspector : ResourceConfigEditorBase
    {
        protected override IReadOnlyDictionary<string, string> Labels => ResourceConfigInspectorLabels.RanchTerrainData;
    }

    public abstract class ResourceConfigPropertyDrawerBase : PropertyDrawer
    {
        protected abstract IReadOnlyDictionary<string, string> Labels { get; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var headerRect = new Rect(position.x, position.y, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                var child = property.Copy();
                var end = child.GetEndProperty();
                var enterChildren = true;
                var y = position.y + lineHeight + spacing;

                while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
                {
                    enterChildren = false;
                    var height = EditorGUI.GetPropertyHeight(child, true);
                    var rect = new Rect(position.x, y, position.width, height);
                    EditorGUI.PropertyField(rect, child, GetChildLabel(child), true);
                    y += height + spacing;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
            {
                return height;
            }

            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var child = property.Copy();
            var end = child.GetEndProperty();
            var enterChildren = true;

            while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
            {
                enterChildren = false;
                height += spacing + EditorGUI.GetPropertyHeight(child, true);
            }

            return height;
        }

        private GUIContent GetChildLabel(SerializedProperty property)
        {
            return Labels.TryGetValue(property.name, out var label)
                ? new GUIContent(label)
                : new GUIContent(property.displayName);
        }
    }

    [CustomPropertyDrawer(typeof(AbilityEffectParams))]
    public class AbilityEffectParamsDrawer : ResourceConfigPropertyDrawerBase
    {
        protected override IReadOnlyDictionary<string, string> Labels => ResourceConfigInspectorLabels.AbilityEffectParams;
    }

    [CustomPropertyDrawer(typeof(ItemEffectParams))]
    public class ItemEffectParamsDrawer : ResourceConfigPropertyDrawerBase
    {
        protected override IReadOnlyDictionary<string, string> Labels => ResourceConfigInspectorLabels.ItemEffectParams;
    }

    [CustomPropertyDrawer(typeof(ItemTargetFilter))]
    public class ItemTargetFilterDrawer : ResourceConfigPropertyDrawerBase
    {
        protected override IReadOnlyDictionary<string, string> Labels => ResourceConfigInspectorLabels.ItemTargetFilter;
    }

    [CustomPropertyDrawer(typeof(ToyEffectParams))]
    public class ToyEffectParamsDrawer : ResourceConfigPropertyDrawerBase
    {
        protected override IReadOnlyDictionary<string, string> Labels => ResourceConfigInspectorLabels.ToyEffectParams;
    }

    [CustomPropertyDrawer(typeof(MapCellObjectEffectParams))]
    public class MapCellObjectEffectParamsDrawer : ResourceConfigPropertyDrawerBase
    {
        protected override IReadOnlyDictionary<string, string> Labels => ResourceConfigInspectorLabels.MapCellObjectEffectParams;
    }

    internal static class ResourceConfigInspectorLabels
    {
        public static readonly GUIContent ScriptLabel = new GUIContent("脚本");

        public static readonly IReadOnlyDictionary<string, string> AnimalData = new Dictionary<string, string>
        {
            ["id"] = "动物ID",
            ["animalName"] = "动物名称",
            ["family"] = "动物家族",
            ["rarity"] = "稀有度",
            ["baseMoney"] = "基础金币",
            ["evolutionThreshold"] = "进化所需次数",
            ["evolutionTarget"] = "进化目标动物",
            ["evolutionTargetLevel"] = "进化目标等级",
            ["ability"] = "动物能力",
            ["description"] = "描述",
            ["icon"] = "图标",
            ["familyIcon"] = "家族图标",
            ["iconScale"] = "图标缩放"
        };

        public static readonly IReadOnlyDictionary<string, string> AbilityData = new Dictionary<string, string>
        {
            ["id"] = "能力ID",
            ["abilityType"] = "能力类型",
            ["priority"] = "执行优先级",
            ["impactType"] = "作用范围",
            ["stackable"] = "是否可叠加",
            ["desc"] = "能力描述",
            ["triggerType"] = "触发时机",
            ["triggerChancePercent"] = "触发概率百分比",
            ["triggerLimit"] = "触发次数上限",
            ["effectType"] = "效果类型",
            ["effectScriptId"] = "效果脚本ID",
            ["effectParams"] = "效果参数",
            ["subAbilities"] = "子能力列表"
        };

        public static readonly IReadOnlyDictionary<string, string> ItemData = new Dictionary<string, string>
        {
            ["id"] = "道具ID",
            ["itemName"] = "道具名称",
            ["description"] = "道具描述",
            ["icon"] = "图标",
            ["rarity"] = "稀有度",
            ["category"] = "道具分类",
            ["targetType"] = "目标类型",
            ["useTiming"] = "使用方式",
            ["triggerType"] = "触发时机",
            ["stackMode"] = "堆叠方式",
            ["consumeOnUse"] = "使用后消耗",
            ["maxStack"] = "最大堆叠数量",
            ["canAppearInShop"] = "可出现在商店",
            ["canAppearInPacks"] = "可出现在补充包",
            ["enabledInDemo"] = "Demo中启用",
            ["effectScriptId"] = "效果脚本ID",
            ["priority"] = "执行优先级",
            ["effectParams"] = "效果参数",
            ["targetFilter"] = "目标筛选",
            ["subItems"] = "子道具列表"
        };

        public static readonly IReadOnlyDictionary<string, string> ToyData = new Dictionary<string, string>
        {
            ["id"] = "玩具ID",
            ["toyName"] = "玩具名称",
            ["description"] = "描述",
            ["icon"] = "图标",
            ["rarity"] = "稀有度",
            ["enabledInDemo"] = "Demo中启用",
            ["slotType"] = "槽位类型",
            ["equipCost"] = "装备消耗",
            ["unique"] = "唯一装备",
            ["canEquipBeforeRun"] = "开局前可装备",
            ["canUnequipDuringRun"] = "局内可卸下",
            ["unlockType"] = "解锁类型",
            ["unlockGameExp"] = "解锁所需游戏经验",
            ["unlockFlag"] = "解锁标记",
            ["effectScriptId"] = "效果脚本ID",
            ["triggerType"] = "触发时机",
            ["priority"] = "执行优先级",
            ["effectParams"] = "效果参数",
            ["tags"] = "标签"
        };

        public static readonly IReadOnlyDictionary<string, string> MapCellObjectData = new Dictionary<string, string>
        {
            ["id"] = "物体ID",
            ["objectName"] = "物体名称",
            ["description"] = "描述",
            ["icon"] = "图标",
            ["consumeScope"] = "消耗范围",
            ["consumeOnSuccess"] = "成功后消耗",
            ["effectScriptId"] = "效果脚本ID",
            ["effectParams"] = "效果参数"
        };

        public static readonly IReadOnlyDictionary<string, string> RanchTerrainData = new Dictionary<string, string>
        {
            ["id"] = "地形ID",
            ["terrainName"] = "地形名称",
            ["description"] = "描述",
            ["icon"] = "图标",
            ["tileSprite"] = "地块贴图",
            ["sizeMultiplier"] = "尺寸倍率",
            ["updateColliderSize"] = "更新碰撞尺寸"
        };

        public static readonly IReadOnlyDictionary<string, string> AbilityEffectParams = new Dictionary<string, string>
        {
            ["money"] = "金币数值",
            ["count"] = "数量",
            ["maxCount"] = "最大数量",
            ["minMultiplier"] = "最小倍率",
            ["maxMultiplier"] = "最大倍率",
            ["maxRarity"] = "最高稀有度",
            ["itemCount"] = "道具数量",
            ["initialCooldownDays"] = "初始冷却天数",
            ["cooldownDays"] = "冷却天数",
            ["cooldownReductionAmount"] = "冷却减少量",
            ["cooldownReductionTileType"] = "冷却减少地块类型",
            ["durationDays"] = "持续天数",
            ["transformChancePercent"] = "变形概率百分比",
            ["type"] = "数值类型",
            ["target"] = "目标",
            ["targetFamily"] = "目标家族",
            ["animalData"] = "动物数据",
            ["growUpAnimalDataA"] = "成长目标A",
            ["growUpWeightA"] = "成长权重A",
            ["growUpAnimalDataB"] = "成长目标B",
            ["growUpWeightB"] = "成长权重B",
            ["growUpAnimalDataC"] = "成长目标C",
            ["growUpWeightC"] = "成长权重C"
        };

        public static readonly IReadOnlyDictionary<string, string> ItemEffectParams = new Dictionary<string, string>
        {
            ["money"] = "金币数值",
            ["gold"] = "黄金数值",
            ["cans"] = "罐头数值",
            ["count"] = "数量",
            ["maxCount"] = "最大数量",
            ["day"] = "天数",
            ["tickCount"] = "触发次数",
            ["durationDays"] = "持续天数",
            ["minValue"] = "最小值",
            ["maxValue"] = "最大值",
            ["level"] = "等级",
            ["fossil"] = "化石数值",
            ["multiplier"] = "倍率",
            ["bonus"] = "加成",
            ["probability"] = "概率",
            ["animalId"] = "动物ID",
            ["targetAnimalId"] = "目标动物ID",
            ["family"] = "家族",
            ["targetFamily"] = "目标家族",
            ["tileType"] = "地块类型",
            ["statusType"] = "状态类型",
            ["resourceType"] = "资源类型",
            ["tags"] = "标签"
        };

        public static readonly IReadOnlyDictionary<string, string> ItemTargetFilter = new Dictionary<string, string>
        {
            ["animalIds"] = "动物ID列表",
            ["families"] = "家族列表",
            ["tileTypes"] = "地块类型列表",
            ["tags"] = "标签列表",
            ["minRarity"] = "最低稀有度",
            ["maxRarity"] = "最高稀有度",
            ["requireEmptyCell"] = "要求空地块",
            ["requireOccupiedCell"] = "要求已有动物",
            ["requireAdjacentTarget"] = "要求相邻目标"
        };

        public static readonly IReadOnlyDictionary<string, string> ToyEffectParams = new Dictionary<string, string>
        {
            ["money"] = "金币数值",
            ["cans"] = "罐头数值",
            ["count"] = "数量",
            ["tickCount"] = "触发次数",
            ["stage"] = "阶段",
            ["animalOfferDelta"] = "动物候选数量变化",
            ["freeAnimalRefreshCount"] = "免费动物刷新次数",
            ["freeShopRefreshCount"] = "免费商店刷新次数",
            ["interestStep"] = "利息步长",
            ["interestMoney"] = "利息金币",
            ["rarity"] = "稀有度",
            ["multiplier"] = "倍率",
            ["probability"] = "概率",
            ["animalId"] = "动物ID",
            ["animalFamily"] = "动物家族",
            ["animalRarity"] = "动物稀有度",
            ["itemId"] = "道具ID",
            ["itemRarity"] = "道具稀有度",
            ["blockedAnimalRarity"] = "屏蔽动物稀有度",
            ["blockedItemRarity"] = "屏蔽道具稀有度",
            ["targetTags"] = "目标标签"
        };

        public static readonly IReadOnlyDictionary<string, string> MapCellObjectEffectParams = new Dictionary<string, string>
        {
            ["sourceBaseMoneyMinPercent"] = "来源基础金币最小百分比",
            ["sourceBaseMoneyMaxPercent"] = "来源基础金币最大百分比",
            ["flatBaseMoneyBonus"] = "固定基础金币加成",
            ["moneyMultiplier"] = "金币倍率",
            ["moneyBonus"] = "金币加成"
        };
    }
}
