using UnityEngine;

public class NewPageSpawner : MonoBehaviour
{
    [Header("Page Setup")]
    public GameObject pagePrefab;           // Page prefab (Project)
    public Transform spawnTransform;        // Where the page appears

    [Header("Bird Delivery Setup")]
    public Transform playerTarget;          // XR camera or a child in front of player
    public Transform birdHomePoint;         // where the bird should return after delivery

    // Note: pageHoldPoint is set on the BirdPickUp component itself (on the prefab)

    private bool hasSpawned = false;

    void OnEnable()
    {
        // Listen for sound puzzle completion
        Puzzle4_HouseController.BirdPuzzleCompleted += OnBirdPuzzleCompleted;
    }

    void OnDisable()
    {
        Puzzle4_HouseController.BirdPuzzleCompleted -= OnBirdPuzzleCompleted;
    }

    private void OnBirdPuzzleCompleted()
    {
        if (!hasSpawned)
        {
            SpawnPage();
        }
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
            // Give the bird the player target (XR camera / stop point)
            if (playerTarget != null)
            {
                courier.SetPlayerTarget(playerTarget);
            }
            else
            {
                Debug.LogWarning("[PageSpawner] playerTarget is not assigned on NewPageSpawner.", this);
            }

            // Give the bird its home perch
            if (birdHomePoint != null)
            {
                courier.SetHomePoint(birdHomePoint);
            }
            else
            {
                Debug.LogWarning("[PageSpawner] birdHomePoint is not assigned on NewPageSpawner.", this);
            }

            // Start the delivery flight with this page
            courier.StartDelivery(pageInstance.transform);
        }
        else
        {
            Debug.LogWarning("[PageSpawner] No BirdPickUp found in scene; page will stay where it spawned.", this);
        }
    }
}
