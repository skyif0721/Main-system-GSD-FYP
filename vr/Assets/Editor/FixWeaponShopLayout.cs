using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixWeaponShopLayout
{
    [MenuItem("Tools/Fix Weapon Shop Layout")]
    public static void Fix()
    {
        // ── 1. Reset Content position so rows are visible ─────────────────────
        GameObject content = GameObject.Find("Content");
        if (content != null)
        {
            RectTransform rt = content.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                EditorUtility.SetDirty(content);
                Debug.Log("[FixWeaponShopLayout] Reset Content anchoredPosition to zero.");
            }
        }

        // ── 2. Hide old BuyHealthButton (keep it, just disable it) ────────────
        GameObject buyHealth = GameObject.Find("BuyHealthButton");
        if (buyHealth != null)
        {
            buyHealth.SetActive(false);
            EditorUtility.SetDirty(buyHealth);
            Debug.Log("[FixWeaponShopLayout] Hid old BuyHealthButton.");
        }

        // ── 3. Hide old Title and CloseButton (replaced by WeaponShopPanel) ───
        // Find the OLD Title/CloseButton that are direct children of ShopMenuCanvas
        GameObject shopMenuCanvas = GameObject.Find("ShopMenuCanvas");
        if (shopMenuCanvas != null)
        {
            Transform oldTitle = shopMenuCanvas.transform.Find("Title");
            if (oldTitle != null) { oldTitle.gameObject.SetActive(false); EditorUtility.SetDirty(oldTitle.gameObject); }

            Transform oldClose = shopMenuCanvas.transform.Find("CloseButton");
            if (oldClose != null) { oldClose.gameObject.SetActive(false); EditorUtility.SetDirty(oldClose.gameObject); }

            Transform oldBg = shopMenuCanvas.transform.Find("Background");
            if (oldBg != null) { oldBg.gameObject.SetActive(false); EditorUtility.SetDirty(oldBg.gameObject); }

            EditorUtility.SetDirty(shopMenuCanvas);
        }

        // ── 4. Ensure WeaponShopPanel fills the canvas ────────────────────────
        GameObject panel = GameObject.Find("WeaponShopPanel");
        if (panel != null)
        {
            RectTransform prt = panel.GetComponent<RectTransform>();
            if (prt != null)
            {
                prt.anchorMin = Vector2.zero;
                prt.anchorMax = Vector2.one;
                prt.offsetMin = Vector2.zero;
                prt.offsetMax = Vector2.zero;
                EditorUtility.SetDirty(panel);
                Debug.Log("[FixWeaponShopLayout] WeaponShopPanel set to fill canvas.");
            }
        }

        // ── 5. Fix ScrollView to sit below header (y=160 from bottom, top=full) 
        GameObject scrollView = GameObject.Find("ScrollView");
        if (scrollView != null)
        {
            RectTransform srt = scrollView.GetComponent<RectTransform>();
            if (srt != null)
            {
                srt.anchorMin = new Vector2(0, 0);
                srt.anchorMax = new Vector2(1, 1);
                srt.offsetMin = new Vector2(0, 0);
                srt.offsetMax = new Vector2(0, -165);
                EditorUtility.SetDirty(scrollView);
                Debug.Log("[FixWeaponShopLayout] ScrollView rect fixed.");
            }
        }

        // ── 6. Mark scene dirty ───────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[FixWeaponShopLayout] ✅ Layout fixed!");
    }
}
