using UnityEngine;
using UnityEditor;
using System.IO;

public class OrganizeAssets
{
    public static void Execute()
    {
        // Ensure target directories exist
        EnsureDirectory("Assets/Scripts");
        EnsureDirectory("Assets/Documentation");

        // Move loose scripts
        MoveAsset("Assets/MovementRecognitier.cs", "Assets/Scripts/MovementRecognitier.cs");
        MoveAsset("Assets/InputSystem_Actions.cs", "Assets/Scripts/InputSystem_Actions.cs");
        
        // Move loose markdown/text files
        MoveAsset("Assets/CoplayPlan.md", "Assets/Documentation/CoplayPlan.md");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Assets organized successfully!");
    }

    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static void MoveAsset(string oldPath, string newPath)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(oldPath) != null)
        {
            string result = AssetDatabase.MoveAsset(oldPath, newPath);
            if (string.IsNullOrEmpty(result))
            {
                Debug.Log($"Moved {oldPath} to {newPath}");
            }
            else
            {
                Debug.LogError($"Failed to move {oldPath}: {result}");
            }
        }
    }
}
