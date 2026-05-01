using UnityEngine;
using UnityEditor;

public class SetupMonsterSpawnerFix
{
    public static void Execute()
    {
        GameObject runningNPC = GameObject.Find("Monster_RunningNPC");
        if (runningNPC != null)
        {
            // Remove missing scripts
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(runningNPC);
            foreach (Transform child in runningNPC.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
            }

            // 1. Make it a prefab
            string prefabPath = "Assets/Prefabs/Monster_RunningNPC.prefab";
            
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(runningNPC, prefabPath);
            
            // 2. Create Spawner
            GameObject spawnerObj = GameObject.Find("MonsterSpawner");
            if (spawnerObj == null)
            {
                spawnerObj = new GameObject("MonsterSpawner");
                GameObject managersFolder = GameObject.Find("--- MANAGERS ---");
                if (managersFolder != null) spawnerObj.transform.SetParent(managersFolder.transform);
            }

            MonsterSpawner spawner = spawnerObj.GetComponent<MonsterSpawner>();
            if (spawner == null) spawner = spawnerObj.AddComponent<MonsterSpawner>();

            spawner.monsterPrefab = prefab;
            
            GameObject player = GameObject.Find("Complete XR Origin Set Up Variant");
            if (player != null) spawner.spawnCenter = player.transform;

            spawner.spawnRadius = 15f;
            spawner.spawnInterval = 5f;
            spawner.maxMonsters = 5;

            // 3. Disable the original NPC in the scene
            runningNPC.SetActive(false);

            Debug.Log("Successfully created MonsterSpawner and set up the prefab!");
        }
        else
        {
            Debug.LogError("Could not find Monster_RunningNPC to create prefab.");
        }
    }
}
