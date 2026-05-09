using UnityEngine;
using UnityEditor;

public class CheckHandModels
{
    public static void Execute()
    {
        // Check Left Hand Model children
        GameObject leftHand = GameObject.Find("Left Hand Model");
        if (leftHand != null)
        {
            Debug.Log("Left Hand Model children count: " + leftHand.transform.childCount);
            Debug.Log("Left Hand Model localPos: " + leftHand.transform.localPosition);
            Debug.Log("Left Hand Model localRot: " + leftHand.transform.localEulerAngles);
            for (int i = 0; i < leftHand.transform.childCount; i++)
            {
                var child = leftHand.transform.GetChild(i);
                Debug.Log("  Child: " + child.name + " active=" + child.gameObject.activeSelf);
                var renderers = child.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var r in renderers)
                    Debug.Log("    SkinnedMeshRenderer: " + r.name + " enabled=" + r.enabled + " mat=" + (r.sharedMaterial != null ? r.sharedMaterial.name : "null"));
            }
        }
        else
        {
            Debug.Log("Left Hand Model not found in scene");
        }

        // Check Right Hand Model children
        GameObject rightHand = GameObject.Find("Right Hand Model");
        if (rightHand != null)
        {
            Debug.Log("Right Hand Model children count: " + rightHand.transform.childCount);
            Debug.Log("Right Hand Model localPos: " + rightHand.transform.localPosition);
            Debug.Log("Right Hand Model localRot: " + rightHand.transform.localEulerAngles);
            for (int i = 0; i < rightHand.transform.childCount; i++)
            {
                var child = rightHand.transform.GetChild(i);
                Debug.Log("  Child: " + child.name + " active=" + child.gameObject.activeSelf);
                var renderers = child.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var r in renderers)
                    Debug.Log("    SkinnedMeshRenderer: " + r.name + " enabled=" + r.enabled + " mat=" + (r.sharedMaterial != null ? r.sharedMaterial.name : "null"));
            }
        }
        else
        {
            Debug.Log("Right Hand Model not found in scene");
        }

        // Check XRInputModalityManager for hand tracking objects
        var modality = Object.FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>();
        if (modality != null)
        {
            Debug.Log("XRInputModalityManager found");
            Debug.Log("  leftController: " + (modality.leftController != null ? modality.leftController.name : "null"));
            Debug.Log("  rightController: " + (modality.rightController != null ? modality.rightController.name : "null"));
        }
    }
}
