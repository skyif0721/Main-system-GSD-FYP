using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class OrganizeScene
{
    public static void Execute()
    {
        // Create category folders (Empty GameObjects)
        GameObject envFolder = GetOrCreateFolder("--- ENVIRONMENT ---");
        GameObject weaponsFolder = GetOrCreateFolder("--- WEAPONS ---");
        GameObject uiFolder = GetOrCreateFolder("--- UI ---");
        GameObject managersFolder = GetOrCreateFolder("--- MANAGERS ---");
        GameObject charactersFolder = GetOrCreateFolder("--- CHARACTERS ---");
        GameObject playerFolder = GetOrCreateFolder("--- PLAYER ---");

        // Get all root objects
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        foreach (GameObject go in rootObjects)
        {
            // Skip the folders themselves
            if (go.name.StartsWith("--- ")) continue;

            // Categorize and parent
            if (go.name.Contains("Manager") || go.name == "EventSystem" || go.name == "Script Holder" || go.name == "XRInteractionManager")
            {
                go.transform.SetParent(managersFolder.transform);
            }
            else if (go.name.Contains("PlayerUI") || go.name.Contains("Canvas"))
            {
                go.transform.SetParent(uiFolder.transform);
            }
            else if (go.name.Contains("Weapon") || go.name.Contains("Mallet") || go.name == "Untitled")
            {
                go.transform.SetParent(weaponsFolder.transform);
            }
            else if (go.name.Contains("NPC") || go.name.Contains("Monster") || go.name == "-- Game object --")
            {
                go.transform.SetParent(charactersFolder.transform);
            }
            else if (go.name.Contains("XR Origin"))
            {
                go.transform.SetParent(playerFolder.transform);
            }
            else if (go.name == "Cube" || go.name == "NavMesh" || go.name == "GameObject" || go.name == "Trail")
            {
                go.transform.SetParent(envFolder.transform);
            }
            else
            {
                // Default to environment if unsure, to keep root clean
                go.transform.SetParent(envFolder.transform);
            }
        }

        Debug.Log("Scene hierarchy organized successfully!");
    }

    private static GameObject GetOrCreateFolder(string name)
    {
        GameObject folder = GameObject.Find(name);
        if (folder == null)
        {
            folder = new GameObject(name);
            // Reset transform
            folder.transform.position = Vector3.zero;
            folder.transform.rotation = Quaternion.identity;
            folder.transform.localScale = Vector3.one;
        }
        return folder;
    }
}
