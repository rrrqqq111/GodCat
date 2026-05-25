using System;
using NekogamiRanch.Animals;
using UnityEngine.Serialization;

namespace NekogamiRanch.Abilities
{
    [Serializable]
    public class AbilityEffectParams
    {
        public int money;
        public int maxCount;
        [FormerlySerializedAs("delayDays")]
        public int initialCooldownDays;
        public int cooldownDays;
        public int cooldownReductionAmount;
        public string cooldownReductionTileType = "None";
        public int durationDays = 1;
        public int transformChancePercent = 100;
        public string type = "Flat";
        public string target = "Self";
        public string targetFamily = "None";
        public AnimalData animalData;
        public AnimalData growUpAnimalDataA;
        public int growUpWeightA;
        public AnimalData growUpAnimalDataB;
        public int growUpWeightB;
        public AnimalData growUpAnimalDataC;
        public int growUpWeightC;
    }
}
