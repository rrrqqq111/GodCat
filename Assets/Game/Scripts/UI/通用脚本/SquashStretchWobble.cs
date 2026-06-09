using UnityEngine;

public class SquashStretchWobble : MonoBehaviour
{
    [Header("Wobble")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField, Min(0f)] private float widthAmplitude = 0.03f;
    [SerializeField, Min(0f)] private float heightAmplitude = 0.03f;
    [SerializeField, Min(0f)] private float speed = 2.5f;
    [SerializeField] private bool invertHeight = true;
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 baseScale;
    private float timeOffset;
    private bool isPlaying;

    private void Awake()
    {
        baseScale = transform.localScale;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void OnEnable()
    {
        baseScale = transform.localScale;
        isPlaying = playOnEnable;
    }

    private void OnDisable()
    {
        transform.localScale = baseScale;
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        float wave = Mathf.Sin(time * speed + timeOffset);
        float widthScale = 1f + wave * widthAmplitude;
        float heightWave = invertHeight ? -wave : wave;
        float heightScale = 1f + heightWave * heightAmplitude;

        transform.localScale = new Vector3(
            baseScale.x * widthScale,
            baseScale.y * heightScale,
            baseScale.z);
    }

    public void Play()
    {
        baseScale = transform.localScale;
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
        transform.localScale = baseScale;
    }
}
