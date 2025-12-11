using UnityEngine;
using UnityEngine.Events;

public class SoundBox : MonoBehaviour
{
    [Header("Matching")]
    public int correctSoundID = 0;

    [Header("Snapping")]
    public Transform snapPoint;              // optional now, we can skip snap if you want
    public bool snapOnlyIfCorrect = true;

    [Header("Perched Bird Swap")]
    [Tooltip("Pre-placed bird on the ledge. Starts disabled, gets enabled on correct match.")]
    public GameObject perchedBird;           // ← assign your ledge bird here

    [Header("Events")]
    public UnityEvent OnCorrect;
    public UnityEvent OnWrong;

    bool _alreadySatisfied = false;

    // ensures BirdOnLedge VO only plays once globally
    private static bool _birdOnLedgeVoPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponentInParent<SoundBall>();
        if (ball == null) return;
        if (ball.IsLocked) return;

        Debug.Log($"[SoundBox] {name} EXPECTS {correctSoundID}, GOT {ball.soundID} from {ball.name}");

        if (ball.soundID == correctSoundID)
        {
            Debug.Log($"[SoundBox] CORRECT match in {name}");

            if (!_alreadySatisfied)
            {
                _alreadySatisfied = true;

                // notify puzzle controller: one more correct bird placed
                if (Puzzle4_HouseController.Instance != null)
                    Puzzle4_HouseController.Instance.RegisterCorrectBird();
            }

            OnCorrect?.Invoke();

            // 🔊 First correct bird placed → BirdOnLedge VO (only once across all boxes)
            if (!_birdOnLedgeVoPlayed && SoundPuzzleVOController.Instance != null)
            {
                _birdOnLedgeVoPlayed = true;
                SoundPuzzleVOController.Instance.CueBirdOnLedge();
            }

            // --- ENABLE PERCHED BIRD & REMOVE FLYING BALL ---
            if (perchedBird != null)
            {
                perchedBird.SetActive(true);
            }

            // Option A: just disable the flying ball so it disappears
            ball.gameObject.SetActive(false);

            // Option B (if you prefer destruction instead):
            // GameObject.Destroy(ball.gameObject);
            // (you can swap to this later if you want)

            // We still lock it in case something re-enables it accidentally
            ball.Lock();
        }
        else
        {
            Debug.Log($"[SoundBox] WRONG match in {name}");
            OnWrong?.Invoke();

            if (SoundPuzzleVOController.Instance != null)
            {
                SoundPuzzleVOController.Instance.CueWrongSoundBall();
            }

            // Optional: snap wrong ball visually even if it's wrong
            if (!snapOnlyIfCorrect && snapPoint != null)
            {
                Transform t = ball.transform;
                t.SetParent(null, true);
                t.position = snapPoint.position;
                t.rotation = snapPoint.rotation;
                // localScale stays as-is
            }
        }
    }
}
