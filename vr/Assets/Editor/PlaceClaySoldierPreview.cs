using UnityEngine;
using UnityEditor;

public class PlaceClaySoldierPreview
{
    public static void Execute()
    {
        // Place a preview instance in the scene at the player's position for visual check
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ClaySoldierEnemy.prefab");
        if (prefab == null)
        {
            Debug.LogError("ClaySoldierEnemy prefab not found!");
            return;
        }

        // Place near the player start position
        GameObject player = GameObject.Find("Complete XR Origin Set Up Variant");
        Vector3 spawnPos = player != null 
            ? player.transform.position + player.transform.forward * 3f 
            : new Vector3(108f, 6.2f, 95f);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.position = spawnPos;
        instance.name = "ClaySoldierEnemy_Preview";

        // Select it so we can see it
        Selection.activeGameObject = instance;

        Debug.Log("Placed ClaySoldierEnemy preview at " + spawnPos);
    }
}
