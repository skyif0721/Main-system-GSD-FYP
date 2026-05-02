using UnityEngine;
using UnityEditor;

public class InspectWalkFBX
{
    public static void Execute()
    {
        string path = "Assets/Game object/character/Enemy/claySoldier/New Folder/Mutant Walking.fbx";
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        Debug.Log($"Total assets in Mutant Walking.fbx: {all.Length}");
        foreach (Object obj in all)
        {
            Debug.Log($"  Type: {obj.GetType().Name} | Name: '{obj.name}'");
        }

        // Also check the ModelImporter
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer != null)
        {
            Debug.Log($"ModelImporter clipAnimations count: {importer.clipAnimations.Length}");
            Debug.Log($"ModelImporter defaultClipAnimations count: {importer.defaultClipAnimations.Length}");
            foreach (var clip in importer.defaultClipAnimations)
            {
                Debug.Log($"  Default clip: '{clip.name}' loop={clip.loopTime} firstFrame={clip.firstFrame} lastFrame={clip.lastFrame}");
            }
            foreach (var clip in importer.clipAnimations)
            {
                Debug.Log($"  Custom clip: '{clip.name}' loop={clip.loopTime}");
            }
        }
    }
}
