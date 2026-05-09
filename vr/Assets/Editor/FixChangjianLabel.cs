using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class FixChangjianLabel
{
    public static void Execute()
    {
        // Find Row_11_Changjian
        GameObject[] allGOs = Object.FindObjectsOfType<GameObject>(true);
        GameObject row = null;
        foreach (var go in allGOs)
        {
            if (go.name == "Row_11_Changjian")
            {
                row = go;
                break;
            }
        }

        if (row == null) { Debug.LogError("Row_11_Changjian not found!"); return; }

        // Update name label to English (font doesn't support Chinese)
        var nameText = row.transform.Find("NameText/Lbl")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = "Changjian";
            EditorUtility.SetDirty(nameText);
            Debug.Log("Updated Row_11_Changjian name label to 'Changjian'");
        }

        // Also update WeaponShopManager weapon name to match
        WeaponShopManager wsm = Object.FindObjectOfType<WeaponShopManager>();
        if (wsm != null && wsm.weapons.Count > 11)
        {
            wsm.weapons[11].weaponName = "Changjian";
            EditorUtility.SetDirty(wsm);
            Debug.Log("Updated WeaponShopManager[11] name to 'Changjian'");
        }

        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Done. Scene saved.");
    }
}
