using UnityEngine;

public class PageSpawner : MonoBehaviour
{
    public GameObject pagePrefab;
    public bool puzzleSuccess=false;
    public Transform spawnTransform;
    private bool hasSpawned = false;
    void Start()
    {
        
    }
    private void OnEnable()
    {
        // 订阅事件
        Puzzle4_HouseController.BirdPuzzleCompleted += OnBirdPuzzleCompleted;
    }

    private void OnDisable()
    {
        // 取消订阅事件
        Puzzle4_HouseController.BirdPuzzleCompleted -= OnBirdPuzzleCompleted;
    }

    // 事件触发后的回调函数
    private void OnBirdPuzzleCompleted()
    {
        puzzleSuccess = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (puzzleSuccess && !hasSpawned)
        {
            SpawnPage();
        }
    }
    private void SpawnPage()
    {
        if (pagePrefab == null || spawnTransform == null)
        {
           
            return;
        }

        
        Instantiate(pagePrefab, spawnTransform.position, spawnTransform.rotation);

        hasSpawned = true; 

       
    }
}
