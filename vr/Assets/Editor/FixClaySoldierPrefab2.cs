using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class FixClaySoldierPrefab2
{
    public static void Execute()
    {
        string prefabPath = "Assets/Prefabs/ClaySoldierEnemy.prefab";
        
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError("Could not load ClaySoldierEnemy prefab.");
            return;
        }

        // Find Camera child and destroy the whole GameObject
        Transform cameraChild = prefabRoot.transform.Find("Camera");
        if (cameraChild != null)
        {
            Object.DestroyImmediate(cameraChild.gameObject);
            Debug.Log("Removed Camera child GameObject.");
        }

        // Find Light child and destroy the whole GameObject
        Transform lightChild = prefabRoot.transform.Find("Light");
        if (lightChild != null)
        {
            Object.DestroyImmediate(lightChild.gameObject);
            Debug.Log("Removed Light child GameObject.");
        }

        // Also search recursively in case they're nested deeper
        foreach (Transform child in prefabRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child == null || child.gameObject == prefabRoot) continue;
            if (child.GetComponent<Camera>() != null)
            {
                Debug.Log("Removing nested Camera: " + child.name);
                Object.DestroyImmediate(child.gameObject);
            }
            else if (child.GetComponent<Light>() != null)
            {
                Debug.Log("Removing nested Light: " + child.name);
                Object.DestroyImmediate(child.gameObject);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        // Fix Walk animation to loop
        string walkFbxPath = "Assets/Game object/character/Enemy/claySoldier/New Folder/Mutant Walking.fbx";
        ModelImporter importer = AssetImporter.GetAtPath(walkFbxPath) as ModelImporter;
        if (importer != null)
        {
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.defaultClipAnimations;

            bool changed = false;
            foreach (var clip in clips)
            {
                if (!clip.loopTime)
                {
                    clip.loopTime = true;
                    changed = true;
                    Debug.Log("Set loop on clip: " + clip.name);
                }
            }

            if (changed)
            {
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
                Debug.Log("Reimported Mutant Walking.fbx with loop enabled.");
            }
        }

        // Also fix Idle to loop
        string idleFbxPath = "Assets/Game object/character/Enemy/claySoldier/New Folder/Idle (1).fbx";
        ModelImporter idleImporter = AssetImporter.GetAtPath(idleFbxPath) as ModelImporter;
        if (idleImporter != null)
        {
            ModelImporterClipAnimation[] clips = idleImporter.clipAnimations;
            if (clips == null || clips.Length == 0)
                clips = idleImporter.defaultClipAnimations;

            bool changed = false;
            foreach (var clip in clips)
            {
                if (!clip.loopTime)
                {
                    clip.loopTime = true;
                    changed = true;
                    Debug.Log("Set loop on idle clip: " + clip.name);
                }
            }

            if (changed)
            {
                idleImporter.clipAnimations = clips;
                idleImporter.SaveAndReimport();
                Debug.Log("Reimported Idle.fbx with loop enabled.");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("ClaySoldierEnemy prefab fixed: Camera/Light removed, Walk/Idle set to loop!");
    }
}
