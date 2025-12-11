using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPuzzleVOController : MonoBehaviour
{
    public static SoundPuzzleVOController Instance { get; private set; }

    [Header("Voice Audio Source")]
    [Tooltip("Single VO source for all puzzle lines (no overlap, uses a queue).")]
    public AudioSource voSource;

    [Header("Clips")]
    [Tooltip("Pickup first house piece: e.g. 'These shapes look like they fit together.'")]
    public AudioClip voPickUpFirstPiece;

    [Tooltip("Approach cage outline/shadow: e.g. 'Looks like I have to put these pieces in that shadow.'")]
    public AudioClip voApproachShadow;

    [Tooltip("Correct piece snapped into place: e.g. 'That fits nicely.'")]
    public AudioClip voPieceRight;

    [Tooltip("Cage / house built: e.g. 'A cozy home… now how do I get the bird inside?'")]
    public AudioClip voCageComplete;

    [Tooltip("After bird drinks water: e.g. 'It looks lonely… maybe I can repair its home.'")]
    public AudioClip voAfterWaterHomeHint;

    [Tooltip("When bird is on a wrong ledge / temporary perch.")]
    public AudioClip voBirdOnLedge;

    [Tooltip("When bird delivers or picks up a page: e.g. 'What’s that page it picked up?'")]
    public AudioClip voBirdDeliverNote;

    [Tooltip("Hint when player walks near the outline puzzle.")]
    public AudioClip voOutlineProximityHint;

    [Tooltip("VO when the wrong sound ball is placed in a box.")]
    public AudioClip voWrongSoundBall;

    [Tooltip("VO when all birds are in the correct boxes (song recognized).")]
    public AudioClip voSongRecognized;

    [Header("Timer VO")]
    [Tooltip("Day 5 warning: 'Only a couple days left.'")]
    public AudioClip voTimerDay5Warning;

    [Tooltip("Day 7 fail line: 'Oh no, I couldn't escape in time.'")]
    public AudioClip voTimerFailDay7;

    // --- queue + state so VO does NOT overlap ---
    private readonly Queue<AudioClip> _queue = new Queue<AudioClip>();
    private bool _isPlayingQueue = false;

    // one-time flags
    private bool _playedPickUpFirstPiece = false;
    private bool _playedApproachShadow = false;
    private bool _playedPieceRight = false;
    private bool _playedCageComplete = false;
    private bool _playedAfterWaterHomeHint = false;
    private bool _playedOutlineProximityHint = false;
    private bool _playedWrongBallHint = false;
    private bool _playedSongRecognized = false;
    private bool _playedBirdDeliverNote = false;

    // timer VO flags
    private bool _playedTimerDay5Warning = false;
    private bool _playedTimerFailDay7    = false;
    // BirdOnLedge can fire multiple times if desired.

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        if (voSource == null)
        {
            voSource = gameObject.AddComponent<AudioSource>();
            voSource.playOnAwake = false;
        }
    }

    // ---------- PUBLIC CUE METHODS ----------

    public void CuePickUpFirstPiece()
    {
        if (_playedPickUpFirstPiece) return;
        _playedPickUpFirstPiece = true;
        EnqueueClip(voPickUpFirstPiece);
    }

    public void CueApproachShadow()
    {
        if (_playedApproachShadow) return;
        _playedApproachShadow = true;
        EnqueueClip(voApproachShadow);
    }

    public void CuePieceRight()
    {
        if (_playedPieceRight) return;
        _playedPieceRight = true;
        EnqueueClip(voPieceRight);
    }

    public void CueCageComplete()
    {
        if (_playedCageComplete) return;
        _playedCageComplete = true;
        EnqueueClip(voCageComplete);
    }

    public void CueAfterWaterHomeHint()
    {
        if (_playedAfterWaterHomeHint) return;
        _playedAfterWaterHomeHint = true;
        EnqueueClip(voAfterWaterHomeHint);
    }

    public void CueBirdOnLedge()
    {
        EnqueueClip(voBirdOnLedge);
    }

    public void CueBirdDeliverNote()
    {
        if (_playedBirdDeliverNote) return;
        _playedBirdDeliverNote = true;
        EnqueueClip(voBirdDeliverNote);
    }

    public void CueOutlineProximityHint()
    {
        if (_playedOutlineProximityHint) return;
        _playedOutlineProximityHint = true;
        EnqueueClip(voOutlineProximityHint);
    }

    public void CueWrongSoundBall()
    {
        if (_playedWrongBallHint) return;
        _playedWrongBallHint = true;
        EnqueueClip(voWrongSoundBall);
    }

    public void CueSongRecognized()
    {
        if (_playedSongRecognized) return;
        _playedSongRecognized = true;
        EnqueueClip(voSongRecognized);
    }

    // ---- NEW: timer VO ----
    public void CueTimerDay5Warning()
    {
        if (_playedTimerDay5Warning) return;
        _playedTimerDay5Warning = true;
        EnqueueClip(voTimerDay5Warning);
    }

    public void CueTimerFailDay7()
    {
        if (_playedTimerFailDay7) return;
        _playedTimerFailDay7 = true;
        EnqueueClip(voTimerFailDay7);
    }

    // ---------- INTERNAL QUEUE LOGIC ----------

    private void EnqueueClip(AudioClip clip)
    {
        if (clip == null || voSource == null) return;

        _queue.Enqueue(clip);

        if (!_isPlayingQueue)
        {
            StartCoroutine(PlayQueueRoutine());
        }
    }

    private IEnumerator PlayQueueRoutine()
    {
        _isPlayingQueue = true;

        while (_queue.Count > 0)
        {
            var clip = _queue.Dequeue();
            voSource.Stop();
            voSource.clip = clip;
            voSource.loop = false;
            voSource.Play();

            while (voSource.isPlaying)
            {
                yield return null;
            }

            // tiny gap so lines don't feel smashed together
            yield return new WaitForSeconds(0.05f);
        }

        _isPlayingQueue = false;
    }
}
