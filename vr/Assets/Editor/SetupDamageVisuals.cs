using UnityEngine;
using UnityEditor;

public class SetupDamageVisuals
{
    public static void Execute()
    {
        // 1. Add DamageFlash to Main Camera
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            if (mainCamera.GetComponent<DamageFlash>() == null)
            {
                mainCamera.AddComponent<DamageFlash>();
                Debug.Log("Added DamageFlash to Main Camera.");
            }
        }
        else
        {
            Debug.LogError("Main Camera not found.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
