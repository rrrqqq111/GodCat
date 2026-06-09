using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocaleVisibilitySwitcher : MonoBehaviour
{
    [SerializeField] private string chineseLocaleCode = "zh-Hans";
    [SerializeField] private string englishLocaleCode = "en";

    [Header("Visible only in Chinese")]
    [SerializeField] private GameObject[] chineseOnlyObjects;

    [Header("Visible only in English")]
    [SerializeField] private GameObject[] englishOnlyObjects;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        StartCoroutine(RefreshAfterInitialization());
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    private IEnumerator RefreshAfterInitialization()
    {
        yield return LocalizationSettings.InitializationOperation;
        Refresh(LocalizationSettings.SelectedLocale);
    }

    private void OnSelectedLocaleChanged(Locale locale)
    {
        Refresh(locale);
    }

    private void Refresh(Locale locale)
    {
        if (locale == null)
            return;

        string localeCode = locale.Identifier.Code;

        SetObjectsActive(chineseOnlyObjects, localeCode == chineseLocaleCode);
        SetObjectsActive(englishOnlyObjects, localeCode == englishLocaleCode);
    }

    private void SetObjectsActive(GameObject[] objects, bool isActive)
    {
        if (objects == null)
            return;

        foreach (GameObject target in objects)
        {
            if (target != null)
                target.SetActive(isActive);
        }
    }
}
