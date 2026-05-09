using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AttachHandAnimation
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

            // --- Left Hand ---
            GameObject leftHand = GameObject.Find("Left Hand Model");
            if (leftHand != null)
            {
                // Remove old component to avoid duplicates
                var existing = leftHand.GetComponent<HandAnimationController>();
                if (existing != null) Object.DestroyImmediate(existing);

                var hac = leftHand.AddComponent<HandAnimationController>();
                hac.handSide = HandAnimationController.HandSide.Left;
                hac.handAnimator = leftHand.GetComponentInChildren<Animator>();
                EditorUtility.SetDirty(leftHand);
                Debug.Log("[" + scenePath + "] Attached HandAnimationController (Left) to Left Hand Model");
                modified = true;
            }

            // --- Right Hand ---
            GameObject rightHand = GameObject.Find("Right Hand Model");
            if (rightHand != null)
            {
                var existing = rightHand.GetComponent<HandAnimationController>();
                if (existing != null) Object.DestroyImmediate(existing);

                var hac = rightHand.AddComponent<HandAnimationController>();
                hac.handSide = HandAnimationController.HandSide.Right;
                hac.handAnimator = rightHand.GetComponentInChildren<Animator>();
                EditorUtility.SetDirty(rightHand);
                Debug.Log("[" + scenePath + "] Attached HandAnimationController (Right) to Right Hand Model");
                modified = true;
            }

            if (modified)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[" + scenePath + "] Scene saved.");
            }
            else
            {
                Debug.Log("[" + scenePath + "] No hand models found, skipping.");
            }
        }

        if (!string.IsNullOrEmpty(originalScene))
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        Debug.Log("[Done] HandAnimationController attached to all applicable scenes.");
    }
}
