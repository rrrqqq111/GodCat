using NekogamiRanch.UI;
using UnityEditor;
using UnityEngine;

namespace NekogamiRanch.EditorTools
{
    [CustomEditor(typeof(AnimalBookCategoryToggleSelectionEffect))]
    public class AnimalBookCategoryToggleSelectionEffectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Draw("toggleRoot", "标签根节点");
            Draw("autoCollectChildToggles", "自动收集子标签");
            Draw("toggles", "标签列表");
            Draw("selectedOffsetX", "选中右移距离");
            Draw("moveDuration", "移动动画时长");
            serializedObject.ApplyModifiedProperties();
        }

        private void Draw(string propertyName, string label)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName), new GUIContent(label), true);
        }
    }

    [CustomEditor(typeof(AnimalBookIconListController))]
    public class AnimalBookIconListControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Draw("animalDataRoot", "动物数据根目录");
            Draw("scrollRect", "滚动矩形");
            Draw("contentRoot", "内容根节点");
            Draw("iconTemplate", "图标模板");
            Draw("templateAnimalIconImage", "模板动物图标图像");
            Draw("selectedAnimalPanel", "选中动物显示面板");
            Draw("templateRarityColorImages", "模板稀有度变色图像");
            Draw("rarityColors", "稀有度颜色表");
            Draw("fallbackRarityColor", "默认稀有度颜色");
            Draw("hideTemplate", "隐藏模板");
            Draw("autoLoadAnimalDataInEditor", "编辑器自动读取动物数据");
            Draw("animalCatalog", "动物数据列表");
            Draw("categories", "分类绑定列表");
            serializedObject.ApplyModifiedProperties();
        }

        private void Draw(string propertyName, string label)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName), new GUIContent(label), true);
        }
    }

    [CustomEditor(typeof(AnimalBookSelectedAnimalPanel))]
    public class AnimalBookSelectedAnimalPanelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Draw("animalNameText", "动物名称文本");
            Draw("familyIconImage", "家族图标图像");
            Draw("baseMoneyText", "基础属性文本");
            Draw("abilityText", "能力描述文本");
            Draw("animalIconImage", "动物图标图像");
            Draw("rarityColorImages", "稀有度变色图像");
            Draw("rarityColors", "稀有度颜色表");
            Draw("fallbackRarityColor", "默认稀有度颜色");
            serializedObject.ApplyModifiedProperties();
        }

        private void Draw(string propertyName, string label)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName), new GUIContent(label), true);
        }
    }

    [CustomPropertyDrawer(typeof(AnimalBookRarityColor))]
    public class AnimalBookRarityColorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var rarityProperty = property.FindPropertyRelative("rarity");
            var colorProperty = property.FindPropertyRelative("color");
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;

            var rarityRect = new Rect(position.x, position.y, position.width, lineHeight);
            var colorRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

            EditorGUI.PropertyField(rarityRect, rarityProperty, new GUIContent("稀有度"));
            EditorGUI.PropertyField(colorRect, colorProperty, new GUIContent("颜色"));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
