using UnityEngine;
using UnityEditor;
using Unity.XR.CoreUtils;

public class FixXROrigin
{
    public static void Execute()
    {
        GameObject origin = GameObject.Find("Complete XR Origin Set Up Variant");
        if (origin != null)
        {
            XROrigin xrOrigin = origin.GetComponent<XROrigin>();
            if (xrOrigin != null)
            {
                xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
                EditorUtility.SetDirty(xrOrigin);
                Debug.Log("Set XROrigin TrackingOriginMode to Floor");
            }
            else
            {
                Debug.Log("XROrigin component not found");
            }
        }
        else
        {
            Debug.Log("XR Origin not found");
        }
    }
}