using System.Linq;
using NekogamiRanch.Abilities;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public class RanchDeploymentTriggerService
    {
        public const string DayStartDeployedTriggerType = "DayStartDeployed";

        private readonly RanchAnimalService animalService;
        private readonly System.Func<Animal, string, AbilityExecutionResult> triggerAbility;

        public RanchDeploymentTriggerService(
            RanchAnimalService animalService,
            System.Func<Animal, string, AbilityExecutionResult> triggerAbility)
        {
            this.animalService = animalService;
            this.triggerAbility = triggerAbility;
        }

        public void TriggerDayStartDeployedAnimals()
        {
            if (animalService == null || triggerAbility == null)
            {
                return;
            }

            var deployedAnimals = animalService.Animals
                .Where(animal => animal != null && animalService.IsAnimalOnMap(animal))
                .ToList();

            foreach (var animal in deployedAnimals)
            {
                if (!animalService.IsAnimalOnMap(animal))
                {
                    continue;
                }

                triggerAbility(animal, DayStartDeployedTriggerType);
            }
        }
    }
}
