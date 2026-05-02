using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class ForceImportWalk
{
    public static void Execute()
    {
        string walkPath = "Assets/Game object/character/Enemy/claySoldier/New Folder/Mutant Walking.fbx";

        // Force reimport
        AssetDatabase.ImportAsset(walkPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        // Check what's in it now
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(walkPath);
        Debug.Log($"After reimport - assets in Mutant Walking.fbx: {all.Length}");
        AnimationClip walkClip = null;
        foreach (Object obj in all)
        {
            Debug.Log($"  Type: {obj.GetType().Name} | Name: '{obj.name}'");
            if (obj is AnimationClip c && !c.name.StartsWith("__preview__"))
                walkClip = c;
        }

        if (walkClip == null)
        {
            Debug.LogError("Still no clip found in Mutant Walking.fbx!");
            return;
        }

        // Set loop on the clip via ModelImporter
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
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            Debug.Log("Set walk clip to loop.");
        }

        // Reload clip after reimport
        all = AssetDatabase.LoadAllAssetsAtPath(walkPath);
        foreach (Object obj in all)
        {
            if (obj is AnimationClip c && !c.name.StartsWith("__preview__"))
            {
                walkClip = c;
                break;
            }
        }

        // Assign to controller
        string controllerPath = "Assets/Game object/character/Enemy/claySoldier/ClaySoldierController.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller != null && walkClip != null)
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

        Debug.Log("Done!");
    }
}
