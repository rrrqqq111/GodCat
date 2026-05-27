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
    }
}
