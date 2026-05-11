using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 1. Adds the green cross health potion as a repeatable buy item in the shop.
/// 2. Fixes the NPC dialogue text color (black → white).
/// 3. Creates a "final boss" scene by duplicating shop-training.
/// </summary>
public class ShopAndBossSetup
{
    [MenuItem("Tools/Setup Shop Potion + Fix Text")]
    public static void SetupShopAndFixText()
    {
        // Fix NPC dialogue text color: black → white
        var npcTextGO = GameObject.Find("VR_Shop_Zone/NPCDialogueCanvas/Text");
        if (npcTextGO != null)
        {
            var uiText = npcTextGO.GetComponent<Text>();
            if (uiText != null)
            {
                uiText.color = Color.white;
                uiText.fontSize = 24;
                EditorUtility.SetDirty(uiText);
                Debug.Log("[Setup] Fixed NPC dialogue text color to white.");
            }
        }

        // Add health potion row to the shop UI
        AddHealthPotionToShop();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Setup] Shop potion added and text fixed. Scene saved.");
    }

    static void AddHealthPotionToShop()
    {
        // Find the scroll content parent
        var contentGO = GameObject.Find("VR_Shop_Zone/ShopMenuCanvas/WeaponShopPanel/ScrollArea/Viewport/Content");
        if (contentGO == null)
        {
            Debug.LogWarning("[Setup] Could not find shop scroll content!");
            return;
        }

        // Check if potion row already exists
        if (contentGO.transform.Find("Row_Potion") != null)
        {
            Debug.Log("[Setup] Potion row already exists, skipping.");
            return;
        }

        // Find an existing row to use as template for sizing
        Transform templateRow = contentGO.transform.Find("Row_00_Dagger");
        if (templateRow == null)
        {
            Debug.LogWarning("[Setup] Could not find template row!");
            return;
        }

        // Get template sizing
        RectTransform templateRT = templateRow.GetComponent<RectTransform>();
        float rowHeight = templateRT.sizeDelta.y;

        // Count existing rows to position the new one
        int existingRows = contentGO.transform.childCount;

        // Create the potion row
        GameObject potionRow = new GameObject("Row_Potion");
        potionRow.transform.SetParent(contentGO.transform, false);
        RectTransform rowRT = potionRow.AddComponent<RectTransform>();
        rowRT.anchorMin = new Vector2(0, 1);
        rowRT.anchorMax = new Vector2(1, 1);
        rowRT.pivot = new Vector2(0.5f, 1f);
        rowRT.sizeDelta = new Vector2(-16f, rowHeight);
        float yPos = -4f - (existingRows * (rowHeight + 4f));
        rowRT.anchoredPosition = new Vector2(0, yPos);

        potionRow.AddComponent<CanvasRenderer>();
        Image rowImg = potionRow.AddComponent<Image>();
        rowImg.color = new Color(0.05f, 0.25f, 0.10f, 0.90f);

        // Badge (number)
        GameObject badge = new GameObject("Badge");
        badge.transform.SetParent(potionRow.transform, false);
        RectTransform badgeRT = badge.AddComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(0, 0);
        badgeRT.anchorMax = new Vector2(0, 1);
        badgeRT.pivot = new Vector2(0, 0.5f);
        badgeRT.anchoredPosition = new Vector2(8, 0);
        badgeRT.sizeDelta = new Vector2(36, -8);
        badge.AddComponent<CanvasRenderer>();
        Image badgeImg = badge.AddComponent<Image>();
        badgeImg.color = new Color(0.2f, 0.8f, 0.3f, 1f);

        GameObject badgeText = new GameObject("Text");
        badgeText.transform.SetParent(badge.transform, false);
        RectTransform badgeTextRT = badgeText.AddComponent<RectTransform>();
        badgeTextRT.anchorMin = Vector2.zero;
        badgeTextRT.anchorMax = Vector2.one;
        badgeTextRT.offsetMin = Vector2.zero;
        badgeTextRT.offsetMax = Vector2.zero;
        badgeText.AddComponent<CanvasRenderer>();
        var badgeTMP = badgeText.AddComponent<TextMeshProUGUI>();
        badgeTMP.text = "+";
        badgeTMP.fontSize = 18;
        badgeTMP.fontStyle = FontStyles.Bold;
        badgeTMP.color = Color.white;
        badgeTMP.alignment = TextAlignmentOptions.Center;

        // Name text
        GameObject nameParent = new GameObject("NameText");
        nameParent.transform.SetParent(potionRow.transform, false);
        RectTransform nameParentRT = nameParent.AddComponent<RectTransform>();
        nameParentRT.anchorMin = new Vector2(0, 0);
        nameParentRT.anchorMax = new Vector2(0, 1);
        nameParentRT.pivot = new Vector2(0, 0.5f);
        nameParentRT.anchoredPosition = new Vector2(52, 0);
        nameParentRT.sizeDelta = new Vector2(140, 0);

        GameObject nameText = new GameObject("Text");
        nameText.transform.SetParent(nameParent.transform, false);
        RectTransform nameTextRT = nameText.AddComponent<RectTransform>();
        nameTextRT.anchorMin = Vector2.zero;
        nameTextRT.anchorMax = Vector2.one;
        nameTextRT.offsetMin = Vector2.zero;
        nameTextRT.offsetMax = Vector2.zero;
        nameText.AddComponent<CanvasRenderer>();
        var nameTMP = nameText.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Health Potion";
        nameTMP.fontSize = 16;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.color = new Color(0.3f, 1f, 0.4f);
        nameTMP.alignment = TextAlignmentOptions.Left;
        nameTMP.enableWordWrapping = false;

        // Price text
        GameObject priceParent = new GameObject("PriceText");
        priceParent.transform.SetParent(potionRow.transform, false);
        RectTransform priceParentRT = priceParent.AddComponent<RectTransform>();
        priceParentRT.anchorMin = new Vector2(0, 0);
        priceParentRT.anchorMax = new Vector2(0, 1);
        priceParentRT.pivot = new Vector2(0, 0.5f);
        priceParentRT.anchoredPosition = new Vector2(200, 0);
        priceParentRT.sizeDelta = new Vector2(100, 0);

        GameObject priceText = new GameObject("Text");
        priceText.transform.SetParent(priceParent.transform, false);
        RectTransform priceTextRT = priceText.AddComponent<RectTransform>();
        priceTextRT.anchorMin = Vector2.zero;
        priceTextRT.anchorMax = Vector2.one;
        priceTextRT.offsetMin = Vector2.zero;
        priceTextRT.offsetMax = Vector2.zero;
        priceText.AddComponent<CanvasRenderer>();
        var priceTMP = priceText.AddComponent<TextMeshProUGUI>();
        priceTMP.text = "15 coins";
        priceTMP.fontSize = 14;
        priceTMP.color = new Color(1f, 0.84f, 0f);
        priceTMP.alignment = TextAlignmentOptions.Center;

        // Status text
        GameObject statusParent = new GameObject("StatusText");
        statusParent.transform.SetParent(potionRow.transform, false);
        RectTransform statusParentRT = statusParent.AddComponent<RectTransform>();
        statusParentRT.anchorMin = new Vector2(0, 0);
        statusParentRT.anchorMax = new Vector2(0, 1);
        statusParentRT.pivot = new Vector2(0, 0.5f);
        statusParentRT.anchoredPosition = new Vector2(310, 0);
        statusParentRT.sizeDelta = new Vector2(100, 0);

        GameObject statusText = new GameObject("Text");
        statusText.transform.SetParent(statusParent.transform, false);
        RectTransform statusTextRT = statusText.AddComponent<RectTransform>();
        statusTextRT.anchorMin = Vector2.zero;
        statusTextRT.anchorMax = Vector2.one;
        statusTextRT.offsetMin = Vector2.zero;
        statusTextRT.offsetMax = Vector2.zero;
        statusText.AddComponent<CanvasRenderer>();
        var statusTMP = statusText.AddComponent<TextMeshProUGUI>();
        statusTMP.text = "Repeatable";
        statusTMP.fontSize = 12;
        statusTMP.color = new Color(0.3f, 1f, 0.3f);
        statusTMP.alignment = TextAlignmentOptions.Center;

        // Buy button
        GameObject buyBtn = new GameObject("BuyButton");
        buyBtn.transform.SetParent(potionRow.transform, false);
        RectTransform buyBtnRT = buyBtn.AddComponent<RectTransform>();
        buyBtnRT.anchorMin = new Vector2(1, 0);
        buyBtnRT.anchorMax = new Vector2(1, 1);
        buyBtnRT.pivot = new Vector2(1, 0.5f);
        buyBtnRT.anchoredPosition = new Vector2(-8, 0);
        buyBtnRT.sizeDelta = new Vector2(80, -8);
        buyBtn.AddComponent<CanvasRenderer>();
        Image buyBtnImg = buyBtn.AddComponent<Image>();
        buyBtnImg.color = new Color(0.10f, 0.65f, 0.20f, 1f);
        Button buyButton = buyBtn.AddComponent<Button>();

        GameObject buyBtnText = new GameObject("Text");
        buyBtnText.transform.SetParent(buyBtn.transform, false);
        RectTransform buyBtnTextRT = buyBtnText.AddComponent<RectTransform>();
        buyBtnTextRT.anchorMin = Vector2.zero;
        buyBtnTextRT.anchorMax = Vector2.one;
        buyBtnTextRT.offsetMin = Vector2.zero;
        buyBtnTextRT.offsetMax = Vector2.zero;
        buyBtnText.AddComponent<CanvasRenderer>();
        var buyBtnTMP = buyBtnText.AddComponent<TextMeshProUGUI>();
        buyBtnTMP.text = "BUY";
        buyBtnTMP.fontSize = 16;
        buyBtnTMP.fontStyle = FontStyles.Bold;
        buyBtnTMP.color = Color.white;
        buyBtnTMP.alignment = TextAlignmentOptions.Center;

        // Expand the content area to fit the new row
        RectTransform contentRT = contentGO.GetComponent<RectTransform>();
        float newHeight = Mathf.Abs(yPos) + rowHeight + 8f;
        if (contentRT.sizeDelta.y < newHeight)
            contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, newHeight);

        Debug.Log("[Setup] Added Health Potion row to shop UI.");
    }

    [MenuItem("Tools/Create Final Boss Scene")]
    public static void CreateFinalBossScene()
    {
        // Save current scene
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        // Copy shop-training to final boss
        string srcPath = "Assets/shop-training.unity";
        string dstPath = "Assets/SCENES/final-boss.unity";

        if (!System.IO.File.Exists(srcPath))
        {
            Debug.LogError("[Setup] shop-training.unity not found!");
            return;
        }

        AssetDatabase.CopyAsset(srcPath, dstPath);
        AssetDatabase.Refresh();

        // Add to build settings
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool found = false;
        foreach (var s in scenes)
        {
            if (s.path == dstPath) { found = true; s.enabled = true; break; }
        }
        if (!found)
        {
            scenes.Add(new EditorBuildSettingsScene(dstPath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        Debug.Log("[Setup] Final boss scene created at " + dstPath);

        // Open the new scene
        EditorSceneManager.OpenScene(dstPath);
    }
}
