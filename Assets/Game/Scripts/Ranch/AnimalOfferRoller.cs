using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class AnimalOfferRoller : MonoBehaviour
    {
        [SerializeField] private List<AnimalData> offerPool = new List<AnimalData>();
        [SerializeField] private bool allowDuplicateOffers;
        [SerializeField] private bool lockLegendaryBeforeDay = true;
        [SerializeField] private int legendaryUnlockDay = 5;
        [SerializeField, Range(0, 4)] private int legendaryRarity = 4;

        public IReadOnlyList<AnimalData> Roll(int day, int count, IReadOnlyList<AnimalData> fallbackPool = null)
        {
            var pool = offerPool.Count > 0 ? offerPool : fallbackPool;
            if (pool == null || count <= 0)
            {
                return Array.Empty<AnimalData>();
            }

            var validPool = pool
                .Where(data => data != null)
                .Where(data => IsAllowedToday(data, day))
                .ToList();

            if (validPool.Count == 0)
            {
                validPool = pool.Where(data => data != null).ToList();
            }

            if (validPool.Count == 0)
            {
                return Array.Empty<AnimalData>();
            }

            if (allowDuplicateOffers)
            {
                return RollWithDuplicates(validPool, count);
            }

            return validPool
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(count)
                .ToList();
        }

        private bool IsAllowedToday(AnimalData data, int currentDay)
        {
            if (!lockLegendaryBeforeDay || currentDay >= legendaryUnlockDay)
            {
                return true;
            }

            return !IsLegendary(data);
        }

        private bool IsLegendary(AnimalData data)
        {
            if (data == null)
            {
                return false;
            }

            return data.Rarity >= legendaryRarity;
        }

        private static IReadOnlyList<AnimalData> RollWithDuplicates(IReadOnlyList<AnimalData> pool, int count)
        {
            var results = new List<AnimalData>();
            for (var i = 0; i < count; i++)
            {
                results.Add(pool[UnityEngine.Random.Range(0, pool.Count)]);
            }

            return results;
        }
    }
}
