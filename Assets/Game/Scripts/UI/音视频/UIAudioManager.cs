using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance { get; private set; }

    [Header("共用音频源")]
    [SerializeField] private AudioSource audioSource;

    [Header("默认UI音效")]
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private const string UI_VOLUME_KEY = "UI_VOLUME";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        KeepAudioManagerIfNeeded();

        float savedVolume = PlayerPrefs.GetFloat(UI_VOLUME_KEY, 1f);
        SetVolume(savedVolume);
    }

    public void PlayHover()
    {
        if (audioSource != null && hoverClip != null)
        {
            audioSource.PlayOneShot(hoverClip);
        }
    }

    public void PlayClip(AudioClip clip)
    {
        PlayClip(clip, 1f);
    }

    public void PlayClip(AudioClip clip, float volumeScale)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }
    }

    public void PlayClick()
    {
        if (audioSource != null && clickClip != null)
        {
            audioSource.PlayOneShot(clickClip);
        }
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    public float GetVolume()
    {
        if (audioSource != null)
            return audioSource.volume;

        return 1f;
    }

    private void KeepAudioManagerIfNeeded()
    {
        if (GetComponentInParent<GlobalSettingsMenuController>() != null)
            return;

        if (GetComponent<Canvas>() != null)
        {
            Debug.LogWarning("UIAudioManager 不应直接挂在 Canvas 上，否则会把整个 Canvas 保留到下一个场景。请把它放到全局设置根物体或独立根物体上。");
            return;
        }

        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
            return;
        }

        Debug.LogWarning("UIAudioManager 当前不是根物体，也不在 GlobalSettingsMenuController 管理下。切换场景时它可能会被销毁。");
    }
}
