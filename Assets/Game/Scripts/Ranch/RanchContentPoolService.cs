using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.Items;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchContentPoolService
    {
        private readonly string animalDataRoot;
        private readonly string itemDataRoot;

        public RanchContentPoolService(string animalDataRoot, string itemDataRoot)
        {
            this.animalDataRoot = animalDataRoot;
            this.itemDataRoot = itemDataRoot;
        }

        public void RefreshPools(
            bool autoPopulateOfferPoolByFamily,
            IReadOnlyList<string> offerPoolFamilies,
            ref List<AnimalData> offerPool,
            ref List<AnimalData> abilitySpawnPool,
            ref List<ItemData> itemRewardPool)
        {
#if UNITY_EDITOR
            if (autoPopulateOfferPoolByFamily)
            {
                offerPool = RanchContentCatalog.LoadOfferAnimals(animalDataRoot, offerPoolFamilies);
            }

            abilitySpawnPool = RanchContentCatalog.LoadAnimals(animalDataRoot);
            itemRewardPool = RanchContentCatalog.LoadItems(itemDataRoot);
#else
            if (autoPopulateOfferPoolByFamily)
            {
                offerPool = FilterAnimalsByFamily(abilitySpawnPool, offerPoolFamilies);
            }
#endif
        }

        public IReadOnlyList<AnimalData> RollRandomStartingAnimals(IReadOnlyList<AnimalData> offerPool, int count)
        {
            if (count <= 0 || offerPool == null || offerPool.Count == 0)
            {
                return Array.Empty<AnimalData>();
            }

            var validPool = offerPool.Where(data => data != null).ToList();
            if (validPool.Count == 0)
            {
                return Array.Empty<AnimalData>();
            }

            var results = new List<AnimalData>();
            for (var i = 0; i < count; i++)
            {
                results.Add(validPool[UnityEngine.Random.Range(0, validPool.Count)]);
            }

            return results;
        }

        public bool TryUseStartingAnimalsAsOfferPool(IReadOnlyList<AnimalData> startingAnimals, ref List<AnimalData> offerPool)
        {
            if (startingAnimals == null || startingAnimals.Count == 0 || offerPool.Count > 0)
            {
                return false;
            }

            offerPool = startingAnimals.Where(data => data != null).Distinct().ToList();
            return offerPool.Count > 0;
        }

        private List<AnimalData> FilterAnimalsByFamily(IReadOnlyList<AnimalData> animals, IReadOnlyList<string> families)
        {
            var familyFilters = (families ?? Array.Empty<string>())
                .Where(family => !string.IsNullOrWhiteSpace(family))
                .Select(family => family.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (familyFilters.Count == 0 || animals == null)
            {
                return new List<AnimalData>();
            }

            return animals
                .Where(data => data != null && familyFilters.Any(data.HasFamily))
                .OrderBy(data => GetFamilySortIndex(familyFilters, data.Family))
                .ThenBy(data => data.Rarity)
                .ThenBy(data => data.DisplayName)
                .ThenBy(data => data.Id)
                .ToList();
        }

        private int GetFamilySortIndex(IReadOnlyList<string> familyFilters, string family)
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
    }
}
