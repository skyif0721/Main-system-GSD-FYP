using UnityEngine;
using UnityEditor;

public class CheckNewbieMaterials
{
    public static void Execute()
    {
        string path = "Assets/Prefabs/newbie.fbx";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (m != null)
                    {
                        Debug.Log($"Renderer: {r.name}, Material: {m.name}, Shader: {m.shader.name}");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("newbie.fbx not found.");
        }
    }
}
