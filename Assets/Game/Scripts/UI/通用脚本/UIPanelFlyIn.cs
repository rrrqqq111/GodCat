using System.Collections;
using UnityEngine;

public class UIPanelFlyIn : MonoBehaviour
{
    public enum FlyDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }

    [Header("飞入方向")]
    [SerializeField] private FlyDirection flyFrom = FlyDirection.Bottom;

    [Header("画面外偏移距离")]
    [SerializeField] private float outsideOffset = 1200f;

    [Header("飞入时长")]
    [SerializeField] private float duration = 0.35f;

    [Header("移动曲线")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;
    private Vector2 targetPosition;
    private Coroutine flyCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetPosition = rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        PlayFlyIn();
    }

    public void PlayFlyIn()
    {
        if (flyCoroutine != null)
            StopCoroutine(flyCoroutine);

        targetPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = GetOutsidePosition(targetPosition);

        flyCoroutine = StartCoroutine(FlyTo(targetPosition));
    }

    public void PlayFlyOut()
    {
        if (flyCoroutine != null)
            StopCoroutine(flyCoroutine);

        flyCoroutine = StartCoroutine(FlyTo(GetOutsidePosition(targetPosition)));
    }

    private IEnumerator FlyTo(Vector2 endPosition)
    {
        Vector2 startPosition = rectTransform.anchoredPosition;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float curveT = moveCurve.Evaluate(t);

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, curveT);
            yield return null;
        }

        rectTransform.anchoredPosition = endPosition;
        flyCoroutine = null;
    }

    private Vector2 GetOutsidePosition(Vector2 basePosition)
    {
        switch (flyFrom)
        {
            case FlyDirection.Left:
                return basePosition + Vector2.left * outsideOffset;

            case FlyDirection.Right:
                return basePosition + Vector2.right * outsideOffset;

            case FlyDirection.Top:
                return basePosition + Vector2.up * outsideOffset;

            case FlyDirection.Bottom:
                return basePosition + Vector2.down * outsideOffset;

            default:
                return basePosition;
        }
    }
}