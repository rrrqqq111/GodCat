using System.Collections.Generic;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public class RanchRosterService
    {
        private readonly List<Animal> animals = new List<Animal>();

        public IReadOnlyList<Animal> Animals => animals;

        public void Add(Animal animal)
        {
            if (animal != null && !animals.Contains(animal))
            {
                animals.Add(animal);
            }
        }

        public bool Remove(Animal animal)
        {
            return animal != null && animals.Remove(animal);
        }

        public void Clear()
        {
            animals.Clear();
        }
    }
}
