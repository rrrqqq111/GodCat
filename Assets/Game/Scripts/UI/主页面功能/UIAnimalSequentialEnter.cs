using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimalSequentialEnter : MonoBehaviour
{
    public enum EnterDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }

    [System.Serializable]
    public class AnimalItem
    {
        [Header("动物UI对象")]
        public RectTransform rect;

        [Header("进入方向")]
        public EnterDirection enterDirection = EnterDirection.Left;

        [Header("画面内目标位置")]
        public Vector2 targetAnchoredPos;

        [Header("画面外偏移距离")]
        public float outsideOffset = 300f;

        [Header("到达目标位置时播放的音效")]
        public AudioClip arriveClip;

        [HideInInspector] public Coroutine runningCoroutine;
    }

    [Header("动物列表")]
    public List<AnimalItem> animals = new List<AnimalItem>();

    [Header("依次进入的间隔")]
    public float interval = 0.2f;

    [Header("单个动物移动时长")]
    public float moveDuration = 0.5f;

    [Header("移动曲线")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("播放时自动重置到画面外")]
    public bool resetBeforePlay = true;

    private Coroutine playCoroutine;

    public void Play()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
        }

        playCoroutine = StartCoroutine(PlaySequence());
    }

    public void ResetAnimalsToOutside()
    {
        for (int i = 0; i < animals.Count; i++)
        {
            AnimalItem item = animals[i];
            if (item == null || item.rect == null) continue;

            item.rect.anchoredPosition = GetOutsidePosition(item);
        }
    }

    public void SnapAnimalsToTarget()
    {
        for (int i = 0; i < animals.Count; i++)
        {
            AnimalItem item = animals[i];
            if (item == null || item.rect == null) continue;

            item.rect.anchoredPosition = item.targetAnchoredPos;
        }
    }

    private IEnumerator PlaySequence()
    {
        if (resetBeforePlay)
        {
            ResetAnimalsToOutside();
        }

        for (int i = 0; i < animals.Count; i++)
        {
            AnimalItem item = animals[i];
            if (item == null || item.rect == null) continue;

            if (item.runningCoroutine != null)
            {
                StopCoroutine(item.runningCoroutine);
            }

            item.runningCoroutine = StartCoroutine(MoveToTarget(item));

            yield return new WaitForSeconds(interval);
        }

        playCoroutine = null;
    }

    private IEnumerator MoveToTarget(AnimalItem item)
    {
        Vector2 startPos = item.rect.anchoredPosition;
        Vector2 endPos = item.targetAnchoredPos;

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / moveDuration);
            float curveT = moveCurve.Evaluate(t);

            item.rect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveT);
            yield return null;
        }

        item.rect.anchoredPosition = endPos;
        item.runningCoroutine = null;

        // 到达目标点时播放音效，共用 UIAudioManager 的 AudioSource
        if (UIAudioManager.Instance != null && item.arriveClip != null)
        {
            UIAudioManager.Instance.PlayClip(item.arriveClip);
        }
    }

    private Vector2 GetOutsidePosition(AnimalItem item)
    {
        Vector2 target = item.targetAnchoredPos;

        switch (item.enterDirection)
        {
            case EnterDirection.Left:
                return target + Vector2.left * item.outsideOffset;
            case EnterDirection.Right:
                return target + Vector2.right * item.outsideOffset;
            case EnterDirection.Top:
                return target + Vector2.up * item.outsideOffset;
            case EnterDirection.Bottom:
                return target + Vector2.down * item.outsideOffset;
            default:
                return target;
        }
    }

    [ContextMenu("记录当前动物位置为目标位置")]
    public void RecordCurrentAsTarget()
    {
        for (int i = 0; i < animals.Count; i++)
        {
            AnimalItem item = animals[i];
            if (item == null || item.rect == null) continue;

            item.targetAnchoredPos = item.rect.anchoredPosition;
        }
    }
    public void PlayExitAll()
    {
        // 停止整体入场流程，防止后续动物继续飞入
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        for (int i = 0; i < animals.Count; i++)
        {
            AnimalItem item = animals[i];
            if (item == null || item.rect == null) continue;

            // 停止该动物当前正在执行的飞入/飞出动画
            if (item.runningCoroutine != null)
            {
                StopCoroutine(item.runningCoroutine);
                item.runningCoroutine = null;
            }

            // 从当前位置飞出
            item.runningCoroutine = StartCoroutine(MoveToOutside(item));
        }
    }
    private IEnumerator MoveToOutside(AnimalItem item)
    {
        Vector2 startPos = item.rect.anchoredPosition;
        Vector2 endPos = GetOutsidePosition(item);

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / moveDuration);
            float curveT = moveCurve.Evaluate(t);

            item.rect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveT);
            yield return null;
        }

        item.rect.anchoredPosition = endPos;
        item.runningCoroutine = null;
    }
}