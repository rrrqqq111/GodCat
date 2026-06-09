using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class LocalizedTMPDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public string tableName = "cn";

    public string[] optionKeys =
    {
        "setting_screen_fullscreen",
        "setting_screen_windowed"
    };

    private void Awake()
    {
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        StartCoroutine(Refresh());
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        StartCoroutine(Refresh());
    }

    private IEnumerator Refresh()
    {
        yield return LocalizationSettings.InitializationOperation;

        var tableHandle = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
        yield return tableHandle;

        StringTable table = tableHandle.Result;
        if (table == null)
        {
            Debug.LogError("找不到本地化表: " + tableName);
            yield break;
        }

        int oldValue = dropdown.value;

        dropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (string key in optionKeys)
        {
            var entry = table.GetEntry(key);
            options.Add(entry != null ? entry.GetLocalizedString() : key);
        }

        dropdown.AddOptions(options);

        dropdown.value = Mathf.Clamp(oldValue, 0, options.Count - 1);
        dropdown.RefreshShownValue();
    }
}