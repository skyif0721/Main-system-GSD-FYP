using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public Transform spawnCenter;
    public float spawnRadius = 15f;
    public float spawnInterval = 5f;
    public int maxMonsters = 5;

    private float lastSpawnTime;
    private int currentMonsters = 0;

    private void Start()
    {
        if (spawnCenter == null)
        {
            GameObject player = GameObject.Find("Complete XR Origin Set Up Variant");
            if (player != null) spawnCenter = player.transform;
        }
    }

    private void Update()
    {
        if (monsterPrefab != null && spawnCenter != null)
        {
            if (Time.time - lastSpawnTime >= spawnInterval && currentMonsters < maxMonsters)
            {
                SpawnMonster();
                lastSpawnTime = Time.time;
            }
        }
    }

    private void SpawnMonster()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        // Don't spawn too close to the player
        if (randomCircle.magnitude < 5f) randomCircle = randomCircle.normalized * 5f;
        
        Vector3 spawnPos = spawnCenter.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            GameObject newMonster = Instantiate(monsterPrefab, hit.position, Quaternion.identity);
            newMonster.SetActive(true);
            currentMonsters++;
            

            
            Debug.Log("Spawned a new monster at " + hit.position);
        }
        else
        {
            Debug.LogWarning("Failed to find NavMesh position near " + spawnPos);
        }
    }

    public void MonsterDied()
    {
        currentMonsters--;
    }
}

