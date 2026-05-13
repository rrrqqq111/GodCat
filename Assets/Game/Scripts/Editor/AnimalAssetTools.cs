using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static class AnimalAssetTools
    {
        private const string AnimalFolder = "Assets/Game/Data/Animals";

        [MenuItem("Tools/Game/Fix Animal Asset Object Names")]
        public static void FixAnimalAssetObjectNames()
        {
            var guids = AssetDatabase.FindAssets("t:AnimalData", new[] { AnimalFolder });
            var fixedCount = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var expectedName = Path.GetFileNameWithoutExtension(path);
                var animalData = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (animalData == null)
                {
                    continue;
                }

                if (animalData.name == expectedName)
                {
                    continue;
                }

                animalData.name = expectedName;
                EditorUtility.SetDirty(animalData);
                fixedCount++;
            }

            if (fixedCount > 0)
            {
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"[AnimalAssetTools] Checked {guids.Length} assets, fixed {fixedCount} name mismatch(es).");
        }
    }
}
