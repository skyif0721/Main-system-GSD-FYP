using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class FixWeaponNaturalGrab
{
    /// <summary>
    /// One-shot tool: makes every XRGrabInteractable in the active scene
    /// "feel natural" when held in VR. Sets trackRotation=true,
    /// removes rigidbody freeze-rotation constraints, ensures velocity
    /// tracking and proper attach behavior.
    /// </summary>
    [MenuItem("Tools/VR/Fix Weapon Grab (natural rotation)")]
    public static void Run()
    {
        Scene s = SceneManager.GetActiveScene();
        int fixedCount = 0;

        foreach (GameObject root in s.GetRootGameObjects())
        {
            foreach (XRGrabInteractable g in root.GetComponentsInChildren<XRGrabInteractable>(true))
            {
                Undo.RecordObject(g, "Fix Weapon Grab");
                g.trackPosition       = true;
                g.trackRotation       = true;
                g.movementType        = XRBaseInteractable.MovementType.VelocityTracking;
                g.throwOnDetach       = true;
                g.useDynamicAttach    = false;
                g.matchAttachPosition = true;
                g.matchAttachRotation = true;
                EditorUtility.SetDirty(g);

                Rigidbody rb = g.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Undo.RecordObject(rb, "Fix Weapon Grab RB");
                    rb.constraints  = RigidbodyConstraints.None;
                    rb.useGravity   = true;
                    rb.isKinematic  = false;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    if (rb.mass <= 0f) rb.mass = 0.5f;
                    if (rb.drag        < 0.5f) rb.drag        = 0.5f;
                    if (rb.angularDrag < 1f)   rb.angularDrag = 1f;
                    EditorUtility.SetDirty(rb);
                }

                fixedCount++;
                Debug.Log($"[FixWeaponGrab] Fixed: {GetPath(g.transform)}");
            }
        }

        EditorSceneManagerUtility.MarkActiveSceneDirtySafe();
        Debug.Log($"[FixWeaponGrab] Done – fixed {fixedCount} weapon(s).");
    }

    static string GetPath(Transform t)
    {
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }
}

internal static class EditorSceneManagerUtility
{
    public static void MarkActiveSceneDirtySafe()
    {
        try { UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                  UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()); }
        catch { /* ignore */ }
    }
}
