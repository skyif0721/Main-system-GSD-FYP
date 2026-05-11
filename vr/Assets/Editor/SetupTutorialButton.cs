using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Replaces the "About" button in the Start Scene with a "Tutorial" button
/// that loads the tutorial scene.
/// </summary>
public class SetupTutorialButton
{
    [MenuItem("Tools/Setup Tutorial Button in Start Scene")]
    public static void Execute()
    {
        // 1. Rename the About button text to "Tutorial"
        var aboutTextGO = GameObject.Find("Game Menu UI/Menu/About/Text (TMP)");
        if (aboutTextGO != null)
        {
            var tmp = aboutTextGO.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "Tutorial";
                EditorUtility.SetDirty(tmp);
                Debug.Log("[SetupTutorial] Changed About button text to 'Tutorial'");
            }
        }
        else
        {
            Debug.LogWarning("[SetupTutorial] Could not find About button text!");
        }

        // 2. Fix build settings: ensure Start Scene is index 0, shop-training is index 1, tutorial is index 2
        FixBuildSettings();

        // 3. Mark scene dirty so it saves
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[SetupTutorial] Done! About button is now 'Tutorial'. Build settings updated.");
    }

    static void FixBuildSettings()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

        // Index 0: Start Scene
        string startPath = "Assets/SCENES/1 Start Scene.unity";
        scenes.Add(new EditorBuildSettingsScene(startPath, true));

        // Index 1: shop-training (main game)
        string shopPath = "Assets/shop-training.unity";
        scenes.Add(new EditorBuildSettingsScene(shopPath, true));

        // Index 2: tutorial
        string tutorialPath = "Assets/SCENES/tutorial.unity";
        scenes.Add(new EditorBuildSettingsScene(tutorialPath, true));

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[SetupTutorial] Build settings: 0=Start Scene, 1=shop-training, 2=tutorial");
    }
}
