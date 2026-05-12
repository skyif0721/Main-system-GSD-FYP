using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Fixes the Tutorial scene's EventSystem to use XRUIInputModule instead of
/// InputSystemUIInputModule, enabling VR ray interaction with UI buttons.
/// </summary>
public class FixTutorialEventSystem
{
    public static string Execute()
    {
        string tutorialScenePath = "Assets/SCENES/tutorial.unity";

        // Open the tutorial scene additively
        var scene = EditorSceneManager.OpenScene(tutorialScenePath, OpenSceneMode.Additive);

        string result = "";

        foreach (var go in scene.GetRootGameObjects())
        {
            var eventSystem = go.GetComponent<EventSystem>();
            if (eventSystem == null) continue;

            // Remove old InputSystemUIInputModule if present
            var oldModule = go.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (oldModule != null)
            {
                Object.DestroyImmediate(oldModule);
                result += "Removed InputSystemUIInputModule from EventSystem. ";
            }

            // Add XRUIInputModule if not already present
            var xrModule = go.GetComponent<XRUIInputModule>();
            if (xrModule == null)
            {
                go.AddComponent<XRUIInputModule>();
                result += "Added XRUIInputModule to EventSystem. ";
            }
            else
            {
                result += "XRUIInputModule already present. ";
            }

            break;
        }

        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.CloseScene(scene, true);

        if (string.IsNullOrEmpty(result))
            result = "No EventSystem found in tutorial scene.";

        Debug.Log("[FixTutorialEventSystem] " + result);
        return result;
    }
}
