using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ButtonTouchTrigger : MonoBehaviour
{
    [Header("Button Logic")]
    public PhysicalButton button;          // the script that actually plays the bird sound
    public string controllerTag = "Controller";

    [Header("Audio Clips")]
    public AudioClip pressSfx;             // button click / tap sound (optional)
    [Tooltip("VO: e.g. 'These sounds seem to have an order to them.'")]
    public AudioClip pressVO;

    private AudioSource sfxSource;
    private AudioSource voSource;

    private void Awake()
    {
        // Ensure this collider is a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Create two audio sources automatically so you don't need to add them in the hierarchy
        sfxSource = gameObject.AddComponent<AudioSource>();
        voSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        voSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(controllerTag))
            return;

        // Trigger the physical button logic (your bird-call sound)
        if (button != null)
        {
            button.Press();
        }

        // Play press SFX (click, thump, etc.)
        if (pressSfx != null)
        {
            sfxSource.PlayOneShot(pressSfx);
        }

        // Play VO: "These sounds seem to have an order to them."
        if (pressVO != null)
        {
            // For clarity, interrupt any previous VO and play this fresh
            if (voSource.isPlaying)
                voSource.Stop();

            voSource.clip = pressVO;
            voSource.loop = false;
            voSource.Play();
        }
    }
}
