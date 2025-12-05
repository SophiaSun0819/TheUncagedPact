using UnityEngine;
using UnityEngine.Events;

public class PhysicalButton : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource clickSound;

    [Header("Events")]
    public UnityEvent OnPressed;

    public void Press()
    {
        if (clickSound != null)
        {
            clickSound.Play();
        }

        OnPressed?.Invoke();
    }
}
