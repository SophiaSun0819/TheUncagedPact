using UnityEngine;
using System;

public class Puzzle4_HouseController : MonoBehaviour
{
    [Header("State (optional)")]
    public PuzzleState puzzleState;          // can be left null
    public GameObject finalHousePrefab;      // optional nicer final model

    [Header("Slots")]
    public Slot[] slots;                     // base_slot, pole_slot, house_slot

    [Header("Enable After Build")]
    public GameObject[] birdSoundBoxes;      // soundbox, soundbox2, soundbox3

    int _placedCount = 0;
    bool _completed   = false;

    // 🔔 NEW EVENT
    public static event Action BirdPuzzleCompleted;

    public void TryPlacePiece(Slot slot, HousePiece piece)
    {
        if (_completed) return;
        if (piece.isLocked) return;

        // Wrong piece for this slot? Ignore.
        if (piece.pieceId != slot.requiredPieceId)
            return;

        // Use the snapPoint if assigned, otherwise the slot's own transform
        Transform sp = slot.snapPoint != null ? slot.snapPoint : slot.transform;

        // Snap the piece into place in WORLD space
        piece.transform.SetPositionAndRotation(sp.position, sp.rotation);

        // 🔒 Freeze physics so it stays where we put it
        var rb = piece.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic   = true;   // ignore forces
            rb.useGravity    = false;  // don't let gravity pull it down
            rb.linearVelocity      = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Keep collider solid (not trigger), so it just "sits" there
        var col = piece.GetComponent<Collider>();
        if (col)
        {
            col.enabled   = true;
            col.isTrigger = false;
        }

        // If you still have a disableOnLock array on HousePiece, you can optionally do:
        if (piece.disableOnLock != null)
        {
            foreach (var b in piece.disableOnLock)
            {
                if (b) b.enabled = false;   // disable Grabbable / GrabInteractable / etc.
            }
        }

        piece.isLocked = true;
        _placedCount++;

        if (_placedCount >= slots.Length)
        {
            CompletePuzzle();
        }
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

        // Turn on the bird sound boxes
        if (birdSoundBoxes != null)
        {
            foreach (var go in birdSoundBoxes)
            {
                if (go) go.SetActive(true);
            }
        }
         // 🔔 NEW: Invoke event
        BirdPuzzleCompleted?.Invoke();
    }
}
