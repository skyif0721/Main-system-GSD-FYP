using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class CheckInputActionManager
{
    public static void Execute()
    {
        GameObject origin = GameObject.Find("Complete XR Origin Set Up Variant");
        if (origin != null)
        {
            InputActionManager iam = origin.GetComponent<InputActionManager>();
            if (iam != null)
            {
                Debug.Log("InputActionManager found. Action Assets count: " + (iam.actionAssets != null ? iam.actionAssets.Count : 0));
                if (iam.actionAssets != null)
                {
                    foreach (var asset in iam.actionAssets)
                    {
                        Debug.Log("Asset: " + (asset != null ? asset.name : "null"));
                    }
                }
            }
            else
            {
                Debug.Log("No InputActionManager found");
            }
        }
        else
        {
            Debug.Log("XR Origin not found");
        }
    }
}