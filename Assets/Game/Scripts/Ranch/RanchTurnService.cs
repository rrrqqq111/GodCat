using System;
using NekogamiRanch.Items;
using NekogamiRanch.Toys;

namespace NekogamiRanch.Ranch
{
    public class RanchTurnService
    {
        private readonly RanchGameState state;
        private readonly RanchMap ranchMap;
        private readonly RanchAnimalService animalService;
        private readonly RanchOfferService offerService;
        private readonly RanchSettlementService settlementService;
        private readonly RanchDeploymentTriggerService deploymentTriggerService;
        private readonly RanchItemService itemService;
        private readonly RanchToyService toyService;
        private readonly Action stateChanged;

        public RanchTurnService(
            RanchGameState state,
            RanchMap ranchMap,
            RanchAnimalService animalService,
            RanchOfferService offerService,
            RanchSettlementService settlementService,
            RanchDeploymentTriggerService deploymentTriggerService,
            RanchItemService itemService,
            RanchToyService toyService,
            Action stateChanged)
        {
            this.state = state;
            this.ranchMap = ranchMap;
            this.animalService = animalService;
            this.offerService = offerService;
            this.settlementService = settlementService;
            this.deploymentTriggerService = deploymentTriggerService;
            this.itemService = itemService;
            this.toyService = toyService;
            this.stateChanged = stateChanged;
        }

        public void NextDay()
        {
            if (state == null)
            {
                return;
            }

            if (state.Phase == RanchPhase.DayTransition)
            {
                EnterNextDay();
                return;
            }

            if (state.Phase == RanchPhase.OfferSelection)
            {
                return;
            }

            settlementService?.ResolveDailySettlement(ranchMap);
            if (state.IsTestMode)
            {
                EnterNextDay();
                return;
            }

            offerService?.Roll(state.Day, RanchManager.AnimalOfferCount);
            if (offerService != null && offerService.CurrentOffers.Count > 0)
            {
                state.SetPhase(RanchPhase.OfferSelection);
                stateChanged?.Invoke();
                return;
            }

            EnterNextDay();
        }

        public void EnterNextDay()
        {
            EnterNextDay(true);
        }

        public void EnterNextDay(bool randomizePositions)
        {
            if (randomizePositions)
            {
                RandomizeAnimalPositionsForNextDay();
            }

            deploymentTriggerService?.TriggerDayStartDeployedAnimals();
            state.AddDay();
            state.SetPhase(state.IsTestMode ? RanchPhase.TestMode : RanchPhase.Playing);
            toyService?.Trigger(ToyTriggerType.DayStart, state.Day);
            itemService?.Trigger(ItemTriggerType.DayStart, state.Day);
            stateChanged?.Invoke();
        }

        public void RandomizeAnimalPositionsForNextDay()
        {
            if (!state.IsTestMode || state.RandomizeAnimalPositionsInTestMode)
            {
                animalService?.RandomizeAnimalPositions();
            }
        }
    }
}
