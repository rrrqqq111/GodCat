using NekogamiRanch.Talents;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    [CustomEditor(typeof(TalentData))]
    public class TalentDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDisabledScriptField();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("talentName"), new GUIContent("天赋名称"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("天赋描述"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"), new GUIContent("天赋图片"));

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
