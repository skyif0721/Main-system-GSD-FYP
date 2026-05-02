using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class VerifyClaySoldier
{
    public static void Execute()
    {
        string animFolderPath = "Assets/Game object/character/Enemy/claySoldier/New Folder";
        
        // Log all clips found in each FBX
        string[] fbxFiles = {
            animFolderPath + "/Idle (1).fbx",
            animFolderPath + "/Mutant Walking.fbx",
            animFolderPath + "/Fist Fight B.fbx",
            animFolderPath + "/Hit To Body.fbx",
            animFolderPath + "/Sword And Shield Death.fbx"
        };

        foreach (string fbx in fbxFiles)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbx);
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip && !asset.name.StartsWith("__preview__"))
                {
                    Debug.Log("Found clip: '" + asset.name + "' in " + fbx);
                }
            }
        }

        // Verify the controller
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
            "Assets/Game object/character/Enemy/claySoldier/ClaySoldierController.controller");
        if (controller != null)
        {
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                foreach (ChildAnimatorState state in layer.stateMachine.states)
                {
                    string motionName = state.state.motion != null ? state.state.motion.name : "NULL";
                    Debug.Log("State: " + state.state.name + " -> Motion: " + motionName);
                }
            }
        }

        // Verify spawner
        GameObject spawnerObj = GameObject.Find("MonsterSpawner");
        if (spawnerObj != null)
        {
            MonsterSpawner spawner = spawnerObj.GetComponent<MonsterSpawner>();
            if (spawner != null && spawner.monsterPrefab != null)
            {
                Debug.Log("MonsterSpawner is using prefab: " + spawner.monsterPrefab.name);
            }
        }
    }
}
