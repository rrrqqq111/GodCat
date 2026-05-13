using System;
using System.Collections.Generic;
using NekogamiRanch.Animals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NekogamiRanch.UI
{
    public class AnimalOfferPanel : MonoBehaviour
    {
        [SerializeField] private GameObject sharedRoot;
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private Toggle visibilityToggle;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform cardParent;
        [SerializeField] private GameObject cardTemplate;

        private readonly List<GameObject> spawnedCards = new List<GameObject>();
        private Action<int> offerSelected;
        private bool isShowingOffers;

        public void Initialize(Action<int> onOfferSelected)
        {
            offerSelected = onOfferSelected;
            if (sharedRoot == null)
            {
                sharedRoot = gameObject;
            }

            if (contentRoot == null)
            {
                contentRoot = gameObject;
            }

            if (cardTemplate != null)
            {
                cardTemplate.SetActive(false);
                if (cardParent == null)
                {
                    cardParent = cardTemplate.transform.parent;
                }
            }

            if (visibilityToggle != null)
            {
                visibilityToggle.onValueChanged.RemoveListener(SetContentVisible);
                visibilityToggle.onValueChanged.AddListener(SetContentVisible);
            }

            HideAll();
        }

        public void Refresh(IReadOnlyList<AnimalData> offers, bool show)
        {
            if (!show)
            {
                HideAll();
                return;
            }

            var shouldOpenContent = !isShowingOffers;
            isShowingOffers = true;
            ShowSharedRoot();
            if (shouldOpenContent)
            {
                SetToggleVisible(true);
                SetContentVisible(true);
            }

            ClearCards();

            if (titleText != null)
            {
                titleText.text = "\u9009\u62e9 1 \u53ea\u52a8\u7269";
            }

            if (cardTemplate == null || cardParent == null)
            {
                Debug.LogWarning("[AnimalOfferPanel] Card template or card parent is missing.");
                return;
            }

            if (offers == null)
            {
                return;
            }

            for (var i = 0; i < offers.Count; i++)
            {
                var offer = offers[i];
                if (offer == null)
                {
                    continue;
                }

                CreateCard(offer, i);
            }
        }

        private void ShowSharedRoot()
        {
            if (sharedRoot != null)
            {
                sharedRoot.SetActive(true);
            }
        }

        private void HideAll()
        {
            isShowingOffers = false;
            ClearCards();
            if (sharedRoot != null)
            {
                sharedRoot.SetActive(false);
            }
        }

        private void SetToggleVisible(bool visible)
        {
            if (visibilityToggle != null)
            {
                visibilityToggle.SetIsOnWithoutNotify(visible);
            }
        }

        private void SetContentVisible(bool visible)
        {
            if (contentRoot != null)
            {
                contentRoot.SetActive(visible);
            }
        }

        private void CreateCard(AnimalData offer, int index)
        {
            var card = Instantiate(cardTemplate, cardParent);
            card.name = $"OfferCard_{offer.Id}_{index}";
            card.SetActive(true);
            spawnedCards.Add(card);

            var button = card.GetComponent<Button>() ?? card.GetComponentInChildren<Button>(true);
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                var capturedIndex = index;
                button.onClick.AddListener(() => SelectOffer(capturedIndex));
            }

            var label = card.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = FormatOfferText(offer);
            }

            var images = card.GetComponentsInChildren<Image>(true);
            foreach (var image in images)
            {
                if (image == null || image.gameObject == card || offer.Icon == null)
                {
                    continue;
                }

                image.sprite = offer.Icon;
                image.preserveAspect = true;
                break;
            }
        }

        private void SelectOffer(int index)
        {
            offerSelected?.Invoke(index);
            HideAll();
        }

        private static string FormatOfferText(AnimalData data)
        {
            return data == null ? "\u672a\u77e5" : $"{data.DisplayName}  ({data.BaseMoney:+#;-#;0})";
        }

        private void ClearCards()
        {
            for (var i = spawnedCards.Count - 1; i >= 0; i--)
            {
                var card = spawnedCards[i];
                if (card != null)
                {
                    Destroy(card);
                }
            }

            spawnedCards.Clear();
        }

        private void OnDestroy()
        {
            ClearCards();
            if (visibilityToggle != null)
            {
                visibilityToggle.onValueChanged.RemoveListener(SetContentVisible);
            }
        }
    }
}
