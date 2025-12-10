using UnityEngine;
using System;

public class Puzzle4_HouseController : MonoBehaviour
{
    public static Puzzle4_HouseController Instance;

    void Awake()
    {
        Instance = this;
    }

    [Header("State (optional)")]
    public PuzzleState puzzleState;
    public GameObject finalHousePrefab;

    [Header("Slots")]
    public Slot[] slots;

    [Header("Enable After Build")]
    public GameObject[] birdSoundBoxes;

    [Header("Bird Sound Puzzle")]
    public int requiredBirds = 3;

    [Header("Audio")]
    public AudioSource pieceSnapSfx;       // plays every time a house piece snaps
    public AudioSource houseCompleteSfx;   // plays when all house pieces are placed

    int _correctBirds = 0;
    bool _birdPuzzleDone = false;
    int _placedCount = 0;
    bool _completed = false;

    public static event Action BirdPuzzleCompleted;

    public void TryPlacePiece(Slot slot, HousePiece piece)
    {
        if (_completed) return;
        if (piece.isLocked) return;

        if (piece.pieceId != slot.requiredPieceId)
            return;

        // ---- SNAP ----
        Transform t = piece.transform;
        t.SetParent(null, true);
        t.position = slot.snapPoint.position;
        t.rotation = slot.snapPoint.rotation;

        // ---- STOP PHYSICS ----
        var rb = piece.GetComponent<Rigidbody>();
        if (rb)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // ---- DISABLE COLLISION ----
        var col = piece.GetComponent<Collider>();
        if (col) col.enabled = false;

        // disable grab components, etc.
        if (piece.disableOnLock != null)
        {
            foreach (var b in piece.disableOnLock)
                if (b) b.enabled = false;
        }

        piece.isLocked = true;
        _placedCount++;

        // 🔊 play snap sfx for each piece
        if (pieceSnapSfx != null)
            pieceSnapSfx.Play();

        // 🔊 VO flow for building the cage:
        // 1) "Pick up first piece" (only actually plays the first time)
        // 2) "That fits nicely." after placing a piece in the outline.
        if (SoundPuzzleVOController.Instance != null)
        {
            SoundPuzzleVOController.Instance.CuePickUpFirstPiece();
            SoundPuzzleVOController.Instance.CuePieceRight();
        }

        if (_placedCount >= slots.Length)
            CompletePuzzle();
    }

    void CompletePuzzle()
    {
        if (_completed) return;
        _completed = true;

        if (puzzleState)
            puzzleState.CageBuilt = true;

        if (finalHousePrefab)
            finalHousePrefab.SetActive(true);

        // 🔊 SFX: house/cage built
        if (houseCompleteSfx != null)
            houseCompleteSfx.Play();

        // 🔊 VO: cage / house complete ("cozy home" etc.)
        if (SoundPuzzleVOController.Instance != null)
        {
            SoundPuzzleVOController.Instance.CueCageComplete();
        }

        // Enable sound puzzle boxes for bird song puzzle
        if (birdSoundBoxes != null)
        {
            foreach (var go in birdSoundBoxes)
                if (go) go.SetActive(true);
        }
    }

    public void RegisterCorrectBird()
    {
        if (_birdPuzzleDone) return;

        _correctBirds++;

        if (_correctBirds >= requiredBirds)
        {
            _birdPuzzleDone = true;

            BirdPuzzleCompleted?.Invoke();

            // 🔊 VO: "Yes, it recognized the song!"
            if (SoundPuzzleVOController.Instance != null)
            {
                SoundPuzzleVOController.Instance.CueSongRecognized();
            }
        }
    }
}
