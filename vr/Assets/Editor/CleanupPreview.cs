using UnityEngine;
using UnityEditor;

public class CleanupPreview
{
    public static void Execute()
    {
        // Remove the preview instance
        GameObject preview = GameObject.Find("ClaySoldierEnemy_Preview");
        if (preview != null)
        {
            Object.DestroyImmediate(preview);
            Debug.Log("Removed ClaySoldierEnemy_Preview from scene.");
        }
    }
}
