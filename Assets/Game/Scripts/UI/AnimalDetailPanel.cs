using System.Collections;
using System.Collections.Generic;
using NekogamiRanch.Animals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NekogamiRanch.UI
{
    public class AnimalDetailPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Image animalIconImage;
        [SerializeField] private TMP_Text baseMoneyText;
        [SerializeField] private TMP_Text abilityText;
        [SerializeField] private TMP_Text animalNameText;
        [SerializeField] private Image familyIconImage;
        [SerializeField] private float hiddenOffsetX = 420f;
        [SerializeField, Min(0f)] private float slideDuration = 0.18f;
        [SerializeField] private bool hideOnStart = true;

        private Vector2 shownAnchoredPosition;
        private Vector2 hiddenAnchoredPosition;
        private Coroutine slideRoutine;
        private bool initialized;
        private bool isShowing;
        private int lastShownFrame = -1;

        public bool IsShowing => isShowing;
        public int LastShownFrame => lastShownFrame;

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            if (hideOnStart)
            {
                SetVisible(false, true);
            }
        }

        public void Refresh(Animal animal)
        {
            if (animal == null || animal.Data == null)
            {
                SetVisible(false);
                return;
            }

            ApplyAnimal(animal);
            SetVisible(true);
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            if (panelRoot == null)
            {
                panelRoot = transform as RectTransform;
            }

            if (panelRoot != null)
            {
                shownAnchoredPosition = panelRoot.anchoredPosition;
                hiddenAnchoredPosition = shownAnchoredPosition + Vector2.right * hiddenOffsetX;
            }

            initialized = true;
        }

        private void ApplyAnimal(Animal animal)
        {
            var data = animal.Data;

            if (animalNameText != null)
            {
                animalNameText.text = data.DisplayName;
            }

            if (baseMoneyText != null)
            {
                baseMoneyText.text = animal.BaseMoney.ToString("+#;-#;0");
            }

            if (abilityText != null)
            {
                abilityText.text = FormatAbilityText(data);
            }

            ApplyImage(animalIconImage, data.Icon);
            ApplyImage(familyIconImage, data.FamilyIcon);
        }

        private void SetVisible(bool visible, bool immediate = false)
        {
            Initialize();
            if (panelRoot == null)
            {
                gameObject.SetActive(visible);
                return;
            }

            if (slideRoutine != null)
            {
                StopCoroutine(slideRoutine);
                slideRoutine = null;
            }

            if (visible && !isShowing)
            {
                lastShownFrame = Time.frameCount;
            }

            isShowing = visible;
            gameObject.SetActive(true);

            var target = visible ? shownAnchoredPosition : hiddenAnchoredPosition;
            if (immediate || slideDuration <= 0f)
            {
                panelRoot.anchoredPosition = target;
                if (!visible)
                {
                    gameObject.SetActive(false);
                }

                return;
            }

            slideRoutine = StartCoroutine(SlideTo(target, visible));
        }

        private IEnumerator SlideTo(Vector2 target, bool visible)
        {
            var start = panelRoot.anchoredPosition;
            var elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / slideDuration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                panelRoot.anchoredPosition = Vector2.LerpUnclamped(start, target, t);
                yield return null;
            }

            panelRoot.anchoredPosition = target;
            slideRoutine = null;
            if (!visible && !isShowing)
            {
                gameObject.SetActive(false);
            }
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
    }
}
