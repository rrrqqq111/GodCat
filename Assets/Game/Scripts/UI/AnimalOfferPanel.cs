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
        [Header("Card Template Bindings")]
        [SerializeField] private Image animalIconImage;
        [SerializeField] private TextMeshProUGUI animalNameText;
        [SerializeField] private Image familyIconImage;
        [SerializeField] private TextMeshProUGUI baseMoneyText;
        [SerializeField] private TextMeshProUGUI abilityText;

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

            ApplyCardData(card, offer);
        }

        private void SelectOffer(int index)
        {
            offerSelected?.Invoke(index);
            HideAll();
        }

        private void ApplyCardData(GameObject card, AnimalData offer)
        {
            var animalImage = FindSpawnedComponent(card, animalIconImage);
            if (animalImage != null)
            {
                ApplyImage(animalImage, offer.Icon);
            }

            var nameLabel = FindSpawnedComponent(card, animalNameText);
            if (nameLabel != null)
            {
                nameLabel.text = offer.DisplayName;
            }

            var familyImage = FindSpawnedComponent(card, familyIconImage);
            if (familyImage != null)
            {
                ApplyImage(familyImage, offer.FamilyIcon);
            }

            var moneyLabel = FindSpawnedComponent(card, baseMoneyText);
            if (moneyLabel != null)
            {
                moneyLabel.text = FormatBaseMoney(offer);
            }

            var abilityLabel = FindSpawnedComponent(card, abilityText);
            if (abilityLabel != null)
            {
                abilityLabel.text = FormatAbilityText(offer);
            }

            ApplyLegacyFallback(card, offer, nameLabel != null, animalImage != null);
        }

        private void ApplyLegacyFallback(GameObject card, AnimalData offer, bool hasNameLabel, bool hasAnimalImage)
        {
            if (!hasNameLabel)
            {
                var label = card.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    label.text = FormatLegacyOfferText(offer);
                }
            }

            if (hasAnimalImage || offer.Icon == null)
            {
                return;
            }

            var images = card.GetComponentsInChildren<Image>(true);
            foreach (var image in images)
            {
                if (image == null || image.gameObject == card)
                {
                    continue;
                }

                ApplyImage(image, offer.Icon);
                break;
            }
        }

        private T FindSpawnedComponent<T>(GameObject spawnedCard, T templateComponent) where T : Component
        {
            if (spawnedCard == null || templateComponent == null || cardTemplate == null)
            {
                return null;
            }

            var path = GetRelativePath(cardTemplate.transform, templateComponent.transform);
            if (string.IsNullOrEmpty(path))
            {
                return spawnedCard.GetComponent<T>();
            }

            var target = spawnedCard.transform.Find(path);
            return target != null ? target.GetComponent<T>() : null;
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return null;
            }

            if (root == target)
            {
                return string.Empty;
            }

            var names = new List<string>();
            var current = target;
            while (current != null && current != root)
            {
                names.Add(current.name);
                current = current.parent;
            }

            if (current != root)
            {
                return null;
            }

            names.Reverse();
            return string.Join("/", names);
        }

        private static void ApplyImage(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
            image.preserveAspect = true;
        }

        private static string FormatBaseMoney(AnimalData data)
        {
            return data == null ? string.Empty : data.BaseMoney.ToString("+#;-#;0");
        }

        private static string FormatAbilityText(AnimalData data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            var ability = data.Ability;
            if (ability == null)
            {
                return string.IsNullOrWhiteSpace(data.Description) ? "\u65e0\u80fd\u529b" : data.Description;
            }

            var descriptions = new List<string>();
            if (!string.IsNullOrWhiteSpace(ability.Desc))
            {
                descriptions.Add(ability.Desc);
            }

            if (ability.SubAbilities != null)
            {
                foreach (var subAbility in ability.SubAbilities)
                {
                    if (subAbility != null && !string.IsNullOrWhiteSpace(subAbility.Desc))
                    {
                        descriptions.Add(subAbility.Desc);
                    }
                }
            }

            if (descriptions.Count > 0)
            {
                return string.Join("\n", descriptions);
            }

            return string.IsNullOrWhiteSpace(data.Description) ? "\u65e0\u80fd\u529b" : data.Description;
        }

        private static string FormatLegacyOfferText(AnimalData data)
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
