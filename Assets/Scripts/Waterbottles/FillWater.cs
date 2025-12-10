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
    public AudioClip sfxSplash;        // eyeball drop sound
    public AudioClip voWaterRising;    // “Water is rising…” after first eyeball
    public AudioClip voWaterStillLow;  // “Water is STILL too low…” after second eyeball
    public AudioClip voWaterTooLow;    // played when bird approaches too early
    public AudioClip voWaterReady;     // “This should be high enough!”
    public AudioClip sfxWaterComplete; // Final completion SFX or VO

    // separate sources so SFX can overlap but VO does not
    private AudioSource sfxSource;
    private AudioSource voSource;

    [Header("== EVENTS FOR OTHER SCRIPTS ==")]
    public UnityEvent onWaterBottleComplete;    // triggers BirdChangeColor
    public bool waterBottleComplete = false;    // read by BirdChangeColor

    [Header("== Ready / Hint Thresholds ==")]
    [Tooltip("After this %, bird should be able to drink. Fires once.")]
    [Range(0f, 1f)]
    public float readyThreshold = 0.7f;

    private bool readyVOFired = false;
    private int eyeballCount = 0;

    private Collider triggerCol;

    private void Awake()
    {
        triggerCol = GetComponent<Collider>();
        triggerCol.isTrigger = true;

        // separate sources for SFX and VO
        sfxSource = gameObject.AddComponent<AudioSource>();
        voSource  = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        voSource.playOnAwake = false;

        ApplyFillToShader();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(eyeballTag))
            return;

        eyeballCount++;

        // Splash SFX
        PlaySfx(sfxSplash);

        if (waterBottleComplete) return;

        // Increase water level
        currentFill01 = Mathf.Clamp01(currentFill01 + fillPerEyeball);
        ApplyFillToShader();

        // FIRST EYEBALL → “Water is rising!”
        if (eyeballCount == 1)
        {
            PlayVoice(voWaterRising);
        }
        // SECOND EYEBALL → “Water is STILL too low!”
        else if (eyeballCount == 2 && currentFill01 < readyThreshold)
        {
            PlayVoice(voWaterStillLow);
        }

        // Ready-to-drink threshold VO
        if (!readyVOFired && currentFill01 >= readyThreshold)
        {
            readyVOFired = true;
            PlayVoice(voWaterReady);
        }

        // FULL completion logic
        if (!waterBottleComplete && currentFill01 >= fullFill01 - 0.0001f)
        {
            waterBottleComplete = true;

            // Final SFX/VO
            PlaySfx(sfxWaterComplete);

            // Trigger event for bird
            onWaterBottleComplete?.Invoke();
        }
    }

    // Called by the bird when approaching to check if water is still too low
    public void TryPlayTooLowVO()
    {
        if (!waterBottleComplete && currentFill01 < readyThreshold)
        {
            PlayVoice(voWaterTooLow);
        }
    }

    private void ApplyFillToShader()
    {
        if (waterRenderer == null)
        {
            Debug.LogWarning("[ShaderWaterLevelController] No waterRenderer assigned.", this);
            return;
        }

        var mat = waterRenderer.material;
        if (string.IsNullOrEmpty(shaderFillProperty))
        {
            Debug.LogError("[ShaderWaterLevelController] shaderFillProperty is empty.", this);
            return;
        }

        if (!mat.HasProperty(shaderFillProperty))
        {
            Debug.LogError(
                $"[ShaderWaterLevelController] Material '{mat.name}' does NOT have a float property '{shaderFillProperty}'. " +
                $"Open your water material and check the actual property name.",
                this
            );
            return;
        }

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

        // stop any previous VO so they don’t overlap
        if (voSource.isPlaying)
            voSource.Stop();

        voSource.clip = clip;
        voSource.loop = false;
        voSource.Play();
    }

    public void ResetWater()
    {
        eyeballCount = 0;
        currentFill01 = 0f;
        waterBottleComplete = false;
        readyVOFired = false;
        ApplyFillToShader();
    }
}
