using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class FixHandModels
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

            // Fix Left Hand Model transform
            GameObject leftHand = GameObject.Find("Left Hand Model");
            if (leftHand != null)
            {
                // Match the prefab's correct offset so the hand aligns with the controller
                leftHand.transform.localPosition = new Vector3(-0.001f, 0.001f, -0.035f);
                leftHand.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                leftHand.transform.localScale = Vector3.one;
                EditorUtility.SetDirty(leftHand);
                Debug.Log("[" + scenePath + "] Fixed Left Hand Model transform");
                modified = true;
            }

            // Fix Right Hand Model transform
            GameObject rightHand = GameObject.Find("Right Hand Model");
            if (rightHand != null)
            {
                rightHand.transform.localPosition = new Vector3(-0.001f, 0.001f, -0.035f);
                rightHand.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
                rightHand.transform.localScale = Vector3.one;
                EditorUtility.SetDirty(rightHand);
                Debug.Log("[" + scenePath + "] Fixed Right Hand Model transform");
                modified = true;
            }

            // Wire hand models into XRInputModalityManager
            // The manager needs motionControllerLeft/Right to be the hand model GameObjects
            // so it can toggle them when switching between controller and hand tracking modes
            XRInputModalityManager modality = Object.FindObjectOfType<XRInputModalityManager>();
            if (modality != null && leftHand != null && rightHand != null)
            {
                // Use SerializedObject to set the motionControllerLeft/Right fields
                SerializedObject so = new SerializedObject(modality);

                SerializedProperty leftControllerProp = so.FindProperty("m_MotionControllerLeft");
                SerializedProperty rightControllerProp = so.FindProperty("m_MotionControllerRight");

                if (leftControllerProp != null)
                {
                    leftControllerProp.objectReferenceValue = leftHand;
                    Debug.Log("[" + scenePath + "] Set XRInputModalityManager.motionControllerLeft = Left Hand Model");
                    modified = true;
                }
                else
                {
                    Debug.Log("[" + scenePath + "] m_MotionControllerLeft property not found, trying alternative...");
                }

                if (rightControllerProp != null)
                {
                    rightControllerProp.objectReferenceValue = rightHand;
                    Debug.Log("[" + scenePath + "] Set XRInputModalityManager.motionControllerRight = Right Hand Model");
                    modified = true;
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(modality);
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

        Debug.Log("[Done] Hand model fixes applied to all scenes.");
    }
}
