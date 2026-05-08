using UnityEngine;
using UnityEditor;

public class FixCameraCanvas
{
    public static void Execute()
    {
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            Canvas canvas = mainCamera.GetComponent<Canvas>();
            if (canvas != null)
            {
                Object.DestroyImmediate(canvas);
                Debug.Log("Removed Canvas from Main Camera.");
            }
            
            UnityEngine.UI.CanvasScaler scaler = mainCamera.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler != null)
            {
                Object.DestroyImmediate(scaler);
                Debug.Log("Removed CanvasScaler from Main Camera.");
            }
            
            UnityEngine.UI.GraphicRaycaster raycaster = mainCamera.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster != null)
            {
                Object.DestroyImmediate(raycaster);
                Debug.Log("Removed GraphicRaycaster from Main Camera.");
            }
            
            // Also remove any FlashImage child that might have been created
            Transform flashImage = mainCamera.transform.Find("FlashImage");
            if (flashImage != null)
            {
                Object.DestroyImmediate(flashImage.gameObject);
                Debug.Log("Removed FlashImage child from Main Camera.");
            }
            
            // Also remove DamageFlashCanvas if it exists so it can be recreated cleanly
            Transform damageFlashCanvas = mainCamera.transform.Find("DamageFlashCanvas");
            if (damageFlashCanvas != null)
            {
                Object.DestroyImmediate(damageFlashCanvas.gameObject);
                Debug.Log("Removed DamageFlashCanvas child from Main Camera.");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
