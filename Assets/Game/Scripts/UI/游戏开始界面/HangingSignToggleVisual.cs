using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HangingSignToggleVisual : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private RectTransform sign;
    [SerializeField] private RectTransform rope;

    [Header("Family")]
    [SerializeField] private string family;

    [Header("Selected State")]
    [SerializeField] private float signDownOffset = 14f;
    [SerializeField] private float ropeStretch = 14f;

    [Header("Animation")]
    [SerializeField] private float duration = 0.16f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector2 signBasePos;
    private Vector2 ropeBaseSize;
    private Coroutine animCo;
    private bool hasWarnedEmptyFamily;

    public string Family
    {
        get
        {
            if (string.IsNullOrWhiteSpace(family) && !hasWarnedEmptyFamily)
            {
                hasWarnedEmptyFamily = true;
                Debug.LogWarning($"[HangingSignToggleVisual] {name} 的 family 为空，请在 Inspector 中填写动物家族 ID。", this);
            }

            return family;
        }
    }

    private void Awake()
    {
        if (toggle == null) toggle = GetComponent<Toggle>();
        signBasePos = sign.anchoredPosition;
        ropeBaseSize = rope.sizeDelta;
    }

    private void OnEnable()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
        ApplyInstant(toggle.isOn);
    }

    private void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(PlayAnim(isOn));
    }

    private IEnumerator PlayAnim(bool isOn)
    {
        Vector2 fromSign = sign.anchoredPosition;
        Vector2 toSign = signBasePos + Vector2.down * (isOn ? signDownOffset : 0f);

        Vector2 fromRope = rope.sizeDelta;
        Vector2 toRope = ropeBaseSize + new Vector2(0f, isOn ? ropeStretch : 0f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float k = curve.Evaluate(p);

            sign.anchoredPosition = Vector2.LerpUnclamped(fromSign, toSign, k);
            rope.sizeDelta = Vector2.LerpUnclamped(fromRope, toRope, k);

            yield return null;
        }

        sign.anchoredPosition = toSign;
        rope.sizeDelta = toRope;
        animCo = null;
    }

    private void ApplyInstant(bool isOn)
    {
        sign.anchoredPosition = signBasePos + Vector2.down * (isOn ? signDownOffset : 0f);
        rope.sizeDelta = ropeBaseSize + new Vector2(0f, isOn ? ropeStretch : 0f);
    }
}
