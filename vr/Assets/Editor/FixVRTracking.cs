using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem.XR;
using Unity.XR.CoreUtils;

public class FixVRTracking
{
    public static void Execute()
    {
        int fixCount = 0;

        // 1. Disable XR Interaction Simulator - it overrides real HMD input
        GameObject sim = GameObject.Find("XR Interaction Simulator");
        if (sim != null)
        {
            if (sim.activeSelf)
            {
                sim.SetActive(false);
                EditorUtility.SetDirty(sim);
                Debug.Log("[Fix] Disabled XR Interaction Simulator");
            }
            else
            {
                Debug.Log("[OK] XR Interaction Simulator already disabled");
            }
            fixCount++;
        }

        // 2. Fix TrackedPoseDriver on Main Camera - set ignoreTrackingState = true
        //    so it always applies pose even if tracking state flags aren't perfect
        GameObject camGO = GameObject.Find("Main Camera");
        if (camGO != null)
        {
            TrackedPoseDriver tpd = camGO.GetComponent<TrackedPoseDriver>();
            if (tpd != null)
            {
                tpd.ignoreTrackingState = true;
                EditorUtility.SetDirty(tpd);
                Debug.Log("[Fix] Set Main Camera TrackedPoseDriver.ignoreTrackingState = true");
                fixCount++;
            }
        }

        // 3. Fix TrackedPoseDriver on Left Controller
        GameObject leftCtrl = GameObject.Find("Left Controller");
        if (leftCtrl != null)
        {
            TrackedPoseDriver tpd = leftCtrl.GetComponent<TrackedPoseDriver>();
            if (tpd != null)
            {
                tpd.ignoreTrackingState = true;
                EditorUtility.SetDirty(tpd);
                Debug.Log("[Fix] Set Left Controller TrackedPoseDriver.ignoreTrackingState = true");
                fixCount++;
            }
        }

        // 4. Fix TrackedPoseDriver on Right Controller
        GameObject rightCtrl = GameObject.Find("Right Controller");
        if (rightCtrl != null)
        {
            TrackedPoseDriver tpd = rightCtrl.GetComponent<TrackedPoseDriver>();
            if (tpd != null)
            {
                tpd.ignoreTrackingState = true;
                EditorUtility.SetDirty(tpd);
                Debug.Log("[Fix] Set Right Controller TrackedPoseDriver.ignoreTrackingState = true");
                fixCount++;
            }
        }

        // 5. Set XROrigin tracking mode to Floor (correct for Meta Quest)
        GameObject origin = GameObject.Find("Complete XR Origin Set Up Variant");
        if (origin != null)
        {
            XROrigin xrOrigin = origin.GetComponent<XROrigin>();
            if (xrOrigin != null)
            {
                xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
                EditorUtility.SetDirty(xrOrigin);
                Debug.Log("[Fix] Set XROrigin TrackingOriginMode = Floor");
                fixCount++;
            }
        }

        // 6. Save the scene
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Done] Applied " + fixCount + " fixes and saved scene.");
    }
}
