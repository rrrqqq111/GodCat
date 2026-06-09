using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.UI;

public class NChooseKToggleSelector : MonoBehaviour
{
    [Header("Selection")]
    [SerializeField, Min(1)] private int requiredSelectionCount = 3;
    [SerializeField] private List<Toggle> optionToggles = new List<Toggle>();
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text selectedCountTMPText;
    [SerializeField] private Text selectedCountText;

    [Header("Save")]
    [SerializeField] private bool saveSelectionOnConfirm = true;
    [SerializeField] private bool loadSavedSelectionOnEnable = true;
    [SerializeField] private string selectedIndicesPlayerPrefsKey = "SelectedOptionIndices";

    [Header("Selected Icon Output")]
    [SerializeField] private List<Image> selectedOutputImages = new List<Image>();
    [SerializeField] private List<Image> optionIconImages = new List<Image>();
    [SerializeField] private string autoFindIconChildName = "图标";
    [SerializeField] private bool clearUnusedOutputImages = true;

    [Header("Close")]
    [SerializeField] private bool closePanelAfterSubmit = true;
    [SerializeField] private GameObject panelToCloseAfterSubmit;

    [Header("Limit Warning Popup")]
    [FormerlySerializedAs("selectionLimitPanel")]
    [SerializeField] private GameObject selectionLimitPopupPrefab;
    [SerializeField] private Transform selectionLimitPopupParent;
    [FormerlySerializedAs("autoHideWarningAfter")]
    [SerializeField, Min(0.1f)] private float popupLifetime = 2f;
    [SerializeField, Min(0.01f)] private float popupGrowDuration = 0.18f;
    [SerializeField, Min(0f)] private float popupStartScale = 0.65f;
    [SerializeField] private AnimationCurve popupGrowCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Locked Visual")]
    [SerializeField] private bool tintLockedOptions = true;
    [SerializeField] private bool tintAllChildGraphics = true;
    [SerializeField] private Color lockedOptionColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private readonly Dictionary<Toggle, UnityAction<bool>> toggleListeners = new Dictionary<Toggle, UnityAction<bool>>();
    private readonly Dictionary<Graphic, Color> originalGraphicColors = new Dictionary<Graphic, Color>();
    private readonly List<GameObject> spawnedLimitPopups = new List<GameObject>();
    private readonly HashSet<Toggle> temporarilyBlockedToggles = new HashSet<Toggle>();
    private bool isApplyingState;

    private void Awake()
    {
        if (optionToggles.Count == 0)
            GetComponentsInChildren(true, optionToggles);

        if (selectionLimitPopupPrefab != null && selectionLimitPopupPrefab.scene.IsValid())
            selectionLimitPopupPrefab.SetActive(false);

        RegisterClickBlockers();
        CacheOriginalGraphicColors();
    }

    private void OnEnable()
    {
        foreach (Toggle toggle in optionToggles)
        {
            if (toggle != null)
            {
                UnityAction<bool> listener = isOn => OnToggleValueChanged(toggle, isOn);
                toggleListeners[toggle] = listener;
                toggle.onValueChanged.AddListener(listener);
            }
        }

        HideSelectionLimitPanel();

        if (confirmButton != null && saveSelectionOnConfirm)
            confirmButton.onClick.AddListener(SubmitCurrentSelection);

        if (loadSavedSelectionOnEnable)
            LoadSavedSelection();

        RefreshState();
    }

    private void OnDisable()
    {
        foreach (var pair in toggleListeners)
        {
            if (pair.Key != null)
                pair.Key.onValueChanged.RemoveListener(pair.Value);
        }

        toggleListeners.Clear();
        ClearSpawnedLimitPopups();
        RestoreTemporarilyBlockedToggles();

        if (confirmButton != null && saveSelectionOnConfirm)
            confirmButton.onClick.RemoveListener(SubmitCurrentSelection);
    }

    private void OnToggleValueChanged(Toggle changedToggle, bool isOn)
    {
        if (isApplyingState)
            return;

        if (isOn && GetSelectedCount() > requiredSelectionCount)
        {
            isApplyingState = true;
            changedToggle.isOn = false;
            isApplyingState = false;

            ShowSelectionLimitPanel();
            RefreshState();
            return;
        }

        RefreshState();
    }

    public void HideSelectionLimitPanel()
    {
        ClearSpawnedLimitPopups();
    }

    public bool ShouldBlockToggleClick(Toggle toggle)
    {
        return toggle != null &&
               !toggle.isOn &&
               GetSelectedCount() >= requiredSelectionCount;
    }

    public void BlockToggleClick(Toggle toggle)
    {
        if (toggle == null)
            return;

        ShowSelectionLimitPanel();
        temporarilyBlockedToggles.Add(toggle);
        toggle.interactable = false;
    }

    public void ReleaseBlockedToggleClick(Toggle toggle)
    {
        if (toggle == null || !temporarilyBlockedToggles.Contains(toggle))
            return;

        StartCoroutine(EnableToggleAfterCurrentClick(toggle));
    }

    public void SubmitCurrentSelection()
    {
        int selectedCount = GetSelectedCount();

        if (selectedCount != requiredSelectionCount)
        {
            Debug.LogWarning("NChooseKToggleSelector: 当前选择数量不是 " + requiredSelectionCount + "，不会提交。");
            return;
        }

        string selectedIndices = GetSelectedIndicesText();
        PlayerPrefs.SetString(selectedIndicesPlayerPrefsKey, selectedIndices);
        PlayerPrefs.Save();

        ApplySelectedIconsToOutputs();

        if (closePanelAfterSubmit)
            ClosePanelAfterSubmit();
    }

    public void LoadSavedSelection()
    {
        if (string.IsNullOrEmpty(selectedIndicesPlayerPrefsKey) ||
            !PlayerPrefs.HasKey(selectedIndicesPlayerPrefsKey))
        {
            return;
        }

        string savedIndices = PlayerPrefs.GetString(selectedIndicesPlayerPrefsKey, string.Empty);
        HashSet<int> selectedIndexSet = ParseSelectedIndices(savedIndices);

        isApplyingState = true;

        for (int i = 0; i < optionToggles.Count; i++)
        {
            Toggle toggle = optionToggles[i];
            if (toggle != null)
                toggle.isOn = selectedIndexSet.Contains(i);
        }

        isApplyingState = false;
        ApplySelectedIconsToOutputs();
    }

    private void RefreshState()
    {
        isApplyingState = true;

        int selectedCount = GetSelectedCount();

        // Defensive cleanup in case the initial state has more than the allowed count.
        if (selectedCount > requiredSelectionCount)
        {
            for (int i = optionToggles.Count - 1; i >= 0 && selectedCount > requiredSelectionCount; i--)
            {
                Toggle toggle = optionToggles[i];
                if (toggle != null && toggle.isOn)
                {
                    toggle.isOn = false;
                    selectedCount--;
                }
            }
        }

        bool reachedLimit = selectedCount >= requiredSelectionCount;

        foreach (Toggle toggle in optionToggles)
        {
            if (toggle == null)
                continue;

            toggle.interactable = !temporarilyBlockedToggles.Contains(toggle);
            ApplyLockedVisual(toggle, reachedLimit && !toggle.isOn);
        }

        if (confirmButton != null)
            confirmButton.interactable = selectedCount == requiredSelectionCount;

        UpdateSelectedCountText(selectedCount);

        isApplyingState = false;
    }

    private int GetSelectedCount()
    {
        int count = 0;

        foreach (Toggle toggle in optionToggles)
        {
            if (toggle != null && toggle.isOn)
                count++;
        }

        return count;
    }

    private System.Collections.IEnumerator EnableToggleAfterCurrentClick(Toggle toggle)
    {
        yield return new WaitForEndOfFrame();

        if (toggle != null)
        {
            temporarilyBlockedToggles.Remove(toggle);
            toggle.interactable = true;
            RefreshState();
        }
    }

    private string GetSelectedIndicesText()
    {
        List<string> selectedIndices = new List<string>();

        for (int i = 0; i < optionToggles.Count; i++)
        {
            Toggle toggle = optionToggles[i];
            if (toggle != null && toggle.isOn)
                selectedIndices.Add(i.ToString());
        }

        return string.Join(",", selectedIndices);
    }

    private List<int> GetSelectedIndices()
    {
        List<int> selectedIndices = new List<int>();

        for (int i = 0; i < optionToggles.Count; i++)
        {
            Toggle toggle = optionToggles[i];
            if (toggle != null && toggle.isOn)
                selectedIndices.Add(i);
        }

        return selectedIndices;
    }

    private HashSet<int> ParseSelectedIndices(string selectedIndices)
    {
        HashSet<int> result = new HashSet<int>();

        if (string.IsNullOrEmpty(selectedIndices))
            return result;

        string[] parts = selectedIndices.Split(',');

        foreach (string part in parts)
        {
            if (int.TryParse(part, out int index) && index >= 0 && index < optionToggles.Count)
                result.Add(index);
        }

        return result;
    }

    private void ApplySelectedIconsToOutputs()
    {
        if (selectedOutputImages == null || selectedOutputImages.Count == 0)
            return;

        List<int> selectedIndices = GetSelectedIndices();
        ApplyIconIndicesToOutputs(selectedIndices);
    }

    private void ApplyIconIndicesToOutputs(List<int> selectedIndices)
    {
        if (selectedOutputImages == null || selectedOutputImages.Count == 0)
            return;

        for (int outputIndex = 0; outputIndex < selectedOutputImages.Count; outputIndex++)
        {
            Image outputImage = selectedOutputImages[outputIndex];
            if (outputImage == null)
                continue;

            if (outputIndex < selectedIndices.Count)
            {
                Sprite selectedSprite = GetOptionIconSprite(selectedIndices[outputIndex]);
                outputImage.sprite = selectedSprite;
                outputImage.enabled = selectedSprite != null;
            }
            else if (clearUnusedOutputImages)
            {
                outputImage.sprite = null;
                outputImage.enabled = false;
            }
        }
    }

    private Sprite GetOptionIconSprite(int optionIndex)
    {
        Image iconImage = GetOptionIconImage(optionIndex);
        return iconImage != null ? iconImage.sprite : null;
    }

    private Image GetOptionIconImage(int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= optionToggles.Count)
            return null;

        if (optionIconImages != null &&
            optionIndex < optionIconImages.Count &&
            optionIconImages[optionIndex] != null)
        {
            return optionIconImages[optionIndex];
        }

        Toggle toggle = optionToggles[optionIndex];
        if (toggle == null)
            return null;

        Image namedIcon = FindChildImageByName(toggle.transform, autoFindIconChildName);
        if (namedIcon != null)
            return namedIcon;

        Image[] childImages = toggle.GetComponentsInChildren<Image>(true);
        foreach (Image image in childImages)
        {
            if (image != null && image.sprite != null && image != toggle.targetGraphic)
                return image;
        }

        return null;
    }

    private Image FindChildImageByName(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName))
            return null;

        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image != null && image.name == childName)
                return image;
        }

        return null;
    }

    private void ShowSelectionLimitPanel()
    {
        if (selectionLimitPopupPrefab == null)
            return;

        Transform parent = selectionLimitPopupParent != null
            ? selectionLimitPopupParent
            : selectionLimitPopupPrefab.transform.parent;

        if (parent == null)
            parent = transform;

        GameObject popup = Instantiate(selectionLimitPopupPrefab, parent);
        popup.SetActive(true);
        spawnedLimitPopups.Add(popup);

        StartCoroutine(PlayPopupAndDestroy(popup));
    }

    private System.Collections.IEnumerator PlayPopupAndDestroy(GameObject popup)
    {
        if (popup == null)
            yield break;

        Transform popupTransform = popup.transform;
        Vector3 targetScale = popupTransform.localScale;
        Vector3 startScale = targetScale * popupStartScale;
        float elapsed = 0f;

        popupTransform.localScale = startScale;

        while (elapsed < popupGrowDuration && popup != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / popupGrowDuration);
            float curveValue = popupGrowCurve != null ? popupGrowCurve.Evaluate(progress) : progress;
            popupTransform.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);
            yield return null;
        }

        if (popup == null)
            yield break;

        popupTransform.localScale = targetScale;

        float remainingLifetime = Mathf.Max(0f, popupLifetime - popupGrowDuration);
        if (remainingLifetime > 0f)
            yield return new WaitForSecondsRealtime(remainingLifetime);

        spawnedLimitPopups.Remove(popup);
        Destroy(popup);
    }

    private void CacheOriginalGraphicColors()
    {
        originalGraphicColors.Clear();

        foreach (Toggle toggle in optionToggles)
        {
            if (toggle == null)
                continue;

            foreach (Graphic graphic in GetTintTargets(toggle))
            {
                if (graphic != null && !originalGraphicColors.ContainsKey(graphic))
                    originalGraphicColors.Add(graphic, graphic.color);
            }
        }
    }

    private void ApplyLockedVisual(Toggle toggle, bool isLocked)
    {
        if (!tintLockedOptions)
            return;

        foreach (Graphic graphic in GetTintTargets(toggle))
        {
            if (graphic == null)
                continue;

            if (isLocked)
            {
                graphic.color = lockedOptionColor;
            }
            else if (originalGraphicColors.TryGetValue(graphic, out Color originalColor))
            {
                graphic.color = originalColor;
            }
        }
    }

    private Graphic[] GetTintTargets(Toggle toggle)
    {
        if (tintAllChildGraphics)
            return toggle.GetComponentsInChildren<Graphic>(true);

        return toggle.targetGraphic != null ? new[] { toggle.targetGraphic } : new Graphic[0];
    }

    private void UpdateSelectedCountText(int selectedCount)
    {
        string countText = selectedCount + "/" + requiredSelectionCount;

        if (selectedCountTMPText != null)
            selectedCountTMPText.text = countText;

        if (selectedCountText != null)
            selectedCountText.text = countText;
    }

    private void ClearSpawnedLimitPopups()
    {
        for (int i = spawnedLimitPopups.Count - 1; i >= 0; i--)
        {
            GameObject popup = spawnedLimitPopups[i];
            if (popup != null)
                Destroy(popup);
        }

        spawnedLimitPopups.Clear();
    }

    private void ClosePanelAfterSubmit()
    {
        GameObject targetPanel = panelToCloseAfterSubmit != null
            ? panelToCloseAfterSubmit
            : gameObject;

        targetPanel.SetActive(false);
    }

    private void RestoreTemporarilyBlockedToggles()
    {
        foreach (Toggle toggle in temporarilyBlockedToggles)
        {
            if (toggle != null)
                toggle.interactable = true;
        }

        temporarilyBlockedToggles.Clear();
    }

    private void RegisterClickBlockers()
    {
        foreach (Toggle toggle in optionToggles)
        {
            if (toggle == null)
                continue;

            NChooseKToggleClickBlocker blocker = toggle.GetComponent<NChooseKToggleClickBlocker>();
            if (blocker == null)
                blocker = toggle.gameObject.AddComponent<NChooseKToggleClickBlocker>();

            blocker.Initialize(this, toggle);
        }
    }
}

class NChooseKToggleClickBlocker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private NChooseKToggleSelector selector;
    private Toggle toggle;
    private bool blockedCurrentClick;

    public void Initialize(NChooseKToggleSelector owner, Toggle targetToggle)
    {
        selector = owner;
        toggle = targetToggle;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selector != null && selector.ShouldBlockToggleClick(toggle))
        {
            blockedCurrentClick = true;
            selector.BlockToggleClick(toggle);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!blockedCurrentClick)
            return;

        blockedCurrentClick = false;

        if (selector != null)
            selector.ReleaseBlockedToggleClick(toggle);
    }
}
