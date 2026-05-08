using UnityEngine;
using UnityEditor;

public class FixNewbieMaterial
{
    public static void Execute()
    {
        string path = "Assets/Prefabs/newbie.fbx";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            // Create a new material
            Material newMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (newMat.shader == null)
            {
                newMat = new Material(Shader.Find("Standard"));
            }
            newMat.color = Color.blue; // Give it a default color
            
            AssetDatabase.CreateAsset(newMat, "Assets/Prefabs/NewbieMaterial.mat");
            
            // We can't directly modify FBX materials, we need to use ModelImporter
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                importer.SaveAndReimport();
                
                // Map the material
                var externalObjects = importer.GetExternalObjectMap();
                // This is complex, let's just instantiate it and change the material in the scene
            }
        }
    }
}
