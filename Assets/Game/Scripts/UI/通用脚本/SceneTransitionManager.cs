using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    private const string ManagerName = "SceneTransitionManager";

    private static SceneTransitionManager instance;

    public static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneTransitionManager>();

                if (instance == null)
                    instance = CreateManagerObject();
            }

            return instance;
        }
    }

    [Header("Canvas")]
    [SerializeField] private int sortingOrder = 32767;

    [Header("Transition")]
    [SerializeField] private Color transitionColor = new Color(0.05f, 0.04f, 0.08f, 1f);
    [SerializeField, Min(2)] private int stripCount = 7;
    [SerializeField, Min(0.05f)] private float coverDuration = 0.65f;
    [SerializeField, Min(0.05f)] private float revealDuration = 0.55f;
    [SerializeField, Min(0f)] private float stripDelay = 0.045f;
    [SerializeField, Min(0f)] private float postLoadDelay = 0.05f;
    [SerializeField] private AnimationCurve coverCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private readonly List<RectTransform> strips = new List<RectTransform>();
    private Canvas transitionCanvas;
    private CanvasGroup canvasGroup;
    private RectTransform stripRoot;
    private bool isTransitioning;
    private bool isProxy;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateDefaultManager()
    {
        if (instance != null)
            return;

        if (FindObjectOfType<SceneTransitionManager>() != null)
            return;

        CreateManagerObject();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            isProxy = true;
            return;
        }

        PromoteToInstance();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        if (ForwardToInstanceIfNeeded(sceneName))
            return;

        if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(sceneName.Trim()))
        {
            Debug.LogError("SceneTransitionManager: 场景名为空，无法切换场景。");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("SceneTransitionManager: 找不到场景或场景未加入 Build Settings: " + sceneName);
            return;
        }

        StartTransition(LoadSceneRoutine(sceneName));
    }

    public void LoadSceneWithTransitionByIndex(int buildIndex)
    {
        if (ForwardToInstanceIfNeeded(buildIndex))
            return;

        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("SceneTransitionManager: Build index 无效: " + buildIndex);
            return;
        }

        StartTransition(LoadSceneRoutine(buildIndex));
    }

    public void ReloadCurrentSceneWithTransition()
    {
        if (ForwardReloadToInstanceIfNeeded())
            return;

        LoadSceneWithTransitionByIndex(SceneManager.GetActiveScene().buildIndex);
    }

    private void StartTransition(IEnumerator loadRoutine)
    {
        if (isTransitioning)
            return;

        StartCoroutine(TransitionRoutine(loadRoutine));
    }

    private IEnumerator TransitionRoutine(IEnumerator loadRoutine)
    {
        isTransitioning = true;
        EnsureCanvas();
        ShowCanvas();

        yield return PlayCover();
        yield return loadRoutine;

        if (postLoadDelay > 0f)
            yield return new WaitForSecondsRealtime(postLoadDelay);

        yield return PlayReveal();
        HideCanvas();
        isTransitioning = false;
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError("SceneTransitionManager: 场景加载失败: " + sceneName);
            yield break;
        }

        while (!operation.isDone)
            yield return null;
    }

    private IEnumerator LoadSceneRoutine(int buildIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(buildIndex);

        if (operation == null)
        {
            Debug.LogError("SceneTransitionManager: 场景加载失败，Build index: " + buildIndex);
            yield break;
        }

        while (!operation.isDone)
            yield return null;
    }

    private void EnsureCanvas()
    {
        if (transitionCanvas != null)
            return;

        GameObject canvasObject = new GameObject("Scene Transition Canvas");
        canvasObject.transform.SetParent(transform, false);

        transitionCanvas = canvasObject.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = sortingOrder;

        canvasObject.AddComponent<GraphicRaycaster>();
        canvasGroup = canvasObject.AddComponent<CanvasGroup>();

        GameObject stripRootObject = new GameObject("Strips");
        stripRootObject.transform.SetParent(canvasObject.transform, false);
        stripRoot = stripRootObject.AddComponent<RectTransform>();
        stripRoot.anchorMin = Vector2.zero;
        stripRoot.anchorMax = Vector2.one;
        stripRoot.offsetMin = Vector2.zero;
        stripRoot.offsetMax = Vector2.zero;

        BuildStrips();
    }

    private void BuildStrips()
    {
        strips.Clear();

        int count = Mathf.Max(2, stripCount);
        float width = 1f / count;

        for (int i = 0; i < count; i++)
        {
            GameObject stripObject = new GameObject("Strip " + (i + 1));
            stripObject.transform.SetParent(stripRoot, false);

            Image image = stripObject.AddComponent<Image>();
            image.color = transitionColor;
            image.raycastTarget = true;

            RectTransform rect = stripObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(width * i, 0f);
            rect.anchorMax = new Vector2(width * (i + 1), 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = new Vector3(1f, 0f, 1f);

            strips.Add(rect);
        }
    }

    private IEnumerator PlayCover()
    {
        SetStripScales(0f);
        yield return PlayStrips(0f, 1f, coverDuration, coverCurve, false);
    }

    private IEnumerator PlayReveal()
    {
        SetStripScales(1f);
        yield return PlayStrips(1f, 0f, revealDuration, revealCurve, true);
    }

    private IEnumerator PlayStrips(float fromScale, float toScale, float duration, AnimationCurve curve, bool reverseOrder)
    {
        duration = Mathf.Max(0.01f, duration);
        float totalDuration = duration + stripDelay * Mathf.Max(0, strips.Count - 1);
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            for (int i = 0; i < strips.Count; i++)
            {
                int orderIndex = reverseOrder ? strips.Count - 1 - i : i;
                float stripElapsed = elapsed - stripDelay * orderIndex;
                float progress = Mathf.Clamp01(stripElapsed / duration);
                float curveValue = curve != null ? curve.Evaluate(progress) : progress;
                float scaleY = Mathf.LerpUnclamped(fromScale, toScale, curveValue);

                strips[i].localScale = new Vector3(1f, scaleY, 1f);
            }

            yield return null;
        }

        SetStripScales(toScale);
    }

    private void SetStripScales(float scaleY)
    {
        foreach (RectTransform strip in strips)
            strip.localScale = new Vector3(1f, scaleY, 1f);
    }

    private void ShowCanvas()
    {
        transitionCanvas.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HideCanvas()
    {
        if (transitionCanvas == null)
            return;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        transitionCanvas.gameObject.SetActive(false);
    }

    private bool ForwardToInstanceIfNeeded(string sceneName)
    {
        if (!isProxy)
            return false;

        if (instance != null && instance != this)
        {
            instance.LoadSceneWithTransition(sceneName);
            return true;
        }

        PromoteToInstance();
        return false;
    }

    private bool ForwardToInstanceIfNeeded(int buildIndex)
    {
        if (!isProxy)
            return false;

        if (instance != null && instance != this)
        {
            instance.LoadSceneWithTransitionByIndex(buildIndex);
            return true;
        }

        PromoteToInstance();
        return false;
    }

    private bool ForwardReloadToInstanceIfNeeded()
    {
        if (!isProxy)
            return false;

        if (instance != null && instance != this)
        {
            instance.ReloadCurrentSceneWithTransition();
            return true;
        }

        PromoteToInstance();
        return false;
    }

    private void PromoteToInstance()
    {
        isProxy = false;
        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureCanvas();
        HideCanvas();
    }

    private static SceneTransitionManager CreateManagerObject()
    {
        GameObject managerObject = new GameObject(ManagerName);
        return managerObject.AddComponent<SceneTransitionManager>();
    }
}
