using UnityEngine;
using UnityEditor;
using System.IO;

public class FixNewbieMaterial
{
    public static void Execute()
    {
        string path = "Assets/Prefabs/newbie.fbx";
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        
        if (importer != null)
        {
            // Ensure the Materials folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Materials"))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Materials");
            }

            // Create a new material
            Material newMat = new Material(Shader.Find("Standard"));
            newMat.color = Color.white;
            
            AssetDatabase.CreateAsset(newMat, "Assets/Prefabs/Materials/NewbieMat.mat");
            AssetDatabase.SaveAssets();
            
            Debug.Log("Created new material for newbie.");
        }
        else
        {
            Debug.LogError("newbie.fbx not found or not a model.");
        }
    }
}
