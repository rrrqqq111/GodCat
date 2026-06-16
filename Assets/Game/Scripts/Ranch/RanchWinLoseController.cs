using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NekogamiRanch.Ranch
{
    public class RanchWinLoseController : MonoBehaviour
    {
        [SerializeField] private List<RanchStageGoal> stageGoals = new List<RanchStageGoal>
        {
            new RanchStageGoal()
        };
        [SerializeField, Min(0)] private int currentStageIndex;
        [FormerlySerializedAs("failurePanel")]
        [SerializeField] private GameObject stageEndPanel;
        [SerializeField] private Button payButton;
        [SerializeField] private string giveUpSceneName = "GameStartSence";

        private RanchManager manager;

        public int CurrentStageIndex => currentStageIndex;
        public int CurrentStageNumber => currentStageIndex + 1;
        public int CurrentStageRequiredTotalMoney => GetCurrentGoal().RequiredTotalMoney;
        public bool HasResult => Result != RanchGameResult.Playing || IsStageEndPanelOpen;
        public bool IsStageEndPanelOpen { get; private set; }
        public RanchGameResult Result { get; private set; } = RanchGameResult.Playing;

        public void Initialize(RanchManager ranchManager)
        {
            manager = ranchManager;
        }

        private void Awake()
        {
            if (stageEndPanel != null)
            {
                stageEndPanel.SetActive(false);
            }

            SetPayButtonInteractable(false);
        }

        private void OnValidate()
        {
            if (stageGoals == null)
            {
                stageGoals = new List<RanchStageGoal>();
            }

            if (stageGoals.Count == 0)
            {
                stageGoals.Add(new RanchStageGoal());
            }

            currentStageIndex = Mathf.Clamp(currentStageIndex, 0, stageGoals.Count - 1);
        }

        public bool TryResolveStageEnd(int day, int totalMoney)
        {
            if (HasResult || !IsStageEndDay(day))
            {
                return false;
            }

            var goal = GetCurrentGoal();
            Result = totalMoney >= goal.RequiredTotalMoney ? RanchGameResult.StagePassed : RanchGameResult.Lost;
            ShowStageEndPanel();
            SetPayButtonInteractable(Result == RanchGameResult.StagePassed);
            return true;
        }

        public void ResetProgress()
        {
            currentStageIndex = 0;
            Result = RanchGameResult.Playing;
            IsStageEndPanelOpen = false;
            HideStageEndPanel();
            SetPayButtonInteractable(false);
        }

        public void GiveUpAndReturnToGameStart()
        {
            var sceneName = string.IsNullOrWhiteSpace(giveUpSceneName) ? "GameStartSence" : giveUpSceneName.Trim();

            if (global::SceneTransitionManager.Instance != null)
            {
                global::SceneTransitionManager.Instance.LoadSceneWithTransition(sceneName);
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        public void PayAndEnterNextStage()
        {
            if (!IsStageEndPanelOpen || Result != RanchGameResult.StagePassed)
            {
                return;
            }

            if (!TryAdvanceToNextStage())
            {
                return;
            }

            Result = RanchGameResult.Playing;
            IsStageEndPanelOpen = false;
            HideStageEndPanel();
            SetPayButtonInteractable(false);
            manager?.ContinueAfterStagePayment();
        }

        public void OnGiveUpButtonClicked()
        {
            GiveUpAndReturnToGameStart();
        }

        public void OnPayButtonClicked()
        {
            PayAndEnterNextStage();
        }

        private bool IsStageEndDay(int day)
        {
            var stageEndDay = 0;
            for (var i = 0; i <= currentStageIndex && i < stageGoals.Count; i++)
            {
                stageEndDay += stageGoals[i].DurationDays;
            }

            return day >= stageEndDay;
        }

        private RanchStageGoal GetCurrentGoal()
        {
            if (stageGoals == null || stageGoals.Count == 0)
            {
                return new RanchStageGoal();
            }

            currentStageIndex = Mathf.Clamp(currentStageIndex, 0, stageGoals.Count - 1);
            return stageGoals[currentStageIndex];
        }

        private void ShowStageEndPanel()
        {
            IsStageEndPanelOpen = true;
            if (stageEndPanel != null)
            {
                stageEndPanel.SetActive(true);
            }
        }

        private void HideStageEndPanel()
        {
            if (stageEndPanel != null)
            {
                stageEndPanel.SetActive(false);
            }
        }

        private bool TryAdvanceToNextStage()
        {
            if (stageGoals == null || stageGoals.Count == 0)
            {
                return false;
            }

            if (currentStageIndex >= stageGoals.Count - 1)
            {
                Debug.LogWarning("[RanchWinLoseController] Cannot pay to enter the next stage because no next stage is configured.");
                return false;
            }

            currentStageIndex++;
            return true;
        }

        private void SetPayButtonInteractable(bool interactable)
        {
            if (payButton != null)
            {
                payButton.interactable = interactable;
            }
        }
    }

    public enum RanchGameResult
    {
        Playing = 0,
        StagePassed = 1,
        Lost = 2
    }

    [Serializable]
    public class RanchStageGoal
    {
        [SerializeField, Min(1)] private int durationDays = 6;
        [SerializeField, Min(0)] private int requiredTotalMoney = 25;

        public int DurationDays => Mathf.Max(1, durationDays);
        public int RequiredTotalMoney => Mathf.Max(0, requiredTotalMoney);
    }
}
