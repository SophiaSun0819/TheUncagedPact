using UnityEngine;
using UnityEngine.Events;

public class SoundBox : MonoBehaviour
{
    [Header("Settings")]
    // ID of the correct sound/bird for this box
    public int correctSoundID;

    [Header("Snapping")]
    // Where to park the ball/bird on this box (assign a child transform)
    public Transform snapPoint;
    // If true, only correct ID will snap. If false, anything that enters snaps.
    public bool snapOnlyIfCorrect = true;

    [Header("Events")]
    public UnityEvent OnCorrect;
    public UnityEvent OnWrong;

    private void OnTriggerEnter(Collider other)
    {
        // Only react to objects that have a SoundBall
        SoundBall ball = other.GetComponent<SoundBall>();
        if (!ball) return;

        bool isCorrect = (ball.soundID == correctSoundID);

        // --- SNAP LOGIC ---
        if (snapPoint && (!snapOnlyIfCorrect || isCorrect))
        {
            Transform t = ball.transform;

            // Move/rotate onto perch
            t.position = snapPoint.position;
            t.rotation = snapPoint.rotation;

            // Stop physics so it doesn't fall/roll
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        // --- EVENTS / FEEDBACK ---
        if (isCorrect)
        {
            Debug.Log($"[SoundBox] {name}: CORRECT ball {ball.soundID}");
            OnCorrect?.Invoke();
        }
        else
        {
            Debug.Log($"[SoundBox] {name}: WRONG ball {ball.soundID}");
            OnWrong?.Invoke();
        }
    }
}
