using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static class UserCacheTools
    {
        [MenuItem("Tools/Game/Clear User Cache")]
        public static void ClearUserCache()
        {
            var confirmed = EditorUtility.DisplayDialog(
                "Clear User Cache",
                "This will clear all PlayerPrefs for this project, including selected families, settings, volume, resolution, and other local user cache data.",
                "Clear",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[UserCacheTools] Cleared all PlayerPrefs user cache data.");
        }
    }
}
