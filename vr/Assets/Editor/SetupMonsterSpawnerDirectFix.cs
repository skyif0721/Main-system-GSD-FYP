using UnityEngine;
using UnityEditor;

public class SetupMonsterSpawnerDirectFix
{
    public static void Execute()
    {
        // Find inactive object
        GameObject runningNPC = null;
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Monster_RunningNPC" && obj.scene.isLoaded)
            {
                runningNPC = obj;
                break;
            }
        }

        if (runningNPC != null)
        {
            // 1. Create Spawner
            GameObject spawnerObj = GameObject.Find("MonsterSpawner");
            if (spawnerObj == null)
            {
                spawnerObj = new GameObject("MonsterSpawner");
                GameObject managersFolder = GameObject.Find("--- MANAGERS ---");
                if (managersFolder != null) spawnerObj.transform.SetParent(managersFolder.transform);
            }

            MonsterSpawner spawner = spawnerObj.GetComponent<MonsterSpawner>();
            if (spawner == null) spawner = spawnerObj.AddComponent<MonsterSpawner>();

            // Use the scene object as the prefab template
            spawner.monsterPrefab = runningNPC;
            
            GameObject player = GameObject.Find("Complete XR Origin Set Up Variant");
            if (player != null) spawner.spawnCenter = player.transform;

            spawner.spawnRadius = 15f;
            spawner.spawnInterval = 5f;
            spawner.maxMonsters = 5;

            // 2. Ensure the original NPC in the scene is disabled so it acts as a hidden template
            runningNPC.SetActive(false);

            Debug.Log("Successfully created MonsterSpawner using the scene object as a template!");
        }
        else
        {
            Debug.LogError("Could not find Monster_RunningNPC.");
        }
    }
}
