using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FixUntitledAttach
{
    public static void Execute()
    {
        // Find Untitled weapon
        GameObject[] all = Object.FindObjectsOfType<GameObject>(true);
        GameObject untitled = null;
        foreach (var go in all)
        {
            if (go.name == "Untitled" && go.transform.parent != null)
            {
                untitled = go;
                break;
            }
        }

        if (untitled == null) { Debug.LogError("Untitled not found!"); return; }

        // Find attach point - named "w"
        Transform attachPoint = untitled.transform.Find("w");
        if (attachPoint == null)
        {
            // Search all children
            foreach (Transform child in untitled.transform)
            {
                Debug.Log("Child: " + child.name);
            }
            Debug.LogWarning("No 'w' child found on Untitled");
            return;
        }

        XRGrabInteractable grab = untitled.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = untitled.AddComponent<XRGrabInteractable>();

        grab.attachTransform        = attachPoint;
        grab.movementType           = XRBaseInteractable.MovementType.VelocityTracking;
        grab.trackPosition          = true;
        grab.trackRotation          = false;
        grab.throwOnDetach          = true;
        grab.throwSmoothingDuration = 0.1f;
        grab.throwVelocityScale     = 1.5f;
        grab.useDynamicAttach       = false;

        Rigidbody rb = untitled.GetComponent<Rigidbody>();
        if (rb == null) rb = untitled.AddComponent<Rigidbody>();
        rb.useGravity  = true;
        rb.isKinematic = false;
        rb.mass        = 0.5f;
        rb.drag        = 2f;
        rb.angularDrag = 5f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        EditorUtility.SetDirty(untitled);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FixUntitledAttach] Fixed Untitled: attachTransform=w at {attachPoint.localPosition}. Scene saved.");
    }
}
