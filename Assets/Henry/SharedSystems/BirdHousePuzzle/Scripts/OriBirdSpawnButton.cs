using UnityEngine;

public class OriBirdSpawnButton : MonoBehaviour
{
    [Header("Bird Setup")]
    public GameObject birdPrefab;
    public Transform spawnPoint;

    [Header("Water System")]
    public ShaderWaterLevelController waterLevelController;

    [Header("Delivery Target (HelpBird)")]
    public Transform playerTarget;

    [Header("Options")]
    public bool canSpawnAgain = false;

    GameObject _spawnedBird;

    public void SpawnBird()
    {
        if (!canSpawnAgain && _spawnedBird != null)
            return;

        if (!birdPrefab || !spawnPoint)
        {
            Debug.LogWarning("[OriBirdSpawnButton] Missing prefab or spawnPoint!", this);
            return;
        }

        _spawnedBird = Instantiate(birdPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("[OriBirdSpawnButton] Bird spawned!", this);

        // Give bird water controller
        var colorLogic = _spawnedBird.GetComponentInChildren<BirdChangeCustom>();
        if (colorLogic != null)
        {
            if (waterLevelController != null)
                colorLogic.Init(waterLevelController);
            else
                Debug.LogWarning("[OriBirdSpawnButton] No water controller assigned.");
        }

        // Inject delivery target (if the bird has courier script)
        var courier = _spawnedBird.GetComponent<BirdPickUp>();
        if (courier != null)
        {
            if (playerTarget != null)
            {
                courier.SetPlayerTarget(playerTarget);
            }
            else
            {
                Debug.LogWarning("[OriBirdSpawnButton] No playerTarget assigned.");
            }
        }
    }
}
