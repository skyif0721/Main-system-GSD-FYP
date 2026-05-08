using UnityEngine;
using UnityEditor;

public class FixWeaponSpawnPoint
{
    public static void Execute()
    {
        GameObject shopMenuCanvas = GameObject.Find("ShopMenuCanvas");
        if (shopMenuCanvas == null)
        {
            Debug.LogError("ShopMenuCanvas not found.");
            return;
        }

        WeaponShopManager wsm = shopMenuCanvas.GetComponent<WeaponShopManager>();
        if (wsm == null)
        {
            Debug.LogError("WeaponShopManager not found on ShopMenuCanvas.");
            return;
        }

        GameObject spawnPoint = GameObject.Find("WeaponSpawnPoint");
        if (spawnPoint == null)
        {
            spawnPoint = new GameObject("WeaponSpawnPoint");
            
            // Find a box to place it on
            GameObject box = GameObject.Find("Prefab_SmallCrate02_color01 (4)");
            if (box != null)
            {
                // The box is at (102.00, 0.95, 106.37) with max.y = 1.266
                spawnPoint.transform.position = new Vector3(102.00f, 1.3f, 106.37f);
                spawnPoint.transform.rotation = Quaternion.Euler(0, 90, 90); // Lay flat
            }
            else
            {
                spawnPoint.transform.position = new Vector3(108.44f, 1.0f, 113.12f);
            }
        }

        wsm.spawnPoint = spawnPoint.transform;
        EditorUtility.SetDirty(wsm);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("WeaponSpawnPoint created and assigned.");
    }
}
