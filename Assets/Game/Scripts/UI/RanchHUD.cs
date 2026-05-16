using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NekogamiRanch.UI
{
    public class RanchHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text dayText;
        [FormerlySerializedAs("statusText")]
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text cansText;
        [SerializeField] private TMP_Text selectionText;
        [SerializeField] private TMP_Text settlementReportText;
        [SerializeField] private Button nextDayButton;
        [SerializeField] private TMP_Text nextDayButtonLabel;

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
            if (dayText != null)
            {
                dayText.text = $"第 {day} 天";
            }

            if (moneyText != null)
            {
                moneyText.text = money.ToString();
            }

            if (cansText != null)
            {
                cansText.text = cans.ToString();
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
                nextDayButtonLabel = nextDayButton.GetComponentInChildren<TMP_Text>();
            }

            if (nextDayButtonLabel == null)
            {
                return;
            }

            if (isWaitingToEnterNextDay)
            {
                nextDayButtonLabel.text = "下一天";
            }
            else
            {
                nextDayButtonLabel.text = "结算";
            }

            nextDayButton.interactable = !isWaitingForOfferSelection;
        }
    }
}
