using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NekogamiRanch.UI
{
    public class AnimalBookCategoryToggleSelectionEffect : MonoBehaviour
    {
        [SerializeField, InspectorName("标签根节点")] private Transform toggleRoot;
        [SerializeField, InspectorName("自动收集子标签")] private bool autoCollectChildToggles = true;
        [SerializeField, InspectorName("标签列表")] private List<Toggle> toggles = new List<Toggle>();
        [SerializeField, InspectorName("选中右移距离")] private float selectedOffsetX = 24f;
        [SerializeField, Min(0f), InspectorName("移动动画时长")] private float moveDuration = 0.12f;

        private readonly List<ToggleItem> items = new List<ToggleItem>();
        private bool initialized;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
            RegisterListeners();
            RefreshPositions(true);
        }

        private void OnDisable()
        {
            UnregisterListeners();
            StopAllCoroutines();
            ClearMoveRoutines();
        }

        private void OnTransformChildrenChanged()
        {
            if (!autoCollectChildToggles || !isActiveAndEnabled)
            {
                return;
            }

            RebuildItems();
            RegisterListeners();
            RefreshPositions(true);
        }

        public void Refresh()
        {
            RebuildItems();
            RegisterListeners();
            RefreshPositions(true);
        }

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            if (toggleRoot == null)
            {
                toggleRoot = transform;
            }

            RebuildItems();
            initialized = true;
        }

        private void RebuildItems()
        {
            RestoreOriginalPositions();
            UnregisterListeners();
            items.Clear();

            if (autoCollectChildToggles && toggleRoot != null)
            {
                toggles.Clear();
                toggleRoot.GetComponentsInChildren(true, toggles);
            }

            for (var i = 0; i < toggles.Count; i++)
            {
                var toggle = toggles[i];
                if (toggle == null)
                {
                    continue;
                }

                var rectTransform = toggle.transform as RectTransform;
                if (rectTransform == null)
                {
                    continue;
                }

                items.Add(new ToggleItem(toggle, rectTransform, rectTransform.anchoredPosition));
            }
        }

        private void RegisterListeners()
        {
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
                item.Toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        private void UnregisterListeners()
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Toggle != null)
                {
                    items[i].Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
                }
            }
        }

        private void OnToggleValueChanged(bool _)
        {
            RefreshPositions(false);
        }

        private void RefreshPositions(bool immediate)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var target = item.OriginalAnchoredPosition;
                if (item.Toggle != null && item.Toggle.isOn)
                {
                    target += Vector2.right * selectedOffsetX;
                }

                if (item.MoveRoutine != null)
                {
                    StopCoroutine(item.MoveRoutine);
                    item.MoveRoutine = null;
                }

                if (immediate || moveDuration <= 0f)
                {
                    item.RectTransform.anchoredPosition = target;
                    continue;
                }

                item.MoveRoutine = StartCoroutine(MoveTo(item, target));
            }
        }

        private void RestoreOriginalPositions()
        {
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.RectTransform != null)
                {
                    item.RectTransform.anchoredPosition = item.OriginalAnchoredPosition;
                }
            }
        }

        private void ClearMoveRoutines()
        {
            for (var i = 0; i < items.Count; i++)
            {
                items[i].MoveRoutine = null;
            }
        }

        private IEnumerator MoveTo(ToggleItem item, Vector2 target)
        {
            var start = item.RectTransform.anchoredPosition;
            var elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / moveDuration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                item.RectTransform.anchoredPosition = Vector2.LerpUnclamped(start, target, t);
                yield return null;
            }

            item.RectTransform.anchoredPosition = target;
            item.MoveRoutine = null;
        }

        private sealed class ToggleItem
        {
            public ToggleItem(Toggle toggle, RectTransform rectTransform, Vector2 originalAnchoredPosition)
            {
                Toggle = toggle;
                RectTransform = rectTransform;
                OriginalAnchoredPosition = originalAnchoredPosition;
            }

            public Toggle Toggle { get; }
            public RectTransform RectTransform { get; }
            public Vector2 OriginalAnchoredPosition { get; }
            public Coroutine MoveRoutine { get; set; }
        }
    }
}
