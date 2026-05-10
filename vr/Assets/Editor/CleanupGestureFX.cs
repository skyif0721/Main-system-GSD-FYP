using UnityEngine;
using UnityEditor;

public static class CleanupGestureFX
{
    public static void Run()
    {
        int n = 0;
        foreach (var go in Object.FindObjectsOfType<GameObject>())
        {
            if (go == null) continue;
            if (go.name.StartsWith("GestureFX_") || go.name.StartsWith("DamagePopup_"))
            {
                Object.DestroyImmediate(go);
                n++;
            }
        }
        Debug.Log($"[CleanupGestureFX] Removed {n} stray gesture/damage FX objects.");
    }
}
