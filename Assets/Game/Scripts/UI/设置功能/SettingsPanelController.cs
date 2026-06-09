using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelController : MonoBehaviour
{
    [Header("����������")]
    [SerializeField] private Slider uiAudioSlider;
    [SerializeField] private Slider bgmAudioSlider;

    [Header("�������� AudioSource")]
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("Ĭ������")]
    [SerializeField] private float defaultUIVolume = 1f;
    [SerializeField] private float defaultBGMVolume = 1f;

    [Header("���Ƥ������")]
    [SerializeField] private Toggle cursorSkinToggle;
    [SerializeField] private ChangeCursor changeCursor;

    [Header("��������")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;

    private const string RESOLUTION_KEY = "RESOLUTION_OPTION";
    private const string SCREEN_MODE_KEY = "SCREEN_MODE_OPTION";
    private const string UI_VOLUME_KEY = "UI_VOLUME";
    private const string BGM_VOLUME_KEY = "BGM_VOLUME";

    private void Awake()
    {
        InitSliders();
        InitCursorToggle();
        InitDisplaySettings();
    }

    private void InitSliders()
    {
        float uiVolume = PlayerPrefs.GetFloat(UI_VOLUME_KEY, defaultUIVolume);
        float bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultBGMVolume);

        ApplyUIVolume(uiVolume);
        ApplyBGMVolume(bgmVolume);

        if (uiAudioSlider != null)
        {
            uiAudioSlider.minValue = 0f;
            uiAudioSlider.maxValue = 1f;
            uiAudioSlider.SetValueWithoutNotify(uiVolume);
            uiAudioSlider.onValueChanged.AddListener(OnUIVolumeChanged);
        }

        if (bgmAudioSlider != null)
        {
            bgmAudioSlider.minValue = 0f;
            bgmAudioSlider.maxValue = 1f;
            bgmAudioSlider.SetValueWithoutNotify(bgmVolume);
            bgmAudioSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }
    }


    private void OnUIVolumeChanged(float value)
    {
        ApplyUIVolume(value);
        PlayerPrefs.SetFloat(UI_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    private void OnBGMVolumeChanged(float value)
    {
        ApplyBGMVolume(value);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    private void ApplyUIVolume(float value)
    {
        if (UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.SetVolume(value);
        }
    }

    private void ApplyBGMVolume(float value)
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = value;
        }
    }

    public void ResetAudioSettings()
    {
        if (uiAudioSlider != null)
            uiAudioSlider.value = defaultUIVolume;

        if (bgmAudioSlider != null)
            bgmAudioSlider.value = defaultBGMVolume;

        PlayerPrefs.SetFloat(UI_VOLUME_KEY, defaultUIVolume);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, defaultBGMVolume);
        PlayerPrefs.Save();
    }
    private void InitCursorToggle()
    {
        if (cursorSkinToggle == null || changeCursor == null)
            return;

        bool enabled = changeCursor.IsCursorSkinEnabled();

        cursorSkinToggle.SetIsOnWithoutNotify(enabled);
        cursorSkinToggle.onValueChanged.AddListener(OnCursorSkinToggleChanged);

        changeCursor.SetCursorSkinEnabled(enabled);
    }
    private void OnCursorSkinToggleChanged(bool isOn)
    {
        if (changeCursor != null)
        {
            changeCursor.SetCursorSkinEnabled(isOn);
        }
    }
    private void InitDisplaySettings()
    {
        int savedResolution = PlayerPrefs.GetInt(RESOLUTION_KEY, 8);
        int savedScreenMode = PlayerPrefs.GetInt(SCREEN_MODE_KEY, 0);

        if (resolutionDropdown != null)
        {
            resolutionDropdown.SetValueWithoutNotify(savedResolution);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        if (screenModeDropdown != null)
        {
            screenModeDropdown.SetValueWithoutNotify(savedScreenMode);
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        }

        ApplyDisplaySettings(savedResolution, savedScreenMode);
    }

    private void OnResolutionChanged(int index)
    {
        PlayerPrefs.SetInt(RESOLUTION_KEY, index);
        PlayerPrefs.Save();

        int screenMode = screenModeDropdown != null ? screenModeDropdown.value : PlayerPrefs.GetInt(SCREEN_MODE_KEY, 0);
        ApplyDisplaySettings(index, screenMode);
    }

    private void OnScreenModeChanged(int index)
    {
        PlayerPrefs.SetInt(SCREEN_MODE_KEY, index);
        PlayerPrefs.Save();

        int resolution = resolutionDropdown != null ? resolutionDropdown.value : PlayerPrefs.GetInt(RESOLUTION_KEY, 0);
        ApplyDisplaySettings(resolution, index);
    }

    private void ApplyDisplaySettings(int resolutionIndex, int screenModeIndex)
    {
        int width;
        int height;

        switch (resolutionIndex)
        {
            case 0:
                width = 640;
                height = 480;
                break;

            case 1:
                width = 800;
                height = 600;
                break;

            case 2:
                width = 1024;
                height = 576;
                break;

            case 3:
                width = 1280;
                height = 720;
                break;

            case 4:
                width = 1280;
                height = 768;
                break;

            case 5:
                width = 1440;
                height = 900;
                break;

            case 6:
                width = 1600;
                height = 900;
                break;

            case 7:
                width = 1680;
                height = 1050;
                break;

            case 8:
                width = 1920;
                height = 1080;
                break;

            default:
                width = 1920;
                height = 1080;
                break;
        }

        FullScreenMode mode;

        switch (screenModeIndex)
        {
            case 0:
                mode = FullScreenMode.FullScreenWindow;
                break;

            case 1:
                mode = FullScreenMode.Windowed;
                break;

            default:
                mode = FullScreenMode.FullScreenWindow;
                break;
        }

        Screen.SetResolution(width, height, mode);
    }


}