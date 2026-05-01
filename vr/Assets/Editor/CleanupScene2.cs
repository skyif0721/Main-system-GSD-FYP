using UnityEngine;
using UnityEditor;

public class CleanupScene2
{
    public static void Execute()
    {
        GameObject xrOrigin = GameObject.Find("Complete XR Origin Set Up Variant");
        if (xrOrigin == null)
        {
            // Try finding by component
            var originComponent = Object.FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (originComponent != null)
            {
                xrOrigin = originComponent.gameObject;
            }
        }

        if (xrOrigin != null)
        {
            GameObject recognizer = GameObject.Find("MovementRecognizer");
            if (recognizer != null) recognizer.transform.SetParent(xrOrigin.transform);

            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null) canvas.transform.SetParent(xrOrigin.transform);

            GameObject handler = GameObject.Find("EventHandler");
            if (handler != null) handler.transform.SetParent(xrOrigin.transform);
            
            Debug.Log("Parented objects to XR Origin.");
        }
        else
        {
            Debug.LogError("Could not find XR Origin.");
        }
    }
}
