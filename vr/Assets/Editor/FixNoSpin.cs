using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FixNoSpin
{
    public static void Execute()
    {
        int count = 0;
        var grabs = Object.FindObjectsOfType<XRGrabInteractable>(true);

        foreach (var grab in grabs)
        {
            // Disable rotation tracking — object keeps its world rotation when grabbed
            grab.trackRotation = false;

            // Freeze all rotation on the Rigidbody so it doesn't tumble when dropped
            Rigidbody rb = grab.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                EditorUtility.SetDirty(rb);
            }

            EditorUtility.SetDirty(grab);
            Debug.Log($"[FixNoSpin] {grab.name}: trackRotation=false, FreezeRotation");
            count++;
        }

        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FixNoSpin] Done. Fixed {count} objects. Scene saved.");
    }
}
