using UnityEngine;
using UnityEditor;
using TMPro;

public class FixWeaponShopHeader
{
    [MenuItem("Tools/Fix Weapon Shop Header")]
    public static void Fix()
    {
        GameObject shopMenuCanvas = GameObject.Find("ShopMenuCanvas");
        if (shopMenuCanvas == null) { Debug.LogError("ShopMenuCanvas not found."); return; }

        Transform panel = shopMenuCanvas.transform.Find("WeaponShopPanel");
        if (panel == null) { Debug.LogError("WeaponShopPanel not found."); return; }

        // ── 1. Push Header down so it doesn't overlap title ───────────────────
        // Header is 72px tall at top — move ColHeader down to y=80 so it's below title
        Transform colHdr = panel.Find("ColHeader");
        if (colHdr != null)
        {
            var rt = colHdr.GetComponent<RectTransform>();
            // Move it to sit at y=80 (below the 72px header)
            rt.anchoredPosition = new Vector2(0, -80f);
            rt.sizeDelta = new Vector2(0, -116f); // top=80, bottom=116 → 36px tall
            EditorUtility.SetDirty(colHdr.gameObject);
        }

        // ── 2. Fix StatusBar position ─────────────────────────────────────────
        Transform statusBar = panel.Find("StatusBar");
        if (statusBar != null)
        {
            // Hide status bar — it overlaps. We'll show status inside rows only.
            statusBar.gameObject.SetActive(false);
            EditorUtility.SetDirty(statusBar.gameObject);
        }

        // ── 3. Fix ScrollArea to start below ColHeader (y=116) ────────────────
        Transform scrollArea = panel.Find("ScrollArea");
        if (scrollArea != null)
        {
            var rt = scrollArea.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, -116f);
            EditorUtility.SetDirty(scrollArea.gameObject);
        }

        // ── 4. Fix Header — make title not overlap column headers ─────────────
        Transform hdr = panel.Find("Header");
        if (hdr != null)
        {
            // Ensure header is exactly 76px
            var rt = hdr.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 76f);
            EditorUtility.SetDirty(hdr.gameObject);

            // Fix CoinDisplay — move it left so it's fully visible
            Transform coinDisplay = hdr.Find("CoinDisplay");
            if (coinDisplay != null)
            {
                var crt = coinDisplay.GetComponent<RectTransform>();
                crt.anchorMin = new Vector2(1, 0);
                crt.anchorMax = new Vector2(1, 1);
                crt.pivot = new Vector2(1, 0.5f);
                crt.anchoredPosition = new Vector2(-115f, 0);
                crt.sizeDelta = new Vector2(200f, 0);
                EditorUtility.SetDirty(coinDisplay.gameObject);

                // Make text right-aligned
                var tmp = coinDisplay.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.alignment = TextAlignmentOptions.MidlineRight;
            }

            // Fix Close button — ensure it's fully inside
            Transform closeBtn = hdr.Find("CloseBtn");
            if (closeBtn != null)
            {
                var brt = closeBtn.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(1, 1);
                brt.anchorMax = new Vector2(1, 1);
                brt.pivot = new Vector2(1, 1);
                brt.anchoredPosition = new Vector2(-4f, -4f);
                brt.sizeDelta = new Vector2(108f, 68f);
                EditorUtility.SetDirty(closeBtn.gameObject);
            }

            // Fix Title — keep it left, not overlapping close button
            Transform title = hdr.Find("Title");
            if (title != null)
            {
                var trt = title.GetComponent<RectTransform>();
                trt.anchorMin = new Vector2(0, 0);
                trt.anchorMax = new Vector2(1, 1);
                trt.offsetMin = new Vector2(12, 0);
                trt.offsetMax = new Vector2(-320f, 0);
                EditorUtility.SetDirty(title.gameObject);
            }
        }

        // ── 5. Fix ColHeader — push column labels down slightly ───────────────
        if (colHdr != null)
        {
            var rt = colHdr.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -76f);
            rt.sizeDelta = new Vector2(0, -116f);
            EditorUtility.SetDirty(colHdr.gameObject);
        }

        EditorUtility.SetDirty(shopMenuCanvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[FixWeaponShopHeader] Done!");
    }
}
