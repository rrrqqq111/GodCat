using UnityEngine;

public class GlobalSettingsMenuController : MonoBehaviour
{
    public static GlobalSettingsMenuController Instance { get; private set; }

    [Header("全局保留根物体")]
    [SerializeField] private GameObject persistentRoot;

    [Header("设置面板")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private bool hidePanelOnStart = true;

    [Header("快捷键")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    [Header("时间控制")]
    [SerializeField] private bool pauseTimeScaleWhenOpen;

    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        KeepRootAcrossScenes();

        if (hidePanelOnStart && settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleSettingsPanel();
    }

    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("GlobalSettingsMenuController: SettingsPanel 未绑定。");
            return;
        }

        SetSettingsPanelVisible(!settingsPanel.activeSelf);
    }

    public void ShowSettingsPanel()
    {
        SetSettingsPanelVisible(true);
    }

    public void HideSettingsPanel()
    {
        SetSettingsPanelVisible(false);
    }

    public bool IsSettingsPanelOpen()
    {
        return settingsPanel != null && settingsPanel.activeSelf;
    }

    public void SetSettingsPanel(GameObject panel)
    {
        settingsPanel = panel;
    }

    private void SetSettingsPanelVisible(bool visible)
    {
        if (settingsPanel == null)
            return;

        if (settingsPanel.activeSelf == visible)
            return;

        if (visible && pauseTimeScaleWhenOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else if (!visible && pauseTimeScaleWhenOpen)
        {
            Time.timeScale = previousTimeScale;
        }

        settingsPanel.SetActive(visible);
    }

    private void KeepRootAcrossScenes()
    {
        GameObject root = persistentRoot != null ? persistentRoot : gameObject;

        if (root.transform.parent != null)
            root.transform.SetParent(null);

        DontDestroyOnLoad(root);
    }
}
