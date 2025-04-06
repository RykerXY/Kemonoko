using UnityEngine;

public class Spawner : MonoBehaviour
{
    private Collider2D SpawnZone; // Collider to check for valid spawn positions
    public GameObject Prefab; // Prefab of the flower to spawn
    public int numberOfPrefab = 10; // Number of flowers to spawn
    public float spawnRadius = 5f; // Radius within which to spawn flowers
    public float minDistance = 1f; // Minimum distance between flowers

    public int delay = 1; // Delay in seconds between each spawn
    private float spawnTimer = 0f; // Timer to track spawn delay

    //Spawns flowers at flowerZone positions
    void Start()
    {
        SpawnZone = GetComponent<Collider2D>();
        SpawnFlowers();
    }

    // Spawns flowers at random positions within the flower zone
    void SpawnFlowers()
    {
        for (int i = 0; i < numberOfPrefab; i++)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();
            Instantiate(Prefab, spawnPosition, Quaternion.identity);
        }
    }
    // Gets a random position within the flower zone

    Vector2 GetRandomSpawnPosition()
    {
        Vector2 randomPosition = (Vector2)SpawnZone.bounds.center + Random.insideUnitCircle * spawnRadius;
        randomPosition.x = Mathf.Clamp(randomPosition.x, SpawnZone.bounds.min.x, SpawnZone.bounds.max.x);
        randomPosition.y = Mathf.Clamp(randomPosition.y, SpawnZone.bounds.min.y, SpawnZone.bounds.max.y);
        return randomPosition;
    }
    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= delay)
        {
            SpawnFlowers();
            spawnTimer = 0f; // Reset the timer after spawning
        }
    }

    bool IsPositionValid(Vector2 position)
    {
        if (SpawnZone.OverlapPoint(position))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, minDistance);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Flower"))
                {
                    return false; // Position is occupied by another flower
                }
            }
            return true; // Position is valid for spawning
        }
        return false; // Position is outside the flower zone
    }
    // Spawns a flower at a random position within the flower zone

    void SpawnFlowerAtRandomPosition()
    {
        Vector2 randomPosition = GetRandomSpawnPosition();
        if (IsPositionValid(randomPosition))
        {
            Instantiate(Prefab, randomPosition, Quaternion.identity);
        }
        else
        {
            Debug.Log("Invalid position for spawning flower: " + randomPosition);
        }
    }

}
