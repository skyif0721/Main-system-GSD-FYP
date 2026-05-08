using UnityEngine;
using UnityEditor;

public class AddHandModels
{
    public static void Execute()
    {
        GameObject leftController = GameObject.Find("Left Controller");
        GameObject rightController = GameObject.Find("Right Controller");

        if (leftController != null)
        {
            // Hide existing visual
            Transform visual = leftController.transform.Find("Left Controller Visual");
            if (visual != null)
            {
                visual.gameObject.SetActive(false);
            }

            // Add hand model
            GameObject handPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Oculus Hands/Prefabs/Left Hand Model.prefab");
            if (handPrefab != null)
            {
                // Check if already added
                if (leftController.transform.Find("Left Hand Model") == null)
                {
                    GameObject hand = (GameObject)PrefabUtility.InstantiatePrefab(handPrefab);
                    hand.name = "Left Hand Model";
                    hand.transform.SetParent(leftController.transform, false);
                    
                    // Adjust rotation/position if needed for Oculus hands
                    hand.transform.localPosition = Vector3.zero;
                    hand.transform.localRotation = Quaternion.identity;
                    
                    Debug.Log("Added Left Hand Model.");
                }
            }
            else
            {
                Debug.LogError("Left Hand Model prefab not found.");
            }
        }

        if (rightController != null)
        {
            // Hide existing visual
            Transform visual = rightController.transform.Find("Right Controller Visual");
            if (visual != null)
            {
                visual.gameObject.SetActive(false);
            }

            // Add hand model
            GameObject handPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Oculus Hands/Prefabs/Right Hand Model.prefab");
            if (handPrefab != null)
            {
                // Check if already added
                if (rightController.transform.Find("Right Hand Model") == null)
                {
                    GameObject hand = (GameObject)PrefabUtility.InstantiatePrefab(handPrefab);
                    hand.name = "Right Hand Model";
                    hand.transform.SetParent(rightController.transform, false);
                    
                    // Adjust rotation/position if needed for Oculus hands
                    hand.transform.localPosition = Vector3.zero;
                    hand.transform.localRotation = Quaternion.identity;
                    
                    Debug.Log("Added Right Hand Model.");
                }
            }
            else
            {
                Debug.LogError("Right Hand Model prefab not found.");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
