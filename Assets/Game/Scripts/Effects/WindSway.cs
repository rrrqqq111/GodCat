using UnityEngine;

namespace NekogamiRanch.Effects
{
    public class WindSway : MonoBehaviour
    {
        [SerializeField] private float frequency = 0.8f;
        [SerializeField] private float angleAmplitude = 4f;
        [SerializeField] private float horizontalStretchAmplitude = 0.04f;
        [SerializeField] private float verticalStretchAmplitude = 0.015f;
        [SerializeField] private float phaseOffset;
        [SerializeField] private bool useUnscaledTime;
        [SerializeField] private bool randomizePhaseOnStart = true;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Vector3 bottomAnchorLocalOffset;
        [SerializeField] private bool autoDetectBottomAnchor = true;

        private Vector3 startLocalPosition;
        private Quaternion startLocalRotation;
        private Vector3 startLocalScale;
        private Vector3 startBottomWorldPosition;

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
            transform.localPosition = startLocalPosition;
            transform.localRotation = startLocalRotation;
            transform.localScale = startLocalScale;
        }

        private void Update()
        {
            var time = useUnscaledTime ? Time.unscaledTime : Time.time;
            var wave = Mathf.Sin(time * frequency * Mathf.PI * 2f + phaseOffset);
            var stretch = Mathf.Abs(wave);

            transform.localRotation = startLocalRotation * Quaternion.Euler(0f, 0f, wave * angleAmplitude);
            transform.localScale = new Vector3(
                startLocalScale.x * (1f + stretch * horizontalStretchAmplitude),
                startLocalScale.y * (1f + stretch * verticalStretchAmplitude),
                startLocalScale.z);

            KeepBottomAnchored();
        }

        public void ResetBaseTransform()
        {
            CacheBaseTransform();
        }

        private void CacheBaseTransform()
        {
            startLocalPosition = transform.localPosition;
            startLocalRotation = transform.localRotation;
            startLocalScale = transform.localScale;

            if (autoDetectBottomAnchor)
            {
                bottomAnchorLocalOffset = DetectBottomAnchorLocalOffset();
            }

            startBottomWorldPosition = transform.TransformPoint(bottomAnchorLocalOffset);
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
            return transform.InverseTransformPoint(bottomCenterWorld);
        }

        private void KeepBottomAnchored()
        {
            var currentBottomWorldPosition = transform.TransformPoint(bottomAnchorLocalOffset);
            var correctionWorld = startBottomWorldPosition - currentBottomWorldPosition;
            if (transform.parent != null)
            {
                transform.localPosition += transform.parent.InverseTransformVector(correctionWorld);
            }
            else
            {
                transform.position += correctionWorld;
            }
        }
    }
}
