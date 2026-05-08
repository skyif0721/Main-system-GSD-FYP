using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SetupFinalBoss
{
    public static void Execute()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Final boss")
        {
            Debug.LogError("Please open the 'Final boss' scene first.");
            return;
        }

        // 1. Add Player
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/XR Interaction Toolkit/3.1.2/Starter Assets/Prefabs/Complete XR Origin Set Up Variant.prefab");
        if (playerPrefab == null)
        {
            // Try to find it
            string[] guids = AssetDatabase.FindAssets("Complete XR Origin Set Up Variant t:Prefab");
            if (guids.Length > 0)
            {
                playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }

        GameObject player = null;
        if (playerPrefab != null)
        {
            player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.name = "Complete XR Origin Set Up Variant";
            player.transform.position = new Vector3(0, 0, 0);
            Debug.Log("Added Player.");
            
            // Remove Main Camera if it exists
            GameObject mainCam = GameObject.Find("Main Camera");
            if (mainCam != null && mainCam.transform.parent == null)
            {
                GameObject.DestroyImmediate(mainCam);
            }
        }
        else
        {
            Debug.LogError("Player prefab not found.");
        }

        // 2. Add Boss
        GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ClaySoldierEnemy 1.prefab");
        GameObject boss = null;
        if (bossPrefab != null)
        {
            boss = (GameObject)PrefabUtility.InstantiatePrefab(bossPrefab);
            boss.name = "Boss_ClaySoldier";
            boss.transform.position = new Vector3(0, 0, 10);
            boss.transform.localScale = new Vector3(3, 3, 3); // Huge boss
            Debug.Log("Added Boss.");
        }
        else
        {
            Debug.LogError("Boss prefab not found.");
        }

        // 3. Spawn weapon in Cube (1)
        GameObject cube1 = GameObject.Find("Cube (1)");
        if (cube1 != null)
        {
            // Find a weapon prefab
            string[] weaponGuids = AssetDatabase.FindAssets("02 Sword t:Prefab");
            if (weaponGuids.Length > 0)
            {
                GameObject weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(weaponGuids[0]));
                if (weaponPrefab != null)
                {
                    GameObject weapon = (GameObject)PrefabUtility.InstantiatePrefab(weaponPrefab);
                    weapon.transform.position = cube1.transform.position + Vector3.up * 1.5f;
                    Debug.Log("Spawned weapon on Cube (1).");
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
    }
}
