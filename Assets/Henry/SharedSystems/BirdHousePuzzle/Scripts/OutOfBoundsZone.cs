using UnityEngine;

public class OutOfBoundsZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the thing that entered is a SoundBall
        var ball = other.GetComponent<SoundBall>();
        if (ball != null)
        {
            ball.ResetToHome();
        }
    }
}
