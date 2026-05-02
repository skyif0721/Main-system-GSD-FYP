using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class FixWalkClipFinal
{
    public static void Execute()
    {
        // Correct filename is "Mutant Walking (1).fbx"
        string walkPath = "Assets/Game object/character/Enemy/claySoldier/New Folder/Mutant Walking (1).fbx";

        // Force reimport and set loop
        AssetDatabase.ImportAsset(walkPath, ImportAssetOptions.ForceUpdate);

        ModelImporter importer = AssetImporter.GetAtPath(walkPath) as ModelImporter;
        if (importer != null)
        {
            var clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.defaultClipAnimations;

            foreach (var clip in clips)
            {
                clip.loopTime = true;
                clip.loopPose = true;
                Debug.Log($"Set loop on walk clip: '{clip.name}'");
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();

        // Load the clip
        AnimationClip walkClip = null;
        foreach (Object obj in AssetDatabase.LoadAllAssetsAtPath(walkPath))
        {
            if (obj is AnimationClip c && !c.name.StartsWith("__preview__"))
            {
                walkClip = c;
                Debug.Log($"Found walk clip: '{c.name}' loop={c.isLooping}");
                break;
            }
        }

        if (walkClip == null)
        {
            Debug.LogError("Still no walk clip found!");
            return;
        }

        // Assign to controller
        string controllerPath = "Assets/Game object/character/Enemy/claySoldier/ClaySoldierController.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller != null)
        {
            foreach (ChildAnimatorState s in controller.layers[0].stateMachine.states)
            {
                if (s.state.name == "Walk")
                {
                    s.state.motion = walkClip;
                    Debug.Log($"Assigned Walk state -> '{walkClip.name}'");
                }
            }
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        Debug.Log("Walk clip fix DONE!");
    }
}
