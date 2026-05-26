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
        private readonly RanchItemService itemService;
        private readonly RanchToyService toyService;
        private readonly Action stateChanged;

        public RanchTurnService(
            RanchGameState state,
            RanchMap ranchMap,
            RanchAnimalService animalService,
            RanchOfferService offerService,
            RanchSettlementService settlementService,
            RanchItemService itemService,
            RanchToyService toyService,
            Action stateChanged)
        {
            this.state = state;
            this.ranchMap = ranchMap;
            this.animalService = animalService;
            this.offerService = offerService;
            this.settlementService = settlementService;
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
                state.SetPhase(RanchPhase.DayTransition);
                stateChanged?.Invoke();
                return;
            }

            offerService?.Roll(state.Day, 3);
            state.SetPhase(offerService != null && offerService.CurrentOffers.Count > 0
                ? RanchPhase.OfferSelection
                : RanchPhase.DayTransition);
            stateChanged?.Invoke();
        }

        private void EnterNextDay()
        {
            if (!state.IsTestMode || state.RandomizeAnimalPositionsInTestMode)
            {
                animalService?.RandomizeAnimalPositions();
            }

            state.AddDay();
            state.SetPhase(state.IsTestMode ? RanchPhase.TestMode : RanchPhase.Playing);
            toyService?.Trigger(ToyTriggerType.DayStart, state.Day);
            itemService?.Trigger(ItemTriggerType.DayStart, state.Day);
            stateChanged?.Invoke();
        }
    }
}
