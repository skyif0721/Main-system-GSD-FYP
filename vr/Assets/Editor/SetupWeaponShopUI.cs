using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Builds the full Weapon Shop UI inside the existing ShopMenuCanvas.
/// Run via Tools > Setup Weapon Shop UI.
/// </summary>
public class SetupWeaponShopUI
{
    static readonly (string name, int price, string sceneObj)[] WEAPONS =
    {
        ("Dagger",     20,  "01 Dagger.002"),
        ("Sword",      40,  "02 Sword.002"),
        ("Long Sword", 60,  "03 Long Sword.002"),
        ("Axe",        80,  "04 Axe.002"),
        ("Battleaxe",  100, "05 Battleaxe.002"),
        ("Mace",       120, "06 Mace.002"),
        ("Heavy Mace", 150, "07 Heavy Mace.002"),
        ("Hammer",     180, "08 Hammer.002"),
        ("Warhammer",  220, "09 Warhammer.002"),
        ("Spear",      260, "10 Spear.002"),
        ("Halberd",    300, "11 Halberd.002"),
    };

    static readonly Color C_BG        = new Color(0.06f, 0.06f, 0.10f, 0.97f);
    static readonly Color C_HEADER    = new Color(0.08f, 0.08f, 0.14f, 1.00f);
    static readonly Color C_COL_HDR   = new Color(0.04f, 0.04f, 0.09f, 1.00f);
    static readonly Color C_ROW       = new Color(0.10f, 0.20f, 0.35f, 0.90f);
    static readonly Color C_GOLD      = new Color(1.00f, 0.84f, 0.00f, 1.00f);
    static readonly Color C_WHITE     = Color.white;
    static readonly Color C_BTN_BUY   = new Color(0.10f, 0.65f, 0.20f, 1.00f);
    static readonly Color C_BTN_CLOSE = new Color(0.75f, 0.10f, 0.10f, 1.00f);
    static readonly Color C_BADGE_BG  = new Color(0.18f, 0.18f, 0.28f, 1.00f);

    const float ROW_H    = 62f;
    const float HDR_H    = 72f;
    const float COLHDR_H = 36f;
    const float STATUS_H = 32f;

    [MenuItem("Tools/Setup Weapon Shop UI")]
    public static void Setup()
    {
        GameObject shopMenuCanvas = GameObject.Find("ShopMenuCanvas");
        if (shopMenuCanvas == null) { Debug.LogError("ShopMenuCanvas not found."); return; }

        // Hide old children that conflict
        foreach (string n in new[] { "Background", "Title", "CloseButton", "BuyHealthButton" })
        {
            Transform t = shopMenuCanvas.transform.Find(n);
            if (t != null) t.gameObject.SetActive(false);
        }

        // Remove old weapon shop panel
        Transform old = shopMenuCanvas.transform.Find("WeaponShopPanel");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // ── Root panel (fills canvas 800x600) ────────────────────────────────
        GameObject panel = MakeUI("WeaponShopPanel", shopMenuCanvas.transform);
        Stretch(panel);
        Img(panel, C_BG);

        // ── Header (top 72px) ─────────────────────────────────────────────────
        GameObject hdr = MakeUI("Header", panel.transform);
        AnchorTopStretch(hdr, HDR_H);
        Img(hdr, C_HEADER);

        // Title text
        GameObject titleGO = MakeUI("Title", hdr.transform);
        Stretch(titleGO, new Vector2(10, 0), new Vector2(-220, 0));
        TMP(titleGO, "WEAPON SHOP", 34, FontStyles.Bold, C_GOLD, TextAlignmentOptions.MidlineLeft);

        // Coin display
        GameObject coinGO = MakeUI("CoinDisplay", hdr.transform);
        AnchorRightStretch(coinGO, 210, 10);
        var coinTMP = TMP(coinGO, "Coins: 0", 26, FontStyles.Bold, C_GOLD, TextAlignmentOptions.MidlineRight);

        // Close button
        GameObject closeGO = MakeUI("CloseBtn", hdr.transform);
        AnchorTopRight(closeGO, 100, HDR_H - 10, 5, 5);
        Img(closeGO, C_BTN_CLOSE);
        var closeBtn = closeGO.AddComponent<Button>();
        var closeLbl = MakeUI("Lbl", closeGO.transform); Stretch(closeLbl);
        TMP(closeLbl, "X  Close", 22, FontStyles.Bold, C_WHITE, TextAlignmentOptions.Center);

        // ── Status bar (below header, 32px) ───────────────────────────────────
        float statusTop = HDR_H + STATUS_H;
        GameObject statusBar = MakeUI("StatusBar", panel.transform);
        AnchorBelowTop(statusBar, HDR_H, statusTop);
        Img(statusBar, new Color(0.04f, 0.04f, 0.07f, 0.85f));
        GameObject statusLblGO = MakeUI("Lbl", statusBar.transform); Stretch(statusLblGO, new Vector2(8,0), new Vector2(-8,0));
        var statusTMP = TMP(statusLblGO, "", 21, FontStyles.Normal, new Color(1f, 0.8f, 0.2f), TextAlignmentOptions.Center);

        // ── Column header (below status, 36px) ────────────────────────────────
        float colTop = statusTop + COLHDR_H;
        GameObject colHdr = MakeUI("ColHeader", panel.transform);
        AnchorBelowTop(colHdr, statusTop, colTop);
        Img(colHdr, C_COL_HDR);
        BuildColHeaders(colHdr.transform);

        // ── Scroll area (rest of panel) ───────────────────────────────────────
        GameObject scroll = MakeUI("ScrollArea", panel.transform);
        AnchorFillBelow(scroll, colTop);
        Img(scroll, new Color(0, 0, 0, 0));

        // Viewport with mask
        GameObject vp = MakeUI("Viewport", scroll.transform);
        Stretch(vp);
        Img(vp, new Color(0, 0, 0, 0.01f)); // tiny alpha needed for mask
        vp.AddComponent<Mask>().showMaskGraphic = false;

        // Content (tall enough for all rows)
        float totalH = WEAPONS.Length * (ROW_H + 4) + 8;
        GameObject content = MakeUI("Content", vp.transform);
        var crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 1);
        crt.anchorMax = new Vector2(1, 1);
        crt.pivot     = new Vector2(0.5f, 1f);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(0, totalH);

        // ScrollRect
        var sr = scroll.AddComponent<ScrollRect>();
        sr.viewport  = vp.GetComponent<RectTransform>();
        sr.content   = crt;
        sr.horizontal = false;
        sr.vertical   = true;
        sr.scrollSensitivity = 30f;

        // ── Weapon rows ───────────────────────────────────────────────────────
        var weaponParent = GameObject.Find("Simple Melee Weapons");
        var entries = new List<WeaponShopManager.WeaponEntry>();

        for (int i = 0; i < WEAPONS.Length; i++)
        {
            var (wName, wPrice, wObjName) = WEAPONS[i];
            float yTop = 4 + i * (ROW_H + 4);

            GameObject row = MakeUI($"Row_{i:D2}_{wName.Replace(" ", "_")}", content.transform);
            var rrt = row.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0, 1);
            rrt.anchorMax = new Vector2(1, 1);
            rrt.pivot     = new Vector2(0.5f, 1f);
            rrt.anchoredPosition = new Vector2(0, -yTop);
            rrt.sizeDelta = new Vector2(-16, ROW_H);
            Img(row, C_ROW);

            // Badge
            GameObject badge = MakeUI("Badge", row.transform);
            AbsRect(badge, 6, 6, 56, ROW_H - 6);
            Img(badge, C_BADGE_BG);
            GameObject badgeLbl = MakeUI("Lbl", badge.transform); Stretch(badgeLbl);
            TMP(badgeLbl, $"{i+1:D2}", 22, FontStyles.Bold, C_GOLD, TextAlignmentOptions.Center);

            // Name
            GameObject nameGO = MakeUI("NameText", row.transform);
            AbsRect(nameGO, 66, 0, 66 + 210, ROW_H);
            GameObject nameLbl = MakeUI("Lbl", nameGO.transform); Stretch(nameLbl, new Vector2(4,0), Vector2.zero);
            TMP(nameLbl, wName, 24, FontStyles.Normal, C_WHITE, TextAlignmentOptions.MidlineLeft);

            // Price
            GameObject priceGO = MakeUI("PriceText", row.transform);
            AbsRect(priceGO, 282, 0, 282 + 120, ROW_H);
            GameObject priceLbl = MakeUI("Lbl", priceGO.transform); Stretch(priceLbl);
            TMP(priceLbl, $"{wPrice} coins", 22, FontStyles.Normal, C_GOLD, TextAlignmentOptions.Center);

            // Status
            GameObject statusGO = MakeUI("StatusText", row.transform);
            AbsRect(statusGO, 408, 0, 408 + 200, ROW_H);
            GameObject statusLbl = MakeUI("Lbl", statusGO.transform); Stretch(statusLbl);
            var stTMP = TMP(statusLbl, i == 0 ? "" : "LOCKED", 20, FontStyles.Normal,
                            new Color(0.55f, 0.55f, 0.55f), TextAlignmentOptions.Center);

            // Buy button
            GameObject buyGO = MakeUI("BuyButton", row.transform);
            AbsRect(buyGO, 614, 8, 614 + 120, ROW_H - 8);
            Img(buyGO, C_BTN_BUY);
            buyGO.AddComponent<Button>();
            GameObject buyLbl = MakeUI("Lbl", buyGO.transform); Stretch(buyLbl);
            TMP(buyLbl, "BUY", 22, FontStyles.Bold, C_WHITE, TextAlignmentOptions.Center);

            // Spawn button (orange, hidden until weapon is owned)
            GameObject spawnGO = MakeUI("SpawnButton", row.transform);
            AbsRect(spawnGO, 614, 8, 614 + 120, ROW_H - 8);
            Img(spawnGO, new Color(0.80f, 0.45f, 0.00f, 1f));
            spawnGO.AddComponent<Button>();
            GameObject spawnLbl = MakeUI("Lbl", spawnGO.transform); Stretch(spawnLbl);
            TMP(spawnLbl, "SPAWN", 20, FontStyles.Bold, C_WHITE, TextAlignmentOptions.Center);
            spawnGO.SetActive(false);

            // Find weapon scene object
            GameObject weaponObj = null;
            if (weaponParent != null)
                weaponObj = FindDeep(weaponParent.transform, wObjName)?.gameObject;

            entries.Add(new WeaponShopManager.WeaponEntry
            {
                weaponName   = wName,
                price        = wPrice,
                weaponObject = weaponObj,
                rowUI        = row
            });
        }

        // ── WeaponShopManager ─────────────────────────────────────────────────
        WeaponShopManager wsm = shopMenuCanvas.GetComponent<WeaponShopManager>();
        if (wsm == null) wsm = shopMenuCanvas.AddComponent<WeaponShopManager>();
        wsm.coinDisplayText   = coinTMP;
        wsm.statusMessageText = statusTMP;
        wsm.weaponListParent  = content.transform;
        wsm.SetupWeapons(entries);

        // Wire close button
        VRShopZone zone = Object.FindObjectOfType<VRShopZone>();
        if (zone != null) closeBtn.onClick.AddListener(zone.CloseShop);

        // Auto-assign right controller
        GameObject rc = GameObject.Find("Right Controller");
        if (rc != null) wsm.rightControllerTransform = rc.transform;

        EditorUtility.SetDirty(shopMenuCanvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[SetupWeaponShopUI] Done!");
    }

    // ── Layout helpers ────────────────────────────────────────────────────────

    static GameObject MakeUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    // Fill parent completely
    static void Stretch(GameObject go, Vector2 oMin = default, Vector2 oMax = default)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = oMin;        rt.offsetMax = oMax;
    }

    // Top-anchored strip of height h
    static void AnchorTopStretch(GameObject go, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, h);
    }

    // Strip between two y-offsets from top (offsetFromTop_start, offsetFromTop_end)
    static void AnchorBelowTop(GameObject go, float yStart, float yEnd)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -yStart);
        rt.sizeDelta = new Vector2(0, -(yEnd - yStart));
    }

    // Fill from yStart to bottom
    static void AnchorFillBelow(GameObject go, float yStart)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, -yStart);
    }

    // Right-aligned, vertically stretched
    static void AnchorRightStretch(GameObject go, float width, float rightPad)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0); rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(1, 0.5f);
        rt.anchoredPosition = new Vector2(-rightPad, 0);
        rt.sizeDelta = new Vector2(width, 0);
    }

    // Absolute rect from left/bottom offsets (left, bottom, right, top) in local space
    static void AbsRect(GameObject go, float left, float bottom, float right, float top)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.zero;
        rt.pivot     = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(left, bottom);
        rt.sizeDelta = new Vector2(right - left, top - bottom);
    }

    // Top-right corner button
    static void AnchorTopRight(GameObject go, float w, float h, float rPad, float tPad)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-rPad, -tPad);
        rt.sizeDelta = new Vector2(w, h);
    }

    static Image Img(GameObject go, Color c)
    {
        var img = go.AddComponent<Image>(); img.color = c; return img;
    }

    static TextMeshProUGUI TMP(GameObject go, string text, float size,
        FontStyles style, Color color, TextAlignmentOptions align)
    {
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.fontStyle = style;
        t.color = color; t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    static void BuildColHeaders(Transform parent)
    {
        string[] labels = { "#",  "Weapon", "Price",  "Status", "Action" };
        float[]  lefts  = { 6,    66,       282,      408,      614 };
        float[]  rights = { 56,   276,      402,      608,      734 };
        for (int i = 0; i < labels.Length; i++)
        {
            GameObject col = MakeUI($"CH_{labels[i]}", parent);
            AbsRect(col, lefts[i], 0, rights[i], COLHDR_H);
            TMP(col, labels[i], 18, FontStyles.Bold,
                new Color(0.7f, 0.7f, 0.9f), TextAlignmentOptions.Center);
        }
    }

    static Transform FindDeep(Transform p, string name)
    {
        foreach (Transform c in p)
        {
            if (c.name == name) return c;
            var f = FindDeep(c, name);
            if (f != null) return f;
        }
        return null;
    }
}
