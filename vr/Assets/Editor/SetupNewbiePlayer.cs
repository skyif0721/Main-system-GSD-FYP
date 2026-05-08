using UnityEngine;
using UnityEditor;

public class SetupNewbiePlayer
{
    public static void Execute()
    {
        // 1. Find the XR Origin
        GameObject xrOrigin = GameObject.Find("Complete XR Origin Set Up Variant");
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin not found.");
            return;
        }

        // 2. Instantiate newbie prefab
        GameObject newbiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/newbie.fbx");
        if (newbiePrefab == null)
        {
            Debug.LogError("newbie.fbx not found.");
            return;
        }

        // Check if already added
        Transform existingNewbie = xrOrigin.transform.Find("newbie");
        if (existingNewbie != null)
        {
            GameObject.DestroyImmediate(existingNewbie.gameObject);
        }

        GameObject newbie = (GameObject)PrefabUtility.InstantiatePrefab(newbiePrefab);
        newbie.name = "newbie";
        newbie.transform.SetParent(xrOrigin.transform, false);
        newbie.transform.localPosition = Vector3.zero;
        newbie.transform.localRotation = Quaternion.identity;

        // 3. Assign the fixed material
        Material fixedMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prefabs/Materials/NewbieMat.mat");
        if (fixedMat != null)
        {
            Renderer[] renderers = newbie.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                Material[] mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = fixedMat;
                }
                r.sharedMaterials = mats;
            }
        }

        // 4. Add Animator and setup controller
        Animator animator = newbie.GetComponent<Animator>();
        if (animator == null)
        {
            animator = newbie.AddComponent<Animator>();
        }

        // Create a simple animator controller for movement
        string controllerPath = "Assets/Prefabs/NewbieController.controller";
        UnityEditor.Animations.AnimatorController controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(controllerPath);
        
        if (controller == null)
        {
            controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            
            // Add parameters
            controller.AddParameter("Forward", AnimatorControllerParameterType.Float);
            controller.AddParameter("Right", AnimatorControllerParameterType.Float);
            
            // We need animations. Let's check if there are any standard assets animations we can use.
            // For now, we'll just create the controller structure. The user might need to assign actual clips.
            Debug.LogWarning("Created NewbieController. Please assign Walk/Run animations to the Blend Tree.");
        }
        
        animator.runtimeAnimatorController = controller;

        // 5. Add the movement script
        NewbieMovement movementScript = newbie.AddComponent<NewbieMovement>();
        movementScript.xrOrigin = xrOrigin.transform;
        movementScript.mainCamera = xrOrigin.transform.Find("Camera Offset/Main Camera");

        Debug.Log("Setup newbie player complete.");
    }
}
