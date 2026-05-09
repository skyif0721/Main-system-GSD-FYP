using UnityEngine;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;

public class CheckOpenXR
{
    public static void Execute()
    {
        XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        if (settings != null && settings.Manager != null)
        {
            Debug.Log("Android XR Loaders:");
            foreach (var loader in settings.Manager.activeLoaders)
            {
                Debug.Log("- " + loader.name);
            }
        }
        else
        {
            Debug.Log("No XR settings found for Android");
        }

        settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
        if (settings != null && settings.Manager != null)
        {
            Debug.Log("Standalone XR Loaders:");
            foreach (var loader in settings.Manager.activeLoaders)
            {
                Debug.Log("- " + loader.name);
            }
        }
        else
        {
            Debug.Log("No XR settings found for Standalone");
        }
    }
}