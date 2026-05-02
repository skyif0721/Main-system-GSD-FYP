using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class FixClaySoldierPrefab
{
    public static void Execute()
    {
        string prefabPath = "Assets/Prefabs/ClaySoldierEnemy.prefab";
        
        // Load and edit the prefab
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        
        if (prefabRoot == null)
        {
            Debug.LogError("Could not load ClaySoldierEnemy prefab.");
            return;
        }

        // Find and remove Camera components
        Camera[] cameras = prefabRoot.GetComponentsInChildren<Camera>(true);
        foreach (Camera cam in cameras)
        {
            Debug.Log("Removing Camera from: " + cam.gameObject.name);
            Object.DestroyImmediate(cam);
        }

        // Find and remove Light components
        Light[] lights = prefabRoot.GetComponentsInChildren<Light>(true);
        foreach (Light light in lights)
        {
            Debug.Log("Removing Light from: " + light.gameObject.name);
            Object.DestroyImmediate(light);
        }

        // Fix Walk animation - set it to loop
        Animator animator = prefabRoot.GetComponent<Animator>();
        if (animator != null)
        {
            Debug.Log("Animator found: " + animator.runtimeAnimatorController?.name);
        }

        // Log all children for inspection
        foreach (Transform child in prefabRoot.GetComponentsInChildren<Transform>(true))
        {
            Component[] comps = child.GetComponents<Component>();
            foreach (Component comp in comps)
            {
                if (comp != null && !(comp is Transform) && !(comp is MeshRenderer) && 
                    !(comp is MeshFilter) && !(comp is SkinnedMeshRenderer))
                {
                    Debug.Log("Child: " + child.name + " | Component: " + comp.GetType().Name);
                }
            }
        }

        // Save the prefab
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("Prefab cleaned up successfully.");
    }
}
