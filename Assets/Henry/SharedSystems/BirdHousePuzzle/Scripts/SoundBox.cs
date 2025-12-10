using UnityEngine;
using UnityEngine.Events;

public class SoundBox : MonoBehaviour
{
    [Header("Matching")]
    public int correctSoundID = 0;

    [Header("Snapping")]
    public Transform snapPoint;
    public bool snapOnlyIfCorrect = true;

    [Header("Events")]
    public UnityEvent OnCorrect;
    public UnityEvent OnWrong;

    bool _alreadySatisfied = false;

    // this ensures BirdOnLedge VO only plays once globally
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

                if (Puzzle4_HouseController.Instance != null)
                    Puzzle4_HouseController.Instance.RegisterCorrectBird();
            }

            OnCorrect?.Invoke();

            // 🔊 First correct bird placed → BirdOnLedge VO
            if (!_birdOnLedgeVoPlayed && SoundPuzzleVOController.Instance != null)
            {
                _birdOnLedgeVoPlayed = true;
                SoundPuzzleVOController.Instance.CueBirdOnLedge();
            }

            // detach from hand
            Transform t = ball.transform;
            t.SetParent(null, true);

            // snap
            if (snapPoint != null)
            {
                t.position = snapPoint.position;
                t.rotation = snapPoint.rotation;
            }

            // fix world scale
            ball.ApplyOriginalScale();

            // lock
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

            if (!snapOnlyIfCorrect && snapPoint != null)
            {
                Transform t = ball.transform;
                t.SetParent(null, true);
                t.position = snapPoint.position;
                t.rotation = snapPoint.rotation;
                ball.ApplyOriginalScale();
            }
        }
    }
}
