using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem.XR;

public class CheckTrackedPoseDriver
{
    public static void Execute()
    {
        GameObject cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            TrackedPoseDriver tpd = cam.GetComponent<TrackedPoseDriver>();
            if (tpd != null)
            {
                Debug.Log("Position Input: " + tpd.positionInput.action?.name + " / " + tpd.positionInput.reference?.name);
                Debug.Log("Rotation Input: " + tpd.rotationInput.action?.name + " / " + tpd.rotationInput.reference?.name);
                Debug.Log("Tracking State Input: " + tpd.trackingStateInput.action?.name + " / " + tpd.trackingStateInput.reference?.name);
            }
            else
            {
                Debug.Log("No TrackedPoseDriver found on Main Camera");
            }
        }
        else
        {
            Debug.Log("Main Camera not found");
        }
    }
}