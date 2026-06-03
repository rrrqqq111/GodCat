using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NekogamiRanch.Presentation
{
    public class RanchAnimationDirector : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField] private Transform ranchGate;
        [SerializeField] private Transform leftGate;
        [SerializeField] private Transform rightGate;
        [SerializeField] private Transform animalEnterPoint;
        [SerializeField, Range(0f, 1f)] private float fadeAlpha = 0.65f;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.35f;
        [SerializeField, Min(0f)] private float nightHoldDuration = 0.18f;
        [SerializeField, Min(0.01f)] private float gateStepDuration = 0.18f;

        public Transform AnimalEnterPoint => animalEnterPoint != null ? animalEnterPoint : ranchGate;

        public IEnumerator PlayDayTransition()
        {
            EnsureFadeOverlay();
            if (fadeOverlay == null)
            {
                yield break;
            }

            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.blocksRaycasts = true;
            yield return FadeOverlay(0f, fadeAlpha, fadeDuration);

            if (nightHoldDuration > 0f)
            {
                yield return new WaitForSeconds(nightHoldDuration);
            }
        }

        public IEnumerator PlayGateSequence()
        {
            ResolveGatePanels();
            if (leftGate != null && rightGate != null)
            {
                yield return PlayDoubleGateSequence();
                yield break;
            }

            yield return PlaySingleGateFallback();
        }

        private IEnumerator PlayDoubleGateSequence()
        {
            var leftOpen = leftGate.localPosition;
            var rightOpen = rightGate.localPosition;
            var gap = Mathf.Abs(rightOpen.x - leftOpen.x);
            var closeOffset = gap * 0.3f;
            var leftClosed = leftOpen + Vector3.right * closeOffset;
            var rightClosed = rightOpen + Vector3.left * closeOffset;

            yield return MoveGatePanels(leftOpen, leftClosed, rightOpen, rightClosed, gateStepDuration);
            yield return OpenGateWithDaylight(leftClosed, leftOpen, rightClosed, rightOpen);
        }

        private IEnumerator PlaySingleGateFallback()
        {
            if (ranchGate == null)
            {
                ranchGate = FindGateTransform();
            }

            if (ranchGate == null)
            {
                Debug.Log("[RanchAnimationDirector] Ranch gate panels are not assigned. Skipping placeholder gate animation.");
                yield return new WaitForSeconds(gateStepDuration * 2f);
                yield return FadeOutOverlay();
                yield break;
            }

            var origin = ranchGate.localPosition;
            var closedPosition = origin + Vector3.down * 0.35f;

            yield return MoveLocal(ranchGate, origin, closedPosition, gateStepDuration);
            yield return OpenSingleGateWithDaylight(closedPosition, origin);
        }

        private IEnumerator OpenGateWithDaylight(Vector3 leftClosed, Vector3 leftOpen, Vector3 rightClosed, Vector3 rightOpen)
        {
            var duration = Mathf.Max(gateStepDuration, fadeDuration);
            var startAlpha = fadeOverlay != null ? fadeOverlay.alpha : 0f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var gateT = Mathf.Clamp01(elapsed / gateStepDuration);
                var fadeT = Mathf.Clamp01(elapsed / fadeDuration);
                leftGate.localPosition = Vector3.Lerp(leftClosed, leftOpen, gateT);
                rightGate.localPosition = Vector3.Lerp(rightClosed, rightOpen, gateT);
                SetFadeAlpha(Mathf.Lerp(startAlpha, 0f, fadeT));
                yield return null;
            }

            leftGate.localPosition = leftOpen;
            rightGate.localPosition = rightOpen;
            CompleteFadeOut();
        }

        private IEnumerator OpenSingleGateWithDaylight(Vector3 closedPosition, Vector3 openPosition)
        {
            var duration = Mathf.Max(gateStepDuration, fadeDuration);
            var startAlpha = fadeOverlay != null ? fadeOverlay.alpha : 0f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var gateT = Mathf.Clamp01(elapsed / gateStepDuration);
                var fadeT = Mathf.Clamp01(elapsed / fadeDuration);
                ranchGate.localPosition = Vector3.Lerp(closedPosition, openPosition, gateT);
                SetFadeAlpha(Mathf.Lerp(startAlpha, 0f, fadeT));
                yield return null;
            }

            ranchGate.localPosition = openPosition;
            CompleteFadeOut();
        }

        private IEnumerator MoveGatePanels(Vector3 leftFrom, Vector3 leftTo, Vector3 rightFrom, Vector3 rightTo, float duration)
        {
            if (duration <= 0f)
            {
                leftGate.localPosition = leftTo;
                rightGate.localPosition = rightTo;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                leftGate.localPosition = Vector3.Lerp(leftFrom, leftTo, t);
                rightGate.localPosition = Vector3.Lerp(rightFrom, rightTo, t);
                yield return null;
            }

            leftGate.localPosition = leftTo;
            rightGate.localPosition = rightTo;
        }

        private IEnumerator FadeOutOverlay()
        {
            if (fadeOverlay == null)
            {
                yield break;
            }

            yield return FadeOverlay(fadeOverlay.alpha, 0f, fadeDuration);
            CompleteFadeOut();
        }

        private IEnumerator FadeOverlay(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                fadeOverlay.alpha = to;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            fadeOverlay.alpha = to;
        }

        private void SetFadeAlpha(float alpha)
        {
            if (fadeOverlay != null)
            {
                fadeOverlay.alpha = alpha;
            }
        }

        private void CompleteFadeOut()
        {
            if (fadeOverlay == null)
            {
                return;
            }

            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
            fadeOverlay.gameObject.SetActive(false);
        }

        private static IEnumerator MoveLocal(Transform target, Vector3 from, Vector3 to, float duration)
        {
            if (target == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                target.localPosition = to;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                target.localPosition = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            target.localPosition = to;
        }

        private void EnsureFadeOverlay()
        {
            if (fadeOverlay != null)
            {
                return;
            }

            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[RanchAnimationDirector] No Canvas found, so the fade overlay cannot be created.");
                return;
            }

            var overlayObject = new GameObject("Runtime Ranch Fade Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            overlayObject.transform.SetParent(canvas.transform, false);
            overlayObject.transform.SetAsLastSibling();

            var rect = overlayObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = overlayObject.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = true;

            fadeOverlay = overlayObject.GetComponent<CanvasGroup>();
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
            overlayObject.SetActive(false);
        }

        private static Transform FindGateTransform()
        {
            foreach (var transform in FindObjectsOfType<Transform>())
            {
                var lowerName = transform.name.ToLowerInvariant();
                if (lowerName.Contains("gate") || transform.name.Contains("门"))
                {
                    return transform;
                }
            }

            return null;
        }

        private void ResolveGatePanels()
        {
            if (leftGate != null && rightGate != null)
            {
                return;
            }

            foreach (var transform in FindObjectsOfType<Transform>())
            {
                if (leftGate == null && transform.name.Contains("门左"))
                {
                    leftGate = transform;
                }
                else if (rightGate == null && transform.name.Contains("门右"))
                {
                    rightGate = transform;
                }

                if (leftGate != null && rightGate != null)
                {
                    return;
                }
            }
        }
    }
}
