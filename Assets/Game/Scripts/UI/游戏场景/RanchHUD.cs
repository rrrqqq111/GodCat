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
        [Header("Next Day Button")]
        [InspectorName("下一天按钮下移距离")]
        [SerializeField, Min(0f)] private float hiddenButtonOffset = 240f;

        private RectTransform nextDayButtonRect;
        private Vector2 nextDayButtonShownPosition;
        private bool hasShownButtonPosition;
        private bool isNextDayButtonHidden;

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

            CacheNextDayButtonPosition();
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

            nextDayButtonLabel.text = "下一天";

            nextDayButton.interactable = !isWaitingForOfferSelection;
        }

        public void SetNextDayInteractable(bool interactable)
        {
            if (nextDayButton != null)
            {
                nextDayButton.interactable = interactable;
            }
        }

        public void MoveNextDayButtonOffscreen()
        {
            CacheNextDayButtonPosition();
            if (nextDayButtonRect == null || !hasShownButtonPosition)
            {
                SetNextDayInteractable(false);
                return;
            }

            nextDayButtonRect.anchoredPosition = nextDayButtonShownPosition + Vector2.down * hiddenButtonOffset;
            isNextDayButtonHidden = true;
            SetNextDayInteractable(false);
        }

        public void RestoreNextDayButton()
        {
            CacheNextDayButtonPosition();
            if (nextDayButtonRect != null && hasShownButtonPosition)
            {
                nextDayButtonRect.anchoredPosition = nextDayButtonShownPosition;
            }

            isNextDayButtonHidden = false;
        }

        private void CacheNextDayButtonPosition()
        {
            if (nextDayButton == null)
            {
                return;
            }

            if (nextDayButtonRect == null)
            {
                nextDayButtonRect = nextDayButton.transform as RectTransform;
            }

            if (!hasShownButtonPosition && nextDayButtonRect != null && !isNextDayButtonHidden)
            {
                nextDayButtonShownPosition = nextDayButtonRect.anchoredPosition;
                hasShownButtonPosition = true;
            }
        }
    }
}
