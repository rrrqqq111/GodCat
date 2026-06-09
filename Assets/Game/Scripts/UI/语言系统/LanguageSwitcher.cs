using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.UI;

public class LanguageSwitcher : MonoBehaviour
{
    [Header("Linked UI")]
    [SerializeField] private Toggle chineseToggle;
    [SerializeField] private Toggle englishToggle;
    [SerializeField] private TMP_Dropdown languageDropdown;

    [Header("Locale Codes")]
    [SerializeField] private string chineseLocaleCode = "zh-Hans";
    [SerializeField] private string englishLocaleCode = "en";

    private bool isSyncingControls;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

        if (chineseToggle != null)
            chineseToggle.onValueChanged.AddListener(OnChineseToggleChanged);

        if (englishToggle != null)
            englishToggle.onValueChanged.AddListener(OnEnglishToggleChanged);

        if (languageDropdown != null)
            languageDropdown.onValueChanged.AddListener(SwitchByIndex);

        StartCoroutine(SyncControlsAfterInitialization());
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;

        if (chineseToggle != null)
            chineseToggle.onValueChanged.RemoveListener(OnChineseToggleChanged);

        if (englishToggle != null)
            englishToggle.onValueChanged.RemoveListener(OnEnglishToggleChanged);

        if (languageDropdown != null)
            languageDropdown.onValueChanged.RemoveListener(SwitchByIndex);
    }

    public void SwitchToChinese()
    {
        StartCoroutine(SetLocale(chineseLocaleCode));
    }

    public void SwitchToEnglish()
    {
        StartCoroutine(SetLocale(englishLocaleCode));
    }

    public void OnChineseToggleChanged(bool isOn)
    {
        if (!isSyncingControls && isOn)
            SwitchToChinese();
    }

    public void OnEnglishToggleChanged(bool isOn)
    {
        if (!isSyncingControls && isOn)
            SwitchToEnglish();
    }

    // 给 Dropdown 用的入口
    public void SwitchByIndex(int index)
    {
        if (isSyncingControls)
            return;

        switch (index)
        {
            case 0:
                SwitchToChinese();
                break;
            case 1:
                SwitchToEnglish();
                break;
        }
    }

    private IEnumerator SyncControlsAfterInitialization()
    {
        yield return LocalizationSettings.InitializationOperation;
        SyncControls(LocalizationSettings.SelectedLocale);
    }

    private void OnSelectedLocaleChanged(Locale locale)
    {
        SyncControls(locale);
    }

    private void SyncControls(Locale locale)
    {
        if (locale == null)
            return;

        isSyncingControls = true;

        string localeCode = locale.Identifier.Code;
        bool isChinese = localeCode == chineseLocaleCode;
        bool isEnglish = localeCode == englishLocaleCode;

        if (chineseToggle != null)
            chineseToggle.SetIsOnWithoutNotify(isChinese);

        if (englishToggle != null)
            englishToggle.SetIsOnWithoutNotify(isEnglish);

        if (languageDropdown != null)
        {
            if (isChinese)
                languageDropdown.SetValueWithoutNotify(0);
            else if (isEnglish)
                languageDropdown.SetValueWithoutNotify(1);

            languageDropdown.RefreshShownValue();
        }

        isSyncingControls = false;
    }

    private IEnumerator SetLocale(string localeCode)
    {
        yield return LocalizationSettings.InitializationOperation;

        if (LocalizationSettings.SelectedLocale != null &&
            LocalizationSettings.SelectedLocale.Identifier.Code == localeCode)
        {
            SyncControls(LocalizationSettings.SelectedLocale);
            yield break;
        }

        var locales = LocalizationSettings.AvailableLocales.Locales;

        foreach (var locale in locales)
        {
            if (locale.Identifier.Code == localeCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                yield break;
            }
        }

        Debug.LogError("找不到语言: " + localeCode);
    }
}
