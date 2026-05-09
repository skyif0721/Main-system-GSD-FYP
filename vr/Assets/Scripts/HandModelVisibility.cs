using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

/// <summary>
/// Ensures the Oculus hand skin models attached to the controllers are always visible.
/// The XRInputModalityManager can hide controllers when switching to hand-tracking mode,
/// which also hides the hand mesh children. This script forces them back on every frame.
/// </summary>
public class HandModelVisibility : MonoBehaviour
{
    [Header("Hand Model GameObjects (children of controllers)")]
    public GameObject leftHandModel;
    public GameObject rightHandModel;

    [Header("Settings")]
    [Tooltip("If true, hand models are always shown regardless of modality.")]
    public bool alwaysShowHands = true;

    private XRInputModalityManager _modalityManager;

    void Start()
    {
        _modalityManager = FindObjectOfType<XRInputModalityManager>();

        // Auto-find hand models if not assigned
        if (leftHandModel == null)
        {
            var go = GameObject.Find("Left Hand Model");
            if (go != null) leftHandModel = go;
        }
        if (rightHandModel == null)
        {
            var go = GameObject.Find("Right Hand Model");
            if (go != null) rightHandModel = go;
        }

        EnsureHandsVisible();
    }

    void Update()
    {
        if (alwaysShowHands)
            EnsureHandsVisible();
    }

    void EnsureHandsVisible()
    {
        if (leftHandModel != null && !leftHandModel.activeSelf)
            leftHandModel.SetActive(true);

        if (rightHandModel != null && !rightHandModel.activeSelf)
            rightHandModel.SetActive(true);

        // Also ensure the parent controllers are active
        if (leftHandModel != null && leftHandModel.transform.parent != null
            && !leftHandModel.transform.parent.gameObject.activeSelf)
            leftHandModel.transform.parent.gameObject.SetActive(true);

        if (rightHandModel != null && rightHandModel.transform.parent != null
            && !rightHandModel.transform.parent.gameObject.activeSelf)
            rightHandModel.transform.parent.gameObject.SetActive(true);
    }
}
