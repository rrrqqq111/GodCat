using UnityEngine;

namespace NekogamiRanch.Effects
{
    public class BobMotion : MonoBehaviour
    {
        [SerializeField] private Transform animatedTarget;
        [SerializeField] private float frequency = 1.5f;
        [SerializeField] private float verticalScaleAmplitude = 0.08f;
        [SerializeField] private float horizontalScaleAmplitude = 0.04f;
        [SerializeField] private float phaseOffset;
        [SerializeField] private bool useUnscaledTime;
        [SerializeField] private bool randomizePhaseOnStart = true;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Vector3 bottomAnchorLocalOffset;
        [SerializeField] private bool autoDetectBottomAnchor = true;

        private Vector3 startLocalPosition;
        private Vector3 startLocalScale;
        private Vector3 startBottomParentPosition;

        private void Awake()
        {
            CacheBaseTransform();
        }

        private void Start()
        {
            if (randomizePhaseOnStart)
            {
                phaseOffset += Random.value * Mathf.PI * 2f;
            }
        }

        private void OnEnable()
        {
            CacheBaseTransform();
        }

        private void OnDisable()
        {
            if (animatedTarget == null)
            {
                return;
            }

            animatedTarget.localPosition = startLocalPosition;
            animatedTarget.localScale = startLocalScale;
        }

        private void Update()
        {
            var time = useUnscaledTime ? Time.unscaledTime : Time.time;
            var wave = Mathf.Sin(time * frequency * Mathf.PI * 2f + phaseOffset);
            var scaleX = 1f - wave * horizontalScaleAmplitude;
            var scaleY = 1f + wave * verticalScaleAmplitude;

            if (animatedTarget == null)
            {
                return;
            }

            animatedTarget.localScale = new Vector3(
                startLocalScale.x * scaleX,
                startLocalScale.y * scaleY,
                startLocalScale.z);

            KeepBottomAnchored();
        }

        public void ResetBaseTransform()
        {
            CacheBaseTransform();
        }

        private void CacheBaseTransform()
        {
            ResolveAnimatedTarget();
            if (animatedTarget == null)
            {
                return;
            }

            startLocalPosition = animatedTarget.localPosition;
            startLocalScale = animatedTarget.localScale;

            if (autoDetectBottomAnchor)
            {
                bottomAnchorLocalOffset = DetectBottomAnchorLocalOffset();
            }

            startBottomParentPosition = GetBottomParentPosition();
        }

        private Vector3 DetectBottomAnchorLocalOffset()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            }

            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return Vector3.zero;
            }

            var spriteBounds = spriteRenderer.sprite.bounds;
            var bottomCenterInRendererLocal = new Vector3(spriteBounds.center.x, spriteBounds.min.y, 0f);
            var bottomCenterWorld = spriteRenderer.transform.TransformPoint(bottomCenterInRendererLocal);
            return animatedTarget.InverseTransformPoint(bottomCenterWorld);
        }

        private void KeepBottomAnchored()
        {
            var correction = startBottomParentPosition - GetBottomParentPosition();
            animatedTarget.localPosition += correction;
        }

        private Vector3 GetBottomParentPosition()
        {
            var bottomWorldPosition = animatedTarget.TransformPoint(bottomAnchorLocalOffset);
            return animatedTarget.parent != null
                ? animatedTarget.parent.InverseTransformPoint(bottomWorldPosition)
                : bottomWorldPosition;
        }

        private void ResolveAnimatedTarget()
        {
            if (animatedTarget != null)
            {
                return;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            }

            animatedTarget = spriteRenderer != null ? spriteRenderer.transform : transform;
        }
    }
}
