using UnityEngine;

public class SoundBall : MonoBehaviour
{
    public AudioSource soundSource;
    public int soundID;

    [Header("Respawn")]
    public Transform homePosition;

    Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void PlaySound()
    {
        if (soundSource) soundSource.Play();
    }

    public void ResetToHome()
    {
        if (!homePosition) return;

        // Teleport to home
        _rb.position = homePosition.position;
        _rb.rotation = homePosition.rotation;

        // Stop movement
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }
}
