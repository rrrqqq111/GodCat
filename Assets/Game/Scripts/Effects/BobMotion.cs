using UnityEngine;

namespace NekogamiRanch.Effects
{
    public class BobMotion : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform animatedTarget;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool autoDetectBottomAnchor = true;
        [SerializeField] private Vector3 bottomAnchorLocalOffset;

        [Header("Motion")]
        [SerializeField, Min(0f)] private float frequency = 1.5f;
        [SerializeField, Range(0f, 0.5f)] private float verticalStretch = 0.08f;
        [SerializeField, Range(0f, 0.5f)] private float horizontalSquash = 0.04f;
        [SerializeField] private float phaseOffset;
        [SerializeField] private bool randomizePhaseOnEnable = true;
        [SerializeField] private bool useUnscaledTime;
        [SerializeField] private bool playOnEnable = true;

        private Vector3 baseLocalPosition;
        private Vector3 baseLocalScale = Vector3.one;
        private Vector3 baseBottomParentPosition;
        private bool hasBasePose;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (randomizePhaseOnEnable)
            {
                phaseOffset += Random.value * Mathf.PI * 2f;
            }

            ResetBaseTransform();
        }

        private void OnDisable()
        {
            RestoreBaseTransform();
        }

        private void LateUpdate()
        {
            if (!playOnEnable || animatedTarget == null)
            {
                return;
            }

            if (!hasBasePose)
            {
                ResetBaseTransform();
            }

            var time = useUnscaledTime ? Time.unscaledTime : Time.time;
            var wave = Mathf.Sin(time * frequency * Mathf.PI * 2f + phaseOffset);
            var scaleX = 1f - wave * horizontalSquash;
            var scaleY = 1f + wave * verticalStretch;

            animatedTarget.localPosition = baseLocalPosition;
            animatedTarget.localScale = new Vector3(
                baseLocalScale.x * scaleX,
                baseLocalScale.y * scaleY,
                baseLocalScale.z);

            KeepBottomAnchored();
        }

        public void ResetBaseTransform()
        {
            ResolveReferences();
            if (animatedTarget == null)
            {
                return;
            }

            baseLocalPosition = animatedTarget.localPosition;
            baseLocalScale = animatedTarget.localScale;
            if (autoDetectBottomAnchor)
            {
                bottomAnchorLocalOffset = DetectBottomAnchorLocalOffset();
            }

            baseBottomParentPosition = GetBottomParentPosition();
            hasBasePose = true;
        }

        public void SetPlaying(bool playing)
        {
            playOnEnable = playing;
            if (!playing)
            {
                RestoreBaseTransform();
            }
        }

        private void RestoreBaseTransform()
        {
            if (!hasBasePose || animatedTarget == null)
            {
                return;
            }

            animatedTarget.localPosition = baseLocalPosition;
            animatedTarget.localScale = baseLocalScale;
        }

        private void ResolveReferences()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            }

            if (animatedTarget == null)
            {
                animatedTarget = spriteRenderer != null ? spriteRenderer.transform : transform;
            }
        }

        private Vector3 DetectBottomAnchorLocalOffset()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null || animatedTarget == null)
            {
                return Vector3.zero;
            }

            var spriteBounds = spriteRenderer.sprite.bounds;
            var bottomCenterRendererLocal = new Vector3(spriteBounds.center.x, spriteBounds.min.y, 0f);
            var bottomCenterWorld = spriteRenderer.transform.TransformPoint(bottomCenterRendererLocal);
            return animatedTarget.InverseTransformPoint(bottomCenterWorld);
        }

        private void KeepBottomAnchored()
        {
            var currentBottomParentPosition = GetBottomParentPosition();
            var correction = baseBottomParentPosition - currentBottomParentPosition;

            if (animatedTarget.parent != null)
            {
                animatedTarget.localPosition += correction;
            }
            else
            {
                animatedTarget.position += correction;
            }
        }

        private Vector3 GetBottomParentPosition()
        {
            var bottomWorldPosition = animatedTarget.TransformPoint(bottomAnchorLocalOffset);
            return animatedTarget.parent != null
                ? animatedTarget.parent.InverseTransformPoint(bottomWorldPosition)
                : bottomWorldPosition;
        }
    }
}
