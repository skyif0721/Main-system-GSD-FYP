using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CheckInteractables
{
    public static void Execute()
    {
        var grabs = Object.FindObjectsOfType<XRGrabInteractable>(true);
        Debug.Log($"Found {grabs.Length} XRGrabInteractable objects:");
        foreach (var g in grabs)
        {
            Rigidbody rb = g.GetComponent<Rigidbody>();
            Debug.Log($"  {g.name} | trackRotation={g.trackRotation} | rb.freezeRot={rb?.constraints}");
        }
    }
}
