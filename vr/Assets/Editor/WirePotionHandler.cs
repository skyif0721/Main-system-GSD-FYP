using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class WirePotionHandler
{
    [MenuItem("Tools/Wire Potion Handler")]
    public static void Execute()
    {
        // Add PotionShopHandler to the ShopMenuCanvas
        var shopCanvas = GameObject.Find("VR_Shop_Zone/ShopMenuCanvas");
        if (shopCanvas == null)
        {
            Debug.LogError("[Wire] ShopMenuCanvas not found!");
            return;
        }

        PotionShopHandler handler = shopCanvas.GetComponent<PotionShopHandler>();
        if (handler == null)
            handler = shopCanvas.AddComponent<PotionShopHandler>();

        // Find the green cross template
        GameObject greenCross = GameObject.Find("green cross 3d model");
        if (greenCross != null)
        {
            handler.potionPrefab = greenCross;
            Debug.Log("[Wire] Assigned green cross as potion prefab.");
        }

        // Find spawn point
        GameObject spawnPt = GameObject.Find("WeaponSpawnPoint");
        if (spawnPt != null)
        {
            handler.spawnPoint = spawnPt.transform;
            Debug.Log("[Wire] Assigned WeaponSpawnPoint.");
        }

        EditorUtility.SetDirty(handler);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Wire] PotionShopHandler wired up and scene saved.");
    }
}
