using UnityEngine;
using UnityEditor;

public class CheckCameras
{
    public static void Execute()
    {
        Camera[] cameras = Object.FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)
        {
            Debug.Log("Camera: " + cam.name + " | Enabled: " + cam.enabled + " | TargetDisplay: " + cam.targetDisplay + " | Depth: " + cam.depth);
        }
    }
}