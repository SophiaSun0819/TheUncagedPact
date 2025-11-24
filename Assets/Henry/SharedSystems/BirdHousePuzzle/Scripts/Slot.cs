using UnityEngine;

public class Slot : MonoBehaviour
{
    public int requiredPieceId = 0;              // which piece belongs here
    public Transform snapPoint;                  // where it should snap to
    public Puzzle4_HouseController controller;   // drag controller here

    private void OnTriggerEnter(Collider other)
    {
        var piece = other.GetComponent<HousePiece>();
        if (!piece) return;

        Debug.Log($"[HousePuzzle] {name} trigger with {piece.name}");
        controller.TryPlacePiece(this, piece);
    }
}
