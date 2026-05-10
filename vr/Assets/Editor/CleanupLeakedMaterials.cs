using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class CleanupLeakedMaterials
{
    /// <summary>
    /// Forces all renderers in the active scene to keep using their
    /// shared material. This drops any per-renderer instance materials
    /// that were leaked into the scene file by edit-mode '.material' calls.
    /// </summary>
    public static void Run()
    {
        Scene s = SceneManager.GetActiveScene();
        int touched = 0;
        foreach (GameObject root in s.GetRootGameObjects())
        {
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null) continue;
                // Re-assign sharedMaterials to themselves – this drops any
                // hidden instance materials that were instantiated.
                Material[] shared = r.sharedMaterials;
                r.sharedMaterials = shared;
                EditorUtility.SetDirty(r);
                touched++;
            }
        }
        EditorSceneManager.MarkSceneDirty(s);
        Debug.Log($"[CleanupLeakedMaterials] Touched {touched} renderers in scene '{s.name}'.");
    }
}
