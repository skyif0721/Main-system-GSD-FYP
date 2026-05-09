using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class CheckActionMap
{
    public static void Execute()
    {
        GameObject origin = GameObject.Find("Complete XR Origin Set Up Variant");
        if (origin != null)
        {
            InputActionManager iam = origin.GetComponent<InputActionManager>();
            if (iam != null && iam.actionAssets != null && iam.actionAssets.Count > 0)
            {
                InputActionAsset asset = iam.actionAssets[0];
                InputActionMap map = asset.FindActionMap("XRI Head");
                if (map != null)
                {
                    Debug.Log("XRI Head map found. Actions:");
                    foreach (var action in map.actions)
                    {
                        Debug.Log("- " + action.name + " bindings count: " + action.bindings.Count);
                        foreach (var binding in action.bindings)
                        {
                            Debug.Log("  - " + binding.path);
                        }
                    }
                }
                else
                {
                    Debug.Log("XRI Head map not found");
                }
            }
        }
    }
}