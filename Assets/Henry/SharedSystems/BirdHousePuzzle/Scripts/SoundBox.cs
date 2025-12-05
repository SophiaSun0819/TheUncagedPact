using UnityEngine;
using UnityEngine.Events;

public class SoundBox : MonoBehaviour
{
    [Header("Matching")]
    public int correctSoundID = 0;

    [Header("Snapping")]
    public Transform snapPoint;             // where to park the bird/ball
    public bool snapOnlyIfCorrect = true;   // if false, snap anything, if true, only correct one

    [Header("Events")]
    public UnityEvent OnCorrect;
    public UnityEvent OnWrong;

    // prevents double-count for this box
    bool _alreadySatisfied = false;

    private void OnTriggerEnter(Collider other)
    {
        // Bird might be on a child collider, so use InParent
        var bird = other.GetComponentInParent<SoundBall>();
        if (bird == null) return;

        // If this bird is already locked, ignore it
        if (bird.IsLocked) return;

        Debug.Log($"[SoundBox] {name} EXPECTS {correctSoundID}, GOT {bird.soundID} from {bird.name}");

        if (bird.soundID == correctSoundID)
        {
            Debug.Log($"[SoundBox] CORRECT match in {name}");

            if (!_alreadySatisfied)
            {
                _alreadySatisfied = true;

                if (Puzzle4_HouseController.Instance != null)
                    Puzzle4_HouseController.Instance.RegisterCorrectBird();
            }

            OnCorrect?.Invoke();

            // --- Detach from hand before snapping ---
            Transform t = bird.transform;
            t.SetParent(null, true);   // drop any grab parent (controller/hand)

            // Snap bird to its perch in world space
            if (snapPoint != null)
            {
                t.position = snapPoint.position;
                t.rotation = snapPoint.rotation;
            }

            // Lock the bird so it can't be moved or grabbed again
            bird.Lock();
        }
        else
        {
            Debug.Log($"[SoundBox] WRONG match in {name}");
            OnWrong?.Invoke();

            if (!snapOnlyIfCorrect && snapPoint != null)
            {
                Transform t = bird.transform;
                t.SetParent(null, true);
                t.position = snapPoint.position;
                t.rotation = snapPoint.rotation;
            }
        }
    }
}
