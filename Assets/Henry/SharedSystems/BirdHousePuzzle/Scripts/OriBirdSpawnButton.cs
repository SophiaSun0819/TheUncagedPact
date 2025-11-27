using UnityEngine;

public class OriBirdSpawnButton : MonoBehaviour
{
    [Header("Bird Setup")]
    public GameObject birdPrefab;            // Bird prefab (from Project)
    public Transform spawnPoint;             // Where the bird appears

    [Header("Water System")]
    public ShaderWaterLevelController waterLevelController;  
    // Drag your pitcher/water object with ShaderWaterLevelController here

    [Header("Delivery Target")]
    public Transform playerTarget;           // e.g. BirdDeliveryPoint child of the camera

    [Header("Spawn Options")]
    public bool canSpawnAgain = false;       // Allow multiple birds or not

    private GameObject _spawnedBird;

    public void SpawnBird()
    {
        // If we don't want multiple birds, stop if one already exists
        if (!canSpawnAgain && _spawnedBird != null)
            return;

        if (!birdPrefab || !spawnPoint)
        {
            Debug.LogWarning("[OriBirdSpawnButton] Missing birdPrefab or spawnPoint!", this);
            return;
        }

        // Spawn the bird
        _spawnedBird = Instantiate(
            birdPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("[OriBirdSpawnButton] Origami bird spawned.", this);

        // Hook up BirdChangeColor -> water controller
        var colorLogic = _spawnedBird.GetComponentInChildren<BirdChangeCustom>();
        if (colorLogic != null)
        {
            if (waterLevelController != null)
            {
                colorLogic.Init(waterLevelController);
            }
            else
            {
                Debug.LogWarning("[OriBirdSpawnButton] No waterLevelController assigned; BirdChangeColor won't react to water.", this);
            }
        }
        else
        {
            Debug.LogWarning("[OriBirdSpawnButton] BirdChangeColor not found on spawned bird.", this);
        }

        // Inject player target into HelpBirdCourier (for delivery later)
        var courier = _spawnedBird.GetComponent<BirdPickUp>();
        if (courier != null)
        {
            if (playerTarget != null)
            {
                courier.SetPlayerTarget(playerTarget);
            }
            else
            {
                Debug.LogWarning("[OriBirdSpawnButton] No playerTarget assigned; courier won't know where to deliver.", this);
            }
        }
        else
        {
            Debug.Log("[OriBirdSpawnButton] No HelpBirdCourier on spawned bird (that's fine if you haven't added it yet).", this);
        }
    }
}
