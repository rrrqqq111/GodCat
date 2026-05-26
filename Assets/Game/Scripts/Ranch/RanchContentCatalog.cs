using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.Items;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NekogamiRanch.Ranch
{
    public static class RanchContentCatalog
    {
#if UNITY_EDITOR
        public static List<AnimalData> LoadOfferAnimals(string root, IReadOnlyList<string> families)
        {
            var familyFilters = (families ?? Array.Empty<string>())
                .Where(family => !string.IsNullOrWhiteSpace(family))
                .Select(family => family.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (familyFilters.Count == 0)
            {
                return new List<AnimalData>();
            }

            var familySet = new HashSet<string>(familyFilters, StringComparer.OrdinalIgnoreCase);
            return LoadAnimals(root)
                .Where(data => familySet.Contains(data.Family))
                .OrderBy(data => GetFamilySortIndex(familyFilters, data.Family))
                .ThenBy(data => data.Rarity)
                .ThenBy(data => data.DisplayName)
                .ThenBy(data => data.Id)
                .ToList();
        }

        public static List<AnimalData> LoadAnimals(string root)
        {
            return AssetDatabase.FindAssets("t:AnimalData", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AnimalData>)
                .Where(data => data != null)
                .OrderBy(data => data.Family)
                .ThenBy(data => data.Rarity)
                .ThenBy(data => data.DisplayName)
                .ToList();
        }

        public static List<ItemData> LoadItems(string root)
        {
            return AssetDatabase.FindAssets("t:ItemData", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ItemData>)
                .Where(item => item != null)
                .OrderBy(item => item.Rarity)
                .ThenBy(item => item.DisplayName)
                .ToList();
        }

        private static int GetFamilySortIndex(IReadOnlyList<string> familyFilters, string family)
        {
            for (var i = 0; i < familyFilters.Count; i++)
            {
                if (string.Equals(familyFilters[i], family, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return int.MaxValue;
        }
#endif
    }
}
