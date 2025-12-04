using UnityEngine;

public class NewPageSpawner : MonoBehaviour
{
    [Header("Page Setup")]
    public GameObject pagePrefab;           // Page prefab (Project)
    public Transform spawnTransform;        // Where the page appears

    [Header("Bird Delivery Setup")]
    public Transform playerTarget;          // XR camera or a child in front of player
    public Transform birdHomePoint;         // where HelpBird returns after delivery

    [Header("Audio")]
    public AudioSource birdPuzzleCompleteSfx;  // plays when all birds are correct (before delivery)

    private bool hasSpawned = false;

    void OnEnable()
    {
        Puzzle4_HouseController.BirdPuzzleCompleted += OnBirdPuzzleCompleted;
    }

    void OnDisable()
    {
        Puzzle4_HouseController.BirdPuzzleCompleted -= OnBirdPuzzleCompleted;
    }

    private void OnBirdPuzzleCompleted()
    {
        if (hasSpawned) return;

        // 🔊 play puzzle-complete sound (bird sound puzzle)
        if (birdPuzzleCompleteSfx != null)
            birdPuzzleCompleteSfx.Play();

        SpawnPage();
    }

    private void SpawnPage()
    {
        if (!pagePrefab || !spawnTransform)
        {
            Debug.LogWarning("[PageSpawner] Missing pagePrefab or spawnTransform!", this);
            return;
        }

        // Spawn the page
        GameObject pageInstance = Instantiate(
            pagePrefab,
            spawnTransform.position,
            spawnTransform.rotation
        );

        hasSpawned = true;
        Debug.Log("[PageSpawner] Page spawned.", this);

        // Try to find the bird courier and tell it where to go
        BirdPickUp courier = Object.FindFirstObjectByType<BirdPickUp>();
        if (courier != null)
        {
            if (playerTarget != null)
                courier.SetPlayerTarget(playerTarget);

            if (birdHomePoint != null)
                courier.SetHomePoint(birdHomePoint);

            courier.StartDelivery(pageInstance.transform);
        }
        else
        {
            Debug.LogWarning("[PageSpawner] No BirdPickUp found in scene; page will stay where it spawned.", this);
        }
    }
}
