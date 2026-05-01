using UnityEngine;
using UnityEditor;

public class CleanupScene
{
    public static void Execute()
    {
        GameObject xrOrigin = GameObject.Find("Complete XR Origin Set Up Variant");
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin not found.");
            return;
        }

        // Parent objects to XR Origin
        GameObject recognizer = GameObject.Find("MovementRecognizer");
        if (recognizer != null) recognizer.transform.SetParent(xrOrigin.transform);

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null) canvas.transform.SetParent(xrOrigin.transform);

        GameObject handler = GameObject.Find("EventHandler");
        if (handler != null) handler.transform.SetParent(xrOrigin.transform);

        // Delete other objects
        GameObject floor = GameObject.Find("Floor");
        if (floor != null) GameObject.DestroyImmediate(floor);

        GameObject dirLight = GameObject.Find("Directional Light");
        if (dirLight != null) GameObject.DestroyImmediate(dirLight);

        Debug.Log("Scene cleaned up. Only XR Origin remains at the root.");
    }
}
