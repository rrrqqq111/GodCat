using System;
using System.Collections;
using NekogamiRanch.Ranch;
using NekogamiRanch.UI;
using UnityEngine;

namespace NekogamiRanch.Presentation
{
    public class RanchTurnFlowController : MonoBehaviour
    {
        [SerializeField] private RanchAnimationDirector director;
        [SerializeField] private RanchHUD hud;

        private RanchManager manager;
        private Action resolveNextDay;
        private Coroutine runningFlow;
        private bool initialized;

        public bool IsAnimating { get; private set; }

        public void Initialize(RanchManager ranchManager, RanchAnimationDirector animationDirector, Action onResolveNextDay)
        {
            manager = ranchManager;
            director = animationDirector != null ? animationDirector : director;
            resolveNextDay = onResolveNextDay;
            initialized = manager != null && resolveNextDay != null;
        }

        public bool PlayNextDayFlow()
        {
            if (!initialized)
            {
                return false;
            }

            if (IsAnimating)
            {
                return true;
            }

            runningFlow = StartCoroutine(NextDayFlowRoutine());
            return true;
        }

        private IEnumerator NextDayFlowRoutine()
        {
            IsAnimating = true;
            ResolveHud();
            hud?.MoveNextDayButtonOffscreen();
            var playedAnimatedSettlement = false;

            if (director != null)
            {
                director.HideAnimals(manager.Map);
                yield return director.PlayDayTransition();
                yield return director.PlayGateClose();
                manager?.RandomizeAnimalPositionsForNextDay();
                director.HideAnimals(manager.Map);
                yield return director.PlayGateOpenWithDaylight();
                if (manager == null || !manager.IsWaitingForOfferSelection)
                {
                    yield return director.PlayAnimalEnterSequence(manager.Map);
                }

                yield return PlayAnimatedSettlementRoutine();
                playedAnimatedSettlement = true;
            }
            else
            {
                yield return new WaitForSeconds(0.25f);
            }

            if (!playedAnimatedSettlement)
            {
                resolveNextDay?.Invoke();
            }

            IsAnimating = false;
            runningFlow = null;

            if (manager == null || !manager.IsWaitingForOfferSelection)
            {
                hud?.RestoreNextDayButton();
                hud?.SetNextDayInteractable(true);
            }
        }

        private void ResolveHud()
        {
            if (hud == null)
            {
                hud = FindObjectOfType<RanchHUD>();
            }
        }

        private void OnDisable()
        {
            if (runningFlow != null)
            {
                StopCoroutine(runningFlow);
                runningFlow = null;
            }

            IsAnimating = false;
            hud?.RestoreNextDayButton();
        }

        private IEnumerator PlayAnimatedSettlementRoutine()
        {
            if (manager == null)
            {
                yield break;
            }

            manager.BeginDailySettlement();
            var triggers = manager.DailySettlementAbilityTriggers;
            foreach (var triggerType in triggers)
            {
                var animalsAtPhaseStart = manager.GetAnimalsInSettlementScanOrder();
                foreach (var animal in animalsAtPhaseStart)
                {
                    if (!manager.HasSettlementAbilityTrigger(animal, triggerType))
                    {
                        continue;
                    }

                    if (director != null)
                    {
                        yield return director.PlayAnimalAbility(manager.Map, animal);
                    }

                    manager.ResolveSettlementAbility(animal, triggerType);
                    if (director != null)
                    {
                        yield return director.PlayPreySequence(manager.Map, manager.ConsumePreyAnimationRequests());
                    }
                }
            }

            manager.CompleteAnimatedDailySettlement();
        }
    }
}
