using UnityEngine;
using UnityEditor;

public class AddCollidersToShopProps
{
    public static void Execute()
    {
        GameObject shop = GameObject.Find("--Map--/shop");
        if (shop == null)
        {
            Debug.LogError("Shop not found.");
            return;
        }

        int count = 0;
        foreach (Transform child in shop.transform)
        {
            if (child.name.Contains("Crate") || child.name.Contains("Barrel"))
            {
                Transform mod = child.GetChild(0);
                if (mod != null)
                {
                    if (mod.GetComponent<Collider>() == null)
                    {
                        mod.gameObject.AddComponent<BoxCollider>();
                        count++;
                    }
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"Added BoxCollider to {count} shop props.");
    }
}
