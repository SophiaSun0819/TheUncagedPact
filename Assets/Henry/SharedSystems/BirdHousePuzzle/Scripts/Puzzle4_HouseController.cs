using UnityEngine;

public class Puzzle4_HouseController : MonoBehaviour
{
    [Header("State")]
    public PuzzleState puzzleState;      // drag your ScriptableObject (PuzzleState_Main)
    public GameObject finalHousePrefab;  // optional: nicer final model

    [Header("Slots")]
    public Slot[] slots;                 // base, pole, house

    [Header("Bird Perches / Sound Boxes")]
    public GameObject[] birdPerches;  
    int _placedCount = 0;
    bool _completed = false;

    public void TryPlacePiece(Slot slot, HousePiece piece)
    {
        if (_completed) return;
        if (piece.isLocked) return;
        if (piece.pieceId != slot.requiredPieceId) return;

        // Snap
        piece.transform.SetPositionAndRotation(
            slot.snapPoint.position,
            slot.snapPoint.rotation
        );
        piece.transform.SetParent(slot.snapPoint, true);

        // Freeze physics
        var rb = piece.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Disable colliders so they don't fight the snap
        var cols = piece.GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = false;

        // Disable grab scripts
        DisableGrabScripts(piece);

        piece.isLocked = true;
        _placedCount++;

        if (_placedCount >= slots.Length)
            CompletePuzzle();
    }


    void DisableGrabScripts(HousePiece piece)
    {
        // Look at *all* MonoBehaviours on the piece
        var behaviours = piece.GetComponents<MonoBehaviour>();

        foreach (var b in behaviours)
        {
            if (b == null) continue;
            string typeName = b.GetType().Name;

            // Catch things like: HandGrabInteractable, GrabInteractable, Grabbable, etc.
            if (typeName.Contains("Grab") || typeName.Contains("Grabbable"))
            {
                b.enabled = false;
                Debug.Log("[HousePuzzle] Disabled grab component: " + typeName, b);
            }
        }
    }


    void CompletePuzzle()
    {
        Debug.Log("[HousePuzzle] COMPLETE!");
        _completed = true;

        if (puzzleState)
            puzzleState.CageBuilt = true;

        if (finalHousePrefab)
            finalHousePrefab.SetActive(true);

        // Enable bird perches / sound boxes AFTER house is built
        if (birdPerches != null)
        {
            foreach (var perch in birdPerches)
            {
                if (perch)
                    perch.SetActive(true);
            }
        }
    }
}
