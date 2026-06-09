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

    [CustomEditor(typeof(BookToggleFolderAssetLoader))]
    public class BookToggleFolderAssetLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("刷新资源缓存"))
            {
                foreach (var targetObject in targets)
                {
                    var method = targetObject.GetType().GetMethod(
                        "RefreshCachedAssetsFromFolder",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    method?.Invoke(targetObject, null);
                    EditorUtility.SetDirty(targetObject);
                }

                serializedObject.Update();
            }

            Draw("toggle", "触发标签");
            Draw("assetFolder", "资源文件夹");
            Draw("assetTypeName", "资源类型名");
            Draw("nameFieldName", "名称字段名");
            Draw("iconFieldName", "图像字段名");
            Draw("scrollRect", "滚动矩形");
            Draw("contentRoot", "内容根节点");
            Draw("iconTemplate", "图标模板");
            Draw("templateIconImage", "模板图像组件");
            Draw("templateNameText", "模板名称文本");
            Draw("hideTemplate", "隐藏模板");
            Draw("autoSelectFirstAsset", "刷新后自动选择第一个");
            Draw("autoLoadAssetsInEditor", "编辑器自动读取资源");
            Draw("cachedAssets", "缓存资源列表");
            Draw("onAssetClicked", "点击资源事件");
            serializedObject.ApplyModifiedProperties();
        }

        private void Draw(string propertyName, string label)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName), new GUIContent(label), true);
        }
    }

    [CustomEditor(typeof(BookAssetDetailPanel))]
    public class BookAssetDetailPanelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Draw("memberBindings", "属性绑定列表");
            Draw("emptyText", "空文本");
            Draw("hideImageWhenMissing", "找不到字段时隐藏图像");
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

    [CustomPropertyDrawer(typeof(BookAssetDetailPanel.MemberBinding))]
    public class BookAssetDetailPanelMemberBindingDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var y = position.y;
            DrawLine(position, ref y, property.FindPropertyRelative("memberName"), "字段或属性名");
            DrawLine(position, ref y, property.FindPropertyRelative("prefix"), "前缀文本");
            DrawLine(position, ref y, property.FindPropertyRelative("suffix"), "后缀文本");
            DrawLine(position, ref y, property.FindPropertyRelative("targetText"), "目标文本");
            DrawLine(position, ref y, property.FindPropertyRelative("targetImage"), "目标图像");
            DrawLine(position, ref y, property.FindPropertyRelative("preserveAspect"), "图像保持比例");

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 6f + EditorGUIUtility.standardVerticalSpacing * 5f;
        }

        private static void DrawLine(Rect position, ref float y, SerializedProperty property, string label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            var rect = new Rect(position.x, y, position.width, height);
            EditorGUI.PropertyField(rect, property, new GUIContent(label), true);
            y += height + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
