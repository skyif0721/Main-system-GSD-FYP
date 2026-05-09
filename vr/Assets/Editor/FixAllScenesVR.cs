using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.XR;
using Unity.XR.CoreUtils;
using System.Collections.Generic;

public class FixAllScenesVR
{
    static readonly string[] scenePaths = new string[]
    {
        "Assets/shop-training.unity",
        "Assets/SCENES/1 Start Scene.unity",
        "Assets/SCENES/c_path_mountain.unity",
        "Assets/SCENES/Final boss.unity",
        "Assets/SCENES/monster.unity",
        "Assets/SCENES/shop.unity",
        "Assets/SCENES/tutorial.unity",
        "Assets/SCENES/tutorial_circle.unity",
        "Assets/VR_Movement_Detection.unity",
        "Assets/shop-training.unity"
    };

    public static void Execute()
    {
        // Save current scene first
        EditorSceneManager.SaveOpenScenes();
        string originalScene = EditorSceneManager.GetActiveScene().path;

        foreach (string scenePath in scenePaths)
        {
            if (!System.IO.File.Exists(scenePath))
                continue;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool modified = false;

            // 1. Disable XR Interaction Simulator
            GameObject sim = GameObject.Find("XR Interaction Simulator");
            if (sim != null && sim.activeSelf)
            {
                sim.SetActive(false);
                EditorUtility.SetDirty(sim);
                Debug.Log("[" + scenePath + "] Disabled XR Interaction Simulator");
                modified = true;
            }

            // 2. Fix TrackedPoseDriver ignoreTrackingState on all TrackedPoseDrivers
            TrackedPoseDriver[] tpds = Object.FindObjectsOfType<TrackedPoseDriver>();
            foreach (var tpd in tpds)
            {
                if (!tpd.ignoreTrackingState)
                {
                    tpd.ignoreTrackingState = true;
                    EditorUtility.SetDirty(tpd);
                    Debug.Log("[" + scenePath + "] Fixed TrackedPoseDriver on: " + tpd.gameObject.name);
                    modified = true;
                }
            }

            // 3. Set XROrigin to Floor tracking mode
            XROrigin[] origins = Object.FindObjectsOfType<XROrigin>();
            foreach (var origin in origins)
            {
                if (origin.RequestedTrackingOriginMode != XROrigin.TrackingOriginMode.Floor)
                {
                    origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
                    EditorUtility.SetDirty(origin);
                    Debug.Log("[" + scenePath + "] Set XROrigin TrackingOriginMode = Floor");
                    modified = true;
                }
            }

            if (modified)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[" + scenePath + "] Scene saved.");
            }
            else
            {
                Debug.Log("[" + scenePath + "] No changes needed.");
            }
        }

        // Reopen original scene
        if (!string.IsNullOrEmpty(originalScene))
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        Debug.Log("[Done] All scenes processed.");
    }
}
