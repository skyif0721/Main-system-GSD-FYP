using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem.XR;

public class CheckOtherTPD
{
    public static void Execute()
    {
        TrackedPoseDriver[] tpds = Object.FindObjectsOfType<TrackedPoseDriver>();
        foreach (TrackedPoseDriver tpd in tpds)
        {
            Debug.Log("TrackedPoseDriver found on: " + tpd.gameObject.name);
        }
    }
}