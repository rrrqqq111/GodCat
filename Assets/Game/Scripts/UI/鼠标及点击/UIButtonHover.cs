using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler
{
    [Header("缩放设置")]
    [SerializeField] private float hoverScaleMultiplier = 1.1f;
    [SerializeField] private float scaleSpeed = 10f;

    [Header("音效（可选覆盖）")]
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );
    }

    // 鼠标进入
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovering) return;

        isHovering = true;
        targetScale = originalScale * hoverScaleMultiplier;

        if (UIAudioManager.Instance != null)
        {
            if (hoverClip != null)
                UIAudioManager.Instance.PlayClip(hoverClip);
            else
                UIAudioManager.Instance.PlayHover();
        }
    }

    // 鼠标离开
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetScale = originalScale;
    }

    // 鼠标按下时立刻触发
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (UIAudioManager.Instance != null)
        {
            if (clickClip != null)
                UIAudioManager.Instance.PlayClip(clickClip);
            else
                UIAudioManager.Instance.PlayClick();
        }
    }
}