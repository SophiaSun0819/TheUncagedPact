using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ShaderWaterLevelController : MonoBehaviour
{
    [Header("== EYEBALL DETECTION ==")]
    public string eyeballTag = "Eye";

    [Header("== WATER VISUAL ==")]
    public Renderer waterRenderer;
    public string shaderFillProperty = "_FillAmount";

    [Header("== FILL SETTINGS ==")]
    [Range(0f, 1f)] public float currentFill01 = 0f;
    [Range(0f, 1f)] public float fullFill01 = 1f;
    [Range(0.01f, 0.5f)] public float fillPerEyeball = 0.1f;

    [Header("== AUDIO CLIPS (Drag these in) ==")]
    public AudioClip sfxSplash;
    public AudioClip voWaterRising;
    public AudioClip voWaterStillLow;
    public AudioClip voWaterTooLow;
    public AudioClip voWaterReady;
    public AudioClip sfxWaterComplete;

    private AudioSource sfxSource;
    private AudioSource voSource;

    [Header("== EVENTS FOR OTHER SCRIPTS ==")]
    public UnityEvent onWaterReady;           // ✅ NEW: when threshold reached
    public UnityEvent onWaterBottleComplete;  // FULL completion
    public bool waterReady = false;           // ✅ NEW
    public bool waterBottleComplete = false;

    [Header("== Ready / Hint Thresholds ==")]
    [Range(0f, 1f)]
    public float readyThreshold = 0.7f;

    private bool readyVOFired = false;
    private int eyeballCount = 0;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        sfxSource = gameObject.AddComponent<AudioSource>();
        voSource  = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        voSource.playOnAwake  = false;

        ApplyFillToShader();

        // handle if you set currentFill01 in inspector
        if (!waterReady && currentFill01 >= readyThreshold)
        {
            waterReady = true;
        }
        if (!waterBottleComplete && currentFill01 >= fullFill01 - 0.0001f)
        {
            waterBottleComplete = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(eyeballTag))
            return;

        eyeballCount++;

        PlaySfx(sfxSplash);

        if (waterBottleComplete) return;

        currentFill01 = Mathf.Clamp01(currentFill01 + fillPerEyeball);
        ApplyFillToShader();

        if (eyeballCount == 1) PlayVoice(voWaterRising);
        else if (eyeballCount == 2 && currentFill01 < readyThreshold) PlayVoice(voWaterStillLow);

        // ✅ READY threshold
        if (!waterReady && currentFill01 >= readyThreshold)
        {
            waterReady = true;

            if (!readyVOFired)
            {
                readyVOFired = true;
                PlayVoice(voWaterReady);
            }

            onWaterReady?.Invoke();
        }

        // FULL completion
        if (!waterBottleComplete && currentFill01 >= fullFill01 - 0.0001f)
        {
            waterBottleComplete = true;
            PlaySfx(sfxWaterComplete);
            onWaterBottleComplete?.Invoke();
        }
    }

    public void TryPlayTooLowVO()
    {
        // use READY, not FULL
        if (!waterReady && currentFill01 < readyThreshold)
        {
            PlayVoice(voWaterTooLow);
        }
    }

    private void ApplyFillToShader()
    {
        if (waterRenderer == null) return;

        var mat = waterRenderer.material;
        if (string.IsNullOrEmpty(shaderFillProperty)) return;
        if (!mat.HasProperty(shaderFillProperty)) return;

        mat.SetFloat(shaderFillProperty, currentFill01);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private void PlayVoice(AudioClip clip)
    {
        if (clip == null) return;
        if (voSource.isPlaying) voSource.Stop();
        voSource.clip = clip;
        voSource.loop = false;
        voSource.Play();
    }

    public void ResetWater()
    {
        eyeballCount = 0;
        currentFill01 = 0f;
        waterReady = false;
        waterBottleComplete = false;
        readyVOFired = false;
        ApplyFillToShader();
    }
}
