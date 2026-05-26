using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchAnimalSpawnService
    {
        private readonly RanchAnimalService animalService;
        private readonly IReadOnlyList<AnimalData> abilitySpawnPool;

        public RanchAnimalSpawnService(RanchAnimalService animalService, IReadOnlyList<AnimalData> abilitySpawnPool)
        {
            this.animalService = animalService;
            this.abilitySpawnPool = abilitySpawnPool ?? Array.Empty<AnimalData>();
        }

        public bool TrySpawnRandomFromFamily(string family, int baseMoneyBonus, out Animal addedAnimal)
        {
            addedAnimal = null;
            if (animalService == null || string.IsNullOrWhiteSpace(family))
            {
                return false;
            }

            var candidates = abilitySpawnPool
                .Where(data => data != null && string.Equals(data.Family, family, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (candidates.Count == 0)
            {
                return false;
            }

            var selectedData = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            if (!animalService.TryAddAnimalToRandomEmptyCell(selectedData, out addedAnimal))
            {
                return false;
            }

            if (baseMoneyBonus != 0)
            {
                addedAnimal.AddPermanentBaseMoneyBonus(baseMoneyBonus);
            }

            return true;
        }
    }
}
