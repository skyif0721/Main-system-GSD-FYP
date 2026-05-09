using UnityEngine;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;

public class CheckOpenXRFeatures
{
    public static void Execute()
    {
        var features = FeatureHelpers.GetFeaturesWithIdsForBuildTarget(BuildTargetGroup.Android, new string[0]);
        Debug.Log("Android OpenXR Features:");
        foreach (var feature in features)
        {
            if (feature.enabled)
            {
                Debug.Log("- " + feature.name + " | Enabled: " + feature.enabled);
            }
        }
    }
}