using UnityEngine;
using System;

public class Puzzle4_HouseController : MonoBehaviour
{
    // 🔔 NEW EVENT
    public static event Action BirdPuzzleCompleted;

    [Header("State (optional)")]
    public PuzzleState puzzleState;          // can be left null
    public GameObject finalHousePrefab;      // optional nicer final model

    [Header("Slots")]
    public Slot[] slots;                     // base_slot, pole_slot, house_slot

    [Header("Enable After Build")]
    public GameObject[] birdSoundBoxes;      // soundbox, soundbox2, soundbox3

    int _placedCount = 0;
    bool _completed   = false;

    public void TryPlacePiece(Slot slot, HousePiece piece)
    {
        if (_completed) return;
        if (piece.isLocked) return;

        if (piece.pieceId != slot.requiredPieceId)
            return;

        Transform sp = slot.snapPoint != null ? slot.snapPoint : slot.transform;
        piece.transform.SetPositionAndRotation(sp.position, sp.rotation);

        var rb = piece.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        var col = piece.GetComponent<Collider>();
        if (col)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        if (piece.disableOnLock != null)
        {
            foreach (var b in piece.disableOnLock)
                if (b) b.enabled = false;
        }

        piece.isLocked = true;
        _placedCount++;

        if (_placedCount >= slots.Length)
            CompletePuzzle();
    }

    void CompletePuzzle()
    {
        if (_completed) return;
        _completed = true;

        Debug.Log("[HousePuzzle] >>> PUZZLE COMPLETE <<<");

        if (puzzleState)
            puzzleState.CageBuilt = true;

        if (finalHousePrefab)
            finalHousePrefab.SetActive(true);

        if (birdSoundBoxes != null)
        {
            foreach (var go in birdSoundBoxes)
                if (go) go.SetActive(true);
        }

        // 🔔 NEW: Invoke event
        BirdPuzzleCompleted?.Invoke();
    }
}
