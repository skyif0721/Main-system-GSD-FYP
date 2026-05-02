using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class FixWalkClip
{
    public static void Execute()
    {
        string controllerPath = "Assets/Game object/character/Enemy/claySoldier/ClaySoldierController.controller";
        string animFolderPath = "Assets/Game object/character/Enemy/claySoldier/New Folder";

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null) { Debug.LogError("Controller not found!"); return; }

        // Load all clips
        AnimationClip idleClip   = GetClip(animFolderPath + "/Idle (1).fbx");
        AnimationClip walkClip   = GetClip(animFolderPath + "/Mutant Walking.fbx");
        AnimationClip attackClip = GetClip(animFolderPath + "/Fist Fight B.fbx");
        AnimationClip hitClip    = GetClip(animFolderPath + "/Hit To Body.fbx");
        AnimationClip dieClip    = GetClip(animFolderPath + "/Sword And Shield Death.fbx");

        Debug.Log($"Idle: {idleClip?.name} | Walk: {walkClip?.name} | Attack: {attackClip?.name} | Hit: {hitClip?.name} | Die: {dieClip?.name}");

        // Assign clips to states
        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState s in sm.states)
        {
            switch (s.state.name)
            {
                case "Idle":   s.state.motion = idleClip;   break;
                case "Walk":   s.state.motion = walkClip;   break;
                case "Attack": s.state.motion = attackClip; break;
                case "Hit":    s.state.motion = hitClip;    break;
                case "Die":    s.state.motion = dieClip;    break;
            }
            Debug.Log($"Assigned {s.state.name} -> {s.state.motion?.name ?? "NULL"}");
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Walk clip fix complete!");
    }

    private static AnimationClip GetClip(string path)
    {
        // Prefer the 'mixamo.com' named clip
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset is AnimationClip c && c.name == "mixamo.com")
                return c;
        }
        // Fallback to first non-preview clip
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset is AnimationClip c && !c.name.StartsWith("__preview__"))
                return c;
        }
        Debug.LogWarning("No clip found in: " + path);
        return null;
    }
}
