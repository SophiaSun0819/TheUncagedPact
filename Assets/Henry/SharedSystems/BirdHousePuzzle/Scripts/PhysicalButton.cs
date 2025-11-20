using UnityEngine;
using UnityEngine.Events;   // <- this fixes the UnityEvent error

public class PhysicalButton : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource clickSound;

    [Header("Events")]
    public UnityEvent OnPressed;

    // Call this when the button is *actually* pressed
    public void Press()
    {
        if (clickSound != null)
        {
            clickSound.Play();
        }

        OnPressed?.Invoke();
    }
}
