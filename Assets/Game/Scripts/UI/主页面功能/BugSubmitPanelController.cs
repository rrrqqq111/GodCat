using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class BugSubmitPanelController : MonoBehaviour
{
    [Header("输入框")]
    [SerializeField] private TMP_InputField contactInput;
    [SerializeField] private TMP_InputField bugContentInput;

    [Header("按钮")]
    [SerializeField] private Button submitButton;
    [SerializeField] private Button exitButton;

    [Header("提交地址")]
    [SerializeField] private string submitUrl = "https://你的服务器地址/bug-submit";

    [Header("按钮颜色")]
    [SerializeField] private Color submitEnabledColor = Color.white;
    [SerializeField] private Color submitDisabledColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    private Image submitButtonImage;
    [SerializeField] private UIButtonHover submitHoverEffect;

    [System.Serializable]
    public class BugReportData
    {
        public string contact;
        public string bugContent;
        public string submitTime;
    }

    private void Awake()
    {
        submitButtonImage = submitButton.GetComponent<Image>();

        contactInput.onValueChanged.AddListener(OnInputChanged);
        bugContentInput.onValueChanged.AddListener(OnInputChanged);

        submitButton.onClick.AddListener(OnClickSubmit);
        exitButton.onClick.AddListener(OnClickExit);
    }

    private void OnEnable()
    {
        RefreshSubmitButtonState();
    }

    private void OnInputChanged(string value)
    {
        RefreshSubmitButtonState();
    }

    private void RefreshSubmitButtonState()
    {
        bool hasBugContent = !string.IsNullOrWhiteSpace(bugContentInput.text);

        submitButton.interactable = hasBugContent;

        if (submitButtonImage != null)
        {
            submitButtonImage.color = hasBugContent ? submitEnabledColor : submitDisabledColor;
        }

        if (submitHoverEffect != null)
        {
            submitHoverEffect.enabled = hasBugContent;
        }
    }

    private void OnClickExit()
    {
        ClearInputs();
        gameObject.SetActive(false);
    }

    private void ClearInputs()
    {
        contactInput.text = "";
        bugContentInput.text = "";
        RefreshSubmitButtonState();
    }

    private void OnClickSubmit()
    {
        if (string.IsNullOrWhiteSpace(bugContentInput.text))
            return;

        BugReportData data = new BugReportData
        {
            contact = contactInput.text,
            bugContent = bugContentInput.text,
            submitTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        string json = JsonUtility.ToJson(data, true);
        Debug.Log("提交BUG JSON：\n" + json);

        StartCoroutine(SendBugReport(json));
        ClearInputs();
        gameObject.SetActive(false);
    }

    private IEnumerator SendBugReport(string json)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(submitUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        submitButton.interactable = false;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("BUG提交成功：" + request.downloadHandler.text);

            ClearInputs();
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("BUG提交失败：" + request.error);
            RefreshSubmitButtonState();
        }

        request.Dispose();
    }
}