using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class CheckAnimatorController
{
    public static void Execute()
    {
        RuntimeAnimatorController runtimeController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Prefabs/NewbieController.controller");
        if (runtimeController is AnimatorController controller)
        {
            Debug.Log("Parameters:");
            foreach (var param in controller.parameters)
            {
                Debug.Log($"- {param.name} ({param.type})");
            }
        }
        else
        {
            Debug.LogError("Could not load AnimatorController.");
        }
    }
}
