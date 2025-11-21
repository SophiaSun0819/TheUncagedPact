using UnityEngine;

public class Slot : MonoBehaviour
{
    public int requiredPieceId = 0;            // set to match the correct piece
    public Transform snapPoint;                // where the piece should lock to
    public Puzzle4_HouseController controller; // drag the controller here

    void OnTriggerEnter(Collider other)
    {
        var piece = other.GetComponent<HousePiece>() 
                    ?? other.GetComponentInParent<HousePiece>();
        if (!piece) return;

        Debug.Log($"[HousePuzzle] {name} trigger with {piece.name}");
        controller.TryPlacePiece(this, piece);
    }
}
