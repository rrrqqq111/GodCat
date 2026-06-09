using NekogamiRanch.CatGods;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    [CustomEditor(typeof(CatGodData))]
    public class CatGodDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDisabledScriptField();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("catGodName"), new GUIContent("猫神名称"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skillName"), new GUIContent("技能名称"));

            var skillTypeProperty = serializedObject.FindProperty("skillType");
            EditorGUILayout.PropertyField(skillTypeProperty, new GUIContent("技能类型"));

            if ((CatGodSkillType)skillTypeProperty.enumValueIndex == CatGodSkillType.Active)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("activeSkillCooldownDays"), new GUIContent("主动技能CD"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("skillDescription"), new GUIContent("技能描述"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("catGodImage"), new GUIContent("猫神图片"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDisabledScriptField()
        {
            var scriptProperty = serializedObject.FindProperty("m_Script");
            if (scriptProperty == null)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(scriptProperty, new GUIContent("脚本"));
            }
        }
    }
}
