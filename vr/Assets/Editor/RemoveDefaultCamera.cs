using UnityEngine;
using UnityEditor;

public class RemoveDefaultCamera
{
    public static void Execute()
    {
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null && mainCamera.transform.parent == null)
        {
            GameObject.DestroyImmediate(mainCamera);
            Debug.Log("Removed default Main Camera.");
        }
    }
}
