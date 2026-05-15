using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NekogamiRanch.UI
{
    public class RanchHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI selectionText;
        [SerializeField] private TextMeshProUGUI settlementReportText;
        [SerializeField] private Button nextDayButton;
        [SerializeField] private TextMeshProUGUI nextDayButtonLabel;

        public void Initialize(Action onNextDayClicked)
        {
            if (nextDayButton == null)
            {
                Debug.LogError("[RanchHUD] nextDayButton is not assigned. Create and bind it in the scene.");
                return;
            }

            nextDayButton.onClick.RemoveAllListeners();
            if (onNextDayClicked != null)
            {
                nextDayButton.onClick.AddListener(() => onNextDayClicked());
            }
        }

        public void Refresh(int day, int money, int cans, string selectionTextValue, string settlementReport, bool isWaitingForOfferSelection, bool isWaitingToEnterNextDay)
        {
            if (statusText != null)
            {
                statusText.text = $"Day {day}   Gold {money}   Cans {cans}";
            }

            if (selectionText != null)
            {
                selectionText.text = selectionTextValue;
            }

            if (settlementReportText != null)
            {
                settlementReportText.text = settlementReport;
            }

            RefreshActionButton(isWaitingForOfferSelection, isWaitingToEnterNextDay);
        }

        private void RefreshActionButton(bool isWaitingForOfferSelection, bool isWaitingToEnterNextDay)
        {
            if (nextDayButtonLabel == null && nextDayButton != null)
            {
                nextDayButtonLabel = nextDayButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (nextDayButtonLabel == null)
            {
                return;
            }

            if (isWaitingToEnterNextDay)
            {
                nextDayButtonLabel.text = "Enter Next Day";
            }
            else
            {
                nextDayButtonLabel.text = "Settle Today";
            }

            nextDayButton.interactable = !isWaitingForOfferSelection;
        }
    }
}
