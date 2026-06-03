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

            if (director != null)
            {
                yield return director.PlayDayTransition();
                yield return director.PlayGateSequence();
            }
            else
            {
                yield return new WaitForSeconds(0.25f);
            }

            resolveNextDay?.Invoke();
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
    }
}
