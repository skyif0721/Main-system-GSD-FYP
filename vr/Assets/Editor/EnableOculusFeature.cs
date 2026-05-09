using UnityEngine;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;

public class EnableOculusFeature
{
    public static void Execute()
    {
        var features = FeatureHelpers.GetFeaturesWithIdsForBuildTarget(BuildTargetGroup.Android, new string[0]);
        foreach (var feature in features)
        {
            if (feature.name.Contains("Oculus") || feature.name.Contains("Meta"))
            {
                feature.enabled = true;
                Debug.Log("Enabled feature: " + feature.name);
            }
        }
        
        features = FeatureHelpers.GetFeaturesWithIdsForBuildTarget(BuildTargetGroup.Standalone, new string[0]);
        foreach (var feature in features)
        {
            if (feature.name.Contains("Oculus") || feature.name.Contains("Meta"))
            {
                feature.enabled = true;
                Debug.Log("Enabled feature: " + feature.name);
            }
        }
    }
}