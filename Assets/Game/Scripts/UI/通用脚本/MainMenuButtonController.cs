using UnityEngine;

public class MainMenuButtonController : MonoBehaviour
{
    [Header("锟斤拷始锟斤拷戏")]
    [SerializeField] private string gameSceneName = "GameStartSence";
    [SerializeField] private string targetSceneName;

    [Header("锟斤拷页锟斤拷锟斤拷")]
    [SerializeField] private string qqGroupUrl = "https://";
    [SerializeField] private string discordUrl = "https://";
    [SerializeField] private string twitterUrl = "https://";
    [SerializeField] private string surveyUrl = "https://";

    [Header("锟斤拷锟斤拷 Panel")]
    [SerializeField] private GameObject Panel_1;
    [SerializeField] private GameObject Panel_2;
    [SerializeField] private GameObject Panel_3;
    [SerializeField] private GameObject Panel_4;
    [SerializeField] private GameObject Panel_5;
    [SerializeField] private GameObject Panel_6;
    [SerializeField] private GameObject Panel_7;
    [SerializeField] public GameObject settingsPanel;

    private void Start()
    {
        CloseAllPanels();
    }

    public void OnClickStartGame()
    {
        SceneTransitionManager.Instance.LoadSceneWithTransition(gameSceneName);
    }

    public void OnClickLoadTargetScene()
    {
        LoadScene(targetSceneName);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Scene name is empty.");
            return;
        }

        SceneTransitionManager.Instance.LoadSceneWithTransition(sceneName.Trim());
    }

    public void OnClickQuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClickQQGroup()
    {
        OpenUrl(qqGroupUrl);
    }
    public void OnClickDiscord()
    {
        OpenUrl(discordUrl);
    }
    public void OnClickTwitter()
    {
        OpenUrl(twitterUrl);
    }

    public void OnClickSurvey()
    {
        OpenUrl(surveyUrl);
    }

    public void OnClickPanel_1()
    {
        OpenPanel(Panel_1);
    }

    public void OnClickPanel_2()
    {
        OpenPanel(Panel_2);
    }

    public void OnClickPanel_3()
    {
        OpenPanel(Panel_3);
    }

    public void OnClickPanel_4()
    {
        OpenPanel(Panel_4);
    }

    public void OnClickPanel_5()
    {
        OpenPanel(Panel_5);
    }

    public void OnClickPanel_6()
    {
        OpenPanel(Panel_6);
    }
    public void OnClickPanel_7()
    {
        OpenPanel(Panel_7);
    }

    public void OnClickSettings()
    {
        if (GlobalSettingsMenuController.Instance != null)
        {
            GlobalSettingsMenuController.Instance.ShowSettingsPanel();
            return;
        }

        OpenPanel(settingsPanel);
    }

    public void CloseAllPanels()
    {
        SetPanelActive(Panel_1, false);
        SetPanelActive(Panel_2, false);
        SetPanelActive(Panel_3, false);
        SetPanelActive(Panel_4, false);
        SetPanelActive(Panel_5, false);
        SetPanelActive(Panel_6, false);
        SetPanelActive(Panel_7, false);
        SetPanelActive(settingsPanel, false);
    }

    public void ClosePanel(GameObject panel)
    {
        SetPanelActive(panel, false);
    }

    private void OpenPanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogWarning("Panel 未锟斤拷");
            return;
        }
        if(panel != settingsPanel)
            CloseAllPanels();
        panel.SetActive(true);
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private void OpenUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("锟斤拷锟斤拷未锟斤拷锟斤拷");
            return;
        }

        Application.OpenURL(url);
    }
}


