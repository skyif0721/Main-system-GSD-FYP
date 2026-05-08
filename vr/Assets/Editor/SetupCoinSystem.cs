using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Editor utility: creates the coin prefab, wires CoinPickup, tags the shop zone,
/// and assigns the prefab to ClaySoldierEnemy.
/// Run via Tools > Setup Coin System.
/// </summary>
public class SetupCoinSystem
{
    [MenuItem("Tools/Setup Coin System")]
    public static void Setup()
    {
        // ── 1. Add "ShopZone" tag if missing ─────────────────────────────────
        AddTagIfMissing("ShopZone");

        // ── 2. Tag the VR_Shop_Zone collider ─────────────────────────────────
        GameObject shopZoneObj = GameObject.Find("VR_Shop_Zone");
        if (shopZoneObj != null)
        {
            shopZoneObj.tag = "ShopZone";
            EditorUtility.SetDirty(shopZoneObj);
            Debug.Log("[SetupCoinSystem] Tagged VR_Shop_Zone as 'ShopZone'.");
        }
        else
        {
            Debug.LogWarning("[SetupCoinSystem] Could not find 'VR_Shop_Zone' in scene.");
        }

        // ── 3. Build coin prefab from the scene 'coin' object ─────────────────
        // Find the scene coin object (the one with XRGrabInteractable)
        GameObject sceneCoin = GameObject.Find("coin");
        if (sceneCoin == null)
        {
            Debug.LogError("[SetupCoinSystem] Could not find 'coin' GameObject in scene.");
            return;
        }

        // Add CoinPickup if not already present
        CoinPickup pickup = sceneCoin.GetComponent<CoinPickup>();
        if (pickup == null)
            pickup = sceneCoin.AddComponent<CoinPickup>();
        pickup.coinValue = 20;
        pickup.shopZoneTag = "ShopZone";

        // Make sure the coin collider is a trigger so OnTriggerEnter fires
        // Keep the CapsuleCollider as non-trigger (for physics/grab)
        // Add a separate trigger collider for zone detection
        SphereCollider triggerCol = sceneCoin.GetComponent<SphereCollider>();
        if (triggerCol == null)
            triggerCol = sceneCoin.AddComponent<SphereCollider>();
        triggerCol.isTrigger = true;
        triggerCol.radius = 0.6f;

        // Make sure Rigidbody exists
        Rigidbody rb = sceneCoin.GetComponent<Rigidbody>();
        if (rb == null) rb = sceneCoin.AddComponent<Rigidbody>();
        rb.useGravity = true;

        // ── 4. Save as prefab ─────────────────────────────────────────────────
        string prefabPath = "Assets/Prefabs/CoinDrop.prefab";
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(sceneCoin, prefabPath);
        if (prefabAsset == null)
        {
            Debug.LogError("[SetupCoinSystem] Failed to save coin prefab.");
            return;
        }
        Debug.Log($"[SetupCoinSystem] Coin prefab saved at {prefabPath}");

        // ── 5. Assign prefab to ClaySoldierEnemy ─────────────────────────────
        GameObject enemy = GameObject.Find("ClaySoldierEnemy");
        if (enemy != null)
        {
            MonsterStat ms = enemy.GetComponent<MonsterStat>();
            if (ms != null)
            {
                ms.coinPrefab = prefabAsset;
                EditorUtility.SetDirty(enemy);
                Debug.Log("[SetupCoinSystem] Assigned CoinDrop prefab to ClaySoldierEnemy.coinPrefab.");
            }
            else
            {
                Debug.LogWarning("[SetupCoinSystem] ClaySoldierEnemy has no MonsterStat component.");
            }
        }
        else
        {
            Debug.LogWarning("[SetupCoinSystem] Could not find 'ClaySoldierEnemy' in scene.");
        }

        // ── 6. Also assign to the ClaySoldierEnemy 1 prefab ──────────────────
        string enemyPrefabPath = "Assets/Prefabs/ClaySoldierEnemy 1.prefab";
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPath);
        if (enemyPrefab != null)
        {
            MonsterStat prefabMs = enemyPrefab.GetComponent<MonsterStat>();
            if (prefabMs != null)
            {
                prefabMs.coinPrefab = prefabAsset;
                EditorUtility.SetDirty(enemyPrefab);
                AssetDatabase.SaveAssets();
                Debug.Log("[SetupCoinSystem] Assigned CoinDrop prefab to ClaySoldierEnemy 1 prefab.");
            }
        }

        // ── 7. Mark scene dirty & save ────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[SetupCoinSystem] ✅ Coin system setup complete!");
    }

    static void AddTagIfMissing(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                return; // already exists
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"[SetupCoinSystem] Added tag '{tag}'.");
    }
}
