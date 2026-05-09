using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AttachHandVisibility
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
    };

    public static void Execute()
    {
        EditorSceneManager.SaveOpenScenes();
        string originalScene = EditorSceneManager.GetActiveScene().path;

        foreach (string scenePath in scenePaths)
        {
            if (!System.IO.File.Exists(scenePath))
                continue;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool modified = false;

            GameObject leftHand = GameObject.Find("Left Hand Model");
            GameObject rightHand = GameObject.Find("Right Hand Model");

            if (leftHand == null && rightHand == null)
            {
                Debug.Log("[" + scenePath + "] No hand models, skipping.");
                continue;
            }

            // Find the XR Origin to attach the script to
            GameObject origin = GameObject.Find("Complete XR Origin Set Up Variant");
            if (origin == null)
            {
                Debug.Log("[" + scenePath + "] XR Origin not found, skipping.");
                continue;
            }

            // Remove existing HandModelVisibility if present (avoid duplicates)
            HandModelVisibility existing = origin.GetComponent<HandModelVisibility>();
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            // Add the component
            HandModelVisibility hmv = origin.AddComponent<HandModelVisibility>();
            hmv.leftHandModel = leftHand;
            hmv.rightHandModel = rightHand;
            hmv.alwaysShowHands = true;

            EditorUtility.SetDirty(origin);
            Debug.Log("[" + scenePath + "] Attached HandModelVisibility to XR Origin");
            modified = true;

            if (modified)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[" + scenePath + "] Scene saved.");
            }
        }

        if (!string.IsNullOrEmpty(originalScene))
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        Debug.Log("[Done] HandModelVisibility attached to all applicable scenes.");
    }
}
