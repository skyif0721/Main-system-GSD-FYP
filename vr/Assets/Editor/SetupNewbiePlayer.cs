using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SetupNewbiePlayer
{
    public static void Execute()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        
        GameObject xrOrigin = GameObject.Find("Complete XR Origin Set Up Variant");
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin not found.");
            return;
        }

        // 1. Add newbie body
        GameObject newbiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/newbie.fbx");
        if (newbiePrefab != null)
        {
            // Check if already added
            if (xrOrigin.transform.Find("newbie") == null)
            {
                GameObject newbie = (GameObject)PrefabUtility.InstantiatePrefab(newbiePrefab);
                newbie.name = "newbie";
                newbie.transform.SetParent(xrOrigin.transform, false);
                
                // Fix material
                Material newMat = new Material(Shader.Find("Standard"));
                newMat.color = new Color(0.2f, 0.5f, 0.8f); // Blueish color
                AssetDatabase.CreateAsset(newMat, "Assets/Prefabs/NewbieMaterial.mat");
                
                Renderer[] renderers = newbie.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    r.sharedMaterial = newMat;
                }

                // Add Animator Controller
                Animator animator = newbie.GetComponent<Animator>();
                if (animator == null) animator = newbie.AddComponent<Animator>();
                
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Prefabs/NewbieController.controller");
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                }
                else
                {
                    Debug.LogWarning("NewbieController.controller not found.");
                }

                // Add NewbieMovement script
                NewbieMovement movementScript = newbie.AddComponent<NewbieMovement>();
                movementScript.xrOrigin = xrOrigin.transform;
                movementScript.mainCamera = xrOrigin.transform.Find("Camera Offset/Main Camera");
                
                Debug.Log("Added newbie body to player.");
            }
        }
        else
        {
            Debug.LogError("newbie.fbx not found.");
        }

        // 2. Setup Fireball and Trap
        GameObject fireballPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fireballPrefab.name = "Fireball";
        fireballPrefab.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        Material fireMat = new Material(Shader.Find("Standard"));
        fireMat.color = Color.red;
        fireMat.EnableKeyword("_EMISSION");
        fireMat.SetColor("_EmissionColor", Color.red * 2f);
        AssetDatabase.CreateAsset(fireMat, "Assets/Prefabs/FireballMaterial.mat");
        fireballPrefab.GetComponent<Renderer>().sharedMaterial = fireMat;
        
        Rigidbody rb = fireballPrefab.AddComponent<Rigidbody>();
        rb.useGravity = false;
        fireballPrefab.GetComponent<Collider>().isTrigger = true;
        fireballPrefab.AddComponent<Fireball>();
        
        // Save as prefab
        string fireballPath = "Assets/Prefabs/Fireball.prefab";
        GameObject savedFireball = PrefabUtility.SaveAsPrefabAsset(fireballPrefab, fireballPath);
        GameObject.DestroyImmediate(fireballPrefab);

        // Assign to VRGestureResponse
        VRGestureResponse gestureResponse = xrOrigin.GetComponentInChildren<VRGestureResponse>();
        if (gestureResponse == null)
        {
            gestureResponse = xrOrigin.AddComponent<VRGestureResponse>();
        }
        gestureResponse.fireballPrefab = savedFireball;
        gestureResponse.fireballSpawnPoint = xrOrigin.transform.Find("Camera Offset/Main Camera");

        // 3. Setup Boss Trap
        GameObject trapPrefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trapPrefab.name = "BossTrap";
        trapPrefab.transform.localScale = new Vector3(1f, 0.1f, 1f);
        Material trapMat = new Material(Shader.Find("Standard"));
        trapMat.color = Color.magenta;
        AssetDatabase.CreateAsset(trapMat, "Assets/Prefabs/TrapMaterial.mat");
        trapPrefab.GetComponent<Renderer>().sharedMaterial = trapMat;
        
        trapPrefab.GetComponent<Collider>().isTrigger = true;
        trapPrefab.AddComponent<TrapDamage>();
        
        // Save as prefab
        string trapPath = "Assets/Prefabs/BossTrap.prefab";
        GameObject savedTrap = PrefabUtility.SaveAsPrefabAsset(trapPrefab, trapPath);
        GameObject.DestroyImmediate(trapPrefab);

        // Assign to Boss
        GameObject boss = GameObject.Find("Boss_ClaySoldier");
        if (boss != null)
        {
            BossTrap bossTrap = boss.AddComponent<BossTrap>();
            bossTrap.trapPrefab = savedTrap;
        }

        // 4. Player Health Bar UI
        GameObject canvasObj = new GameObject("PlayerHealthCanvas");
        canvasObj.transform.SetParent(xrOrigin.transform.Find("Camera Offset/Main Camera"), false);
        canvasObj.transform.localPosition = new Vector3(0, -0.3f, 0.8f);
        canvasObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = Color.black;
        bgImage.rectTransform.sizeDelta = new Vector2(200, 20);
        
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image fillImage = fillObj.AddComponent<UnityEngine.UI.Image>();
        fillImage.color = Color.green;
        fillImage.rectTransform.sizeDelta = new Vector2(200, 20);
        
        // Add PlayerStats if missing
        PlayerStats playerStats = xrOrigin.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = xrOrigin.AddComponent<PlayerStats>();
        }
        
        // We need to link the UI to PlayerStats, but PlayerStats might not have a direct reference to this UI.
        // Let's just create a simple script to update the UI.
        
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Setup complete.");
    }
}
