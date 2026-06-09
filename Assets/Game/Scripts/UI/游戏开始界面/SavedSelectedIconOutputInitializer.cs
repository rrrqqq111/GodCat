using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SavedSelectedIconOutputInitializer : MonoBehaviour
{
    [Header("Save")]
    [SerializeField] private string selectedIndicesPlayerPrefsKey = "SelectedOptionIndices";

    [Header("Images")]
    [SerializeField] private List<Image> selectedOutputImages = new List<Image>();
    [SerializeField] private List<Image> optionIconImages = new List<Image>();
    [SerializeField] private bool clearUnusedOutputImages = true;

    [Header("Timing")]
    [SerializeField] private bool refreshOnAwake = true;
    [SerializeField] private bool refreshOnEnable = false;

    private void Awake()
    {
        if (refreshOnAwake)
            RefreshFromSavedSelection();
    }

    private void OnEnable()
    {
        if (refreshOnEnable)
            RefreshFromSavedSelection();
    }

    public void RefreshFromSavedSelection()
    {
        if (selectedOutputImages == null || selectedOutputImages.Count == 0)
            return;

        if (string.IsNullOrEmpty(selectedIndicesPlayerPrefsKey) ||
            !PlayerPrefs.HasKey(selectedIndicesPlayerPrefsKey))
        {
            ClearOutputs();
            return;
        }

        string savedIndices = PlayerPrefs.GetString(selectedIndicesPlayerPrefsKey, string.Empty);
        List<int> selectedIndices = ParseSelectedIndices(savedIndices);
        ApplyIconIndicesToOutputs(selectedIndices);
    }

    private void ApplyIconIndicesToOutputs(List<int> selectedIndices)
    {
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
        if (optionIndex < 0 ||
            optionIconImages == null ||
            optionIndex >= optionIconImages.Count ||
            optionIconImages[optionIndex] == null)
        {
            return null;
        }

        return optionIconImages[optionIndex].sprite;
    }

    private List<int> ParseSelectedIndices(string selectedIndicesText)
    {
        List<int> selectedIndices = new List<int>();

        if (string.IsNullOrEmpty(selectedIndicesText))
            return selectedIndices;

        string[] parts = selectedIndicesText.Split(',');

        foreach (string part in parts)
        {
            if (int.TryParse(part, out int index))
                selectedIndices.Add(index);
        }

        return selectedIndices;
    }

    private void ClearOutputs()
    {
        if (!clearUnusedOutputImages)
            return;

        foreach (Image outputImage in selectedOutputImages)
        {
            if (outputImage == null)
                continue;

            outputImage.sprite = null;
            outputImage.enabled = false;
        }
    }
}
