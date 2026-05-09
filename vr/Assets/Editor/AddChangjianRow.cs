using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class AddChangjianRow
{
    public static void Execute()
    {
        // Find the scroll content
        GameObject content = null;
        var allGOs = Object.FindObjectsOfType<GameObject>(true);
        foreach (var go in allGOs)
        {
            if (go.name == "Content" && go.transform.parent != null &&
                go.transform.parent.name == "Viewport")
            {
                content = go;
                break;
            }
        }
        if (content == null) { Debug.LogError("Content not found!"); return; }

        // Check if row already exists
        if (content.transform.Find("Row_11_Changjian") != null)
        {
            Debug.Log("Row_11_Changjian already exists.");
        }
        else
        {
            // Duplicate Row_10_Halberd as template
            Transform template = content.transform.Find("Row_10_Halberd");
            if (template == null) { Debug.LogError("Row_10_Halberd not found!"); return; }

            GameObject newRow = Object.Instantiate(template.gameObject, content.transform);
            newRow.name = "Row_11_Changjian";

            // Update name label
            var nameText = newRow.transform.Find("NameText/Lbl")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) nameText.text = "长剑";

            // Update price label
            var priceText = newRow.transform.Find("PriceText/Lbl")?.GetComponent<TextMeshProUGUI>();
            if (priceText != null) priceText.text = "350 coins";

            // Update badge label
            var badgeText = newRow.transform.Find("Badge/Lbl")?.GetComponent<TextMeshProUGUI>();
            if (badgeText != null) badgeText.text = "12";

            // Clear status text
            var statusText = newRow.transform.Find("StatusText/Lbl")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null) statusText.text = "";

            // Make sure it's at the bottom of the list
            newRow.transform.SetAsLastSibling();

            EditorUtility.SetDirty(content);
            Debug.Log("Created Row_11_Changjian");
        }

        // Now wire it into WeaponShopManager
        WeaponShopManager wsm = Object.FindObjectOfType<WeaponShopManager>();
        if (wsm != null)
        {
            Transform rowT = content.transform.Find("Row_11_Changjian");
            if (rowT != null && wsm.weapons.Count > 11)
            {
                wsm.weapons[11].rowUI = rowT.gameObject;
                EditorUtility.SetDirty(wsm);
                Debug.Log("Wired Row_11_Changjian into WeaponShopManager[11]");
            }
        }

        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Done. Scene saved.");
    }
}
