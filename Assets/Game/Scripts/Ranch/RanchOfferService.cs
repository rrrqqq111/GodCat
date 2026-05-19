using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public class RanchOfferService
    {
        private readonly List<AnimalData> currentOffers = new List<AnimalData>();
        private readonly AnimalOfferRoller offerRoller;
        private readonly IReadOnlyList<AnimalData> offerPool;

        public RanchOfferService(AnimalOfferRoller offerRoller, IReadOnlyList<AnimalData> offerPool)
        {
            this.offerRoller = offerRoller;
            this.offerPool = offerPool;
        }

        public IReadOnlyList<AnimalData> CurrentOffers => currentOffers;

        public void Clear()
        {
            currentOffers.Clear();
        }

        public void Roll(int day, int count)
        {
            currentOffers.Clear();
            if (offerRoller != null)
            {
                currentOffers.AddRange(offerRoller.Roll(day, count, offerPool));
                return;
            }

            RollWithoutRoller(count);
        }

        public bool SelectOffer(int index, RanchAnimalService animalService)
        {
            if (index < 0 || index >= currentOffers.Count || animalService == null)
            {
                return false;
            }

            var selectedAnimal = currentOffers[index];
            var added = animalService.TryAddAnimalToRandomEmptyCell(selectedAnimal);
            currentOffers.Clear();
            return added;
        }

        private void RollWithoutRoller(int count)
        {
            if (offerPool == null || offerPool.Count == 0)
            {
                return;
            }

            var validPool = offerPool.Where(data => data != null).ToList();
            if (validPool.Count == 0)
            {
                return;
            }

            var shuffledPool = validPool.OrderBy(_ => UnityEngine.Random.value).ToList();
            for (var i = 0; i < count && i < shuffledPool.Count; i++)
            {
                currentOffers.Add(shuffledPool[i]);
            }
        }
    }
}
