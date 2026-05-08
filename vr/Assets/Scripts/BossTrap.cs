using UnityEngine;

public class BossTrap : MonoBehaviour
{
    public GameObject trapPrefab;
    public float spawnInterval = 5f;
    public float trapLifetime = 3f;
    public int trapDamage = 20;

    [Header("Spawn Settings")]
    public int trapsPerSpawn = 5;
    public float minSpawnRadius = 3f;
    public float maxSpawnRadius = 12f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnTrapsAroundBoss();
        }
    }

    void SpawnTrapsAroundBoss()
    {
        if (trapPrefab == null) return;

        for (int i = 0; i < trapsPerSpawn; i++)
        {
            // Random position around the boss
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            spawnPos.y = 0.1f; // Keep it near the ground

            GameObject trap = Instantiate(trapPrefab, spawnPos, Quaternion.identity);
            Destroy(trap, trapLifetime);
        }
    }
}
