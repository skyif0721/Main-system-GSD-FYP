using UnityEngine;
using UnityEditor;

public class SetupMonsterSpawner
{
    public static void Execute()
    {
        GameObject runningNPC = GameObject.Find("Monster_RunningNPC");
        if (runningNPC != null)
        {
            // 1. Make it a prefab
            string prefabPath = "Assets/Prefabs/Monster_RunningNPC.prefab";
            
            // Ensure Prefabs folder exists
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
                
                // Put it in Managers folder
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

            // 3. Disable the original NPC in the scene so it doesn't just stand there
            runningNPC.SetActive(false);

            Debug.Log("Successfully created MonsterSpawner and set up the prefab!");
        }
        else
        {
            Debug.LogError("Could not find Monster_RunningNPC to create prefab.");
        }
    }
}
