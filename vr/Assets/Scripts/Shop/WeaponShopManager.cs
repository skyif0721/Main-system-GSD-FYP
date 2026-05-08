using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Manages the weapon shop UI.
/// - Weapons unlock one-by-one in order.
/// - After unlocking, a SPAWN button appears so the player can summon the weapon
///   to their right hand at any time.
/// - Spawned weapon gets XRGrabInteractable so it can be grabbed and used.
/// </summary>
public class WeaponShopManager : MonoBehaviour
{
    [System.Serializable]
    public class WeaponEntry
    {
        public string weaponName;
        public int price;
        public GameObject weaponObject;   // Original weapon in scene (template)
        public GameObject rowUI;          // The UI row for this weapon
    }

    [Header("Weapon Data")]
    public List<WeaponEntry> weapons = new List<WeaponEntry>();

    [Header("UI References")]
    public TextMeshProUGUI coinDisplayText;
    public TextMeshProUGUI statusMessageText;
    public Transform weaponListParent;

    [Header("Spawn Settings")]
    [Tooltip("Right controller transform — weapon spawns here")]
    public Transform rightControllerTransform;
    [Tooltip("Offset from right controller where weapon appears (forward = in front of hand)")]
    public Vector3 spawnOffset = new Vector3(0f, 0f, 0.3f);

    // Per-weapon stats: (damage, maxDurability)
    // Cheaper weapons = lower damage + lower durability (breaks faster)
    static readonly (int damage, int durability)[] WEAPON_STATS =
    {
        (8,  4),   // 00 Dagger      - 20 coins  - weak, breaks in 4 hits
        (12, 6),   // 01 Sword       - 40 coins
        (16, 8),   // 02 Long Sword  - 60 coins
        (20, 10),  // 03 Axe         - 80 coins
        (26, 13),  // 04 Battleaxe   - 100 coins
        (32, 16),  // 05 Mace        - 120 coins
        (38, 19),  // 06 Heavy Mace  - 150 coins
        (45, 22),  // 07 Hammer      - 180 coins
        (52, 26),  // 08 Warhammer   - 220 coins
        (60, 30),  // 09 Spear       - 260 coins
        (70, 35),  // 10 Halberd     - 300 coins - strongest, lasts 35 hits
    };

    private const string UNLOCK_KEY = "WeaponUnlocked_";

    // Colors
    static readonly Color C_OWNED    = new Color(0.10f, 0.25f, 0.10f, 0.90f);
    static readonly Color C_LOCKED   = new Color(0.15f, 0.15f, 0.15f, 0.90f);
    static readonly Color C_AFFORD   = new Color(0.10f, 0.20f, 0.35f, 0.90f);
    static readonly Color C_POOR     = new Color(0.25f, 0.10f, 0.05f, 0.90f);
    static readonly Color C_BTN_BUY  = new Color(0.10f, 0.65f, 0.20f, 1.00f);
    static readonly Color C_BTN_SPAWN= new Color(0.80f, 0.45f, 0.00f, 1.00f);
    static readonly Color C_BTN_OFF  = new Color(0.25f, 0.25f, 0.25f, 1.00f);

    void Start()
    {
        // Auto-find right controller if not assigned
        if (rightControllerTransform == null)
        {
            GameObject rc = GameObject.Find("Right Controller");
            if (rc != null) rightControllerTransform = rc.transform;
        }
    }

    void OnEnable() => RefreshUI();

    public void RefreshUI()
    {
        if (coinDisplayText != null)
            coinDisplayText.text = $"Coins: {ShopManager.coins}";
        if (statusMessageText != null)
            statusMessageText.text = "";
        for (int i = 0; i < weapons.Count; i++)
            UpdateRow(i);
    }

    // ── Row state machine ─────────────────────────────────────────────────────

    void UpdateRow(int i)
    {
        if (i < 0 || i >= weapons.Count) return;
        WeaponEntry w = weapons[i];
        if (w.rowUI == null) return;

        bool owned    = IsUnlocked(i);
        bool prevOwned= i == 0 || IsUnlocked(i - 1);
        bool canAfford= ShopManager.coins >= w.price;

        // Sub-elements
        var statusTxt = w.rowUI.transform.Find("StatusText/Text")?.GetComponent<TextMeshProUGUI>();
        var buyBtn    = w.rowUI.transform.Find("BuyButton")?.GetComponent<Button>();
        var spawnBtn  = w.rowUI.transform.Find("SpawnButton")?.GetComponent<Button>();
        var rowImg    = w.rowUI.GetComponent<Image>();

        // Update price text to always show current price
        var priceTxt = w.rowUI.transform.Find("PriceText/Text")?.GetComponent<TextMeshProUGUI>();
        if (priceTxt != null) priceTxt.text = $"{w.price} coins";

        if (owned)
        {
            // ── OWNED: hide BUY, show SPAWN ───────────────────────────────────
            if (rowImg    != null) rowImg.color = C_OWNED;
            if (statusTxt != null) { statusTxt.text = "OWNED"; statusTxt.color = new Color(0.3f, 1f, 0.3f); }
            if (buyBtn    != null) buyBtn.gameObject.SetActive(false);
            if (spawnBtn  != null)
            {
                spawnBtn.gameObject.SetActive(true);
                spawnBtn.onClick.RemoveAllListeners();
                int idx = i;
                spawnBtn.onClick.AddListener(() => SpawnWeapon(idx));
                // Orange color
                var spawnImg = spawnBtn.GetComponent<Image>();
                if (spawnImg != null) spawnImg.color = C_BTN_SPAWN;
            }
        }
        else if (!prevOwned)
        {
            // ── LOCKED ────────────────────────────────────────────────────────
            if (rowImg    != null) rowImg.color = C_LOCKED;
            if (statusTxt != null) { statusTxt.text = "LOCKED"; statusTxt.color = new Color(0.5f, 0.5f, 0.5f); }
            if (buyBtn    != null) { buyBtn.gameObject.SetActive(false); }
            if (spawnBtn  != null) spawnBtn.gameObject.SetActive(false);
        }
        else if (!canAfford)
        {
            // ── CAN'T AFFORD ──────────────────────────────────────────────────
            if (rowImg    != null) rowImg.color = C_POOR;
            if (statusTxt != null) { statusTxt.text = "Fight for more!"; statusTxt.color = new Color(1f, 0.4f, 0.1f); }
            if (buyBtn    != null)
            {
                buyBtn.gameObject.SetActive(true);
                buyBtn.interactable = false;
                var img = buyBtn.GetComponent<Image>();
                if (img != null) img.color = C_BTN_OFF;
            }
            if (spawnBtn != null) spawnBtn.gameObject.SetActive(false);
        }
        else
        {
            // ── CAN BUY ───────────────────────────────────────────────────────
            if (rowImg    != null) rowImg.color = C_AFFORD;
            if (statusTxt != null) { statusTxt.text = ""; }
            if (buyBtn    != null)
            {
                buyBtn.gameObject.SetActive(true);
                buyBtn.interactable = true;
                var img = buyBtn.GetComponent<Image>();
                if (img != null) img.color = C_BTN_BUY;
                buyBtn.onClick.RemoveAllListeners();
                int idx = i;
                buyBtn.onClick.AddListener(() => BuyWeapon(idx));
            }
            if (spawnBtn != null) spawnBtn.gameObject.SetActive(false);
        }
    }

    // ── Buy ───────────────────────────────────────────────────────────────────

    public void BuyWeapon(int i)
    {
        if (i < 0 || i >= weapons.Count) return;
        WeaponEntry w = weapons[i];
        if (IsUnlocked(i)) return;

        bool prevOwned = i == 0 || IsUnlocked(i - 1);
        if (!prevOwned)
        {
            ShowStatus("Unlock previous weapon first!");
            return;
        }
        if (ShopManager.coins < w.price)
        {
            ShowStatus($"Need {w.price - ShopManager.coins} more coins!");
            return;
        }

        ShopManager.coins -= w.price;
        PlayerPrefs.SetInt("SavedCoins", ShopManager.coins);
        PlayerPrefs.SetInt(UNLOCK_KEY + i, 1);
        PlayerPrefs.Save();

        ShowStatus($"{w.weaponName} unlocked! Press SPAWN to equip.");
        Debug.Log($"[WeaponShop] Bought {w.weaponName}. Coins left: {ShopManager.coins}");

        RefreshUI();
    }

    // ── Spawn ─────────────────────────────────────────────────────────────────

    public void SpawnWeapon(int i)
    {
        if (i < 0 || i >= weapons.Count) return;
        WeaponEntry w = weapons[i];
        if (!IsUnlocked(i)) return;
        if (w.weaponObject == null)
        {
            ShowStatus($"Weapon object not found for {w.weaponName}!");
            return;
        }

        // ── Spawn position: right controller or camera fallback ───────────────
        Vector3 spawnPos;
        Quaternion spawnRot;
        if (rightControllerTransform != null)
        {
            spawnPos = rightControllerTransform.position
                     + rightControllerTransform.TransformDirection(spawnOffset);
            spawnRot = rightControllerTransform.rotation;
        }
        else
        {
            Camera cam = Camera.main;
            spawnPos = cam != null
                ? cam.transform.position + cam.transform.forward * 0.5f + Vector3.down * 0.2f
                : Vector3.zero;
            spawnRot = Quaternion.identity;
        }

        // ── Instantiate ───────────────────────────────────────────────────────
        GameObject spawned = Object.Instantiate(w.weaponObject, spawnPos, spawnRot);
        spawned.name = $"{w.weaponName}_Spawned";
        spawned.SetActive(true);

        // ── Fix scale: weapons are stored at scale 100 — normalise to 1 ───────
        // The original weapons have localScale 100,100,100 (model units → metres)
        // We keep that scale so the mesh looks right, but ensure it's not parented
        spawned.transform.SetParent(null);
        // Scale is inherited from original — keep as-is (already correct world size)

        // ── Rigidbody ─────────────────────────────────────────────────────────
        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb == null) rb = spawned.AddComponent<Rigidbody>();
        rb.useGravity    = true;
        rb.isKinematic   = false;
        rb.mass          = 0.5f;
        rb.drag          = 1f;
        rb.angularDrag   = 2f;

        // ── Collider (trigger for hit detection + solid for grab) ─────────────
        // Remove any existing colliders first to avoid duplicates
        foreach (var c in spawned.GetComponents<Collider>())
            Object.DestroyImmediate(c);

        // Solid capsule for physics/grab
        CapsuleCollider physCol = spawned.AddComponent<CapsuleCollider>();
        physCol.isTrigger  = false;
        physCol.direction  = 2; // Z-axis (along blade)
        physCol.radius     = 0.015f;
        physCol.height     = 0.6f;
        physCol.center     = new Vector3(0, 0, 0);

        // Trigger for hit detection (slightly larger)
        CapsuleCollider hitCol = spawned.AddComponent<CapsuleCollider>();
        hitCol.isTrigger  = true;
        hitCol.direction  = 2;
        hitCol.radius     = 0.025f;
        hitCol.height     = 0.65f;
        hitCol.center     = new Vector3(0, 0, 0);

        // ── XRGrabInteractable ────────────────────────────────────────────────
        XRGrabInteractable grab = spawned.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = spawned.AddComponent<XRGrabInteractable>();
        grab.throwOnDetach          = true;
        grab.velocityScale          = 1f;
        grab.angularVelocityScale   = 1f;
        grab.throwSmoothingDuration = 0.1f;
        grab.throwVelocityScale     = 1.5f;

        // ── WeaponStats (damage + durability) ─────────────────────────────────
        WeaponStats stats = spawned.GetComponent<WeaponStats>();
        if (stats == null) stats = spawned.AddComponent<WeaponStats>();
        var (dmg, dur) = i < WEAPON_STATS.Length
            ? WEAPON_STATS[i]
            : (10 + i * 5, 5 + i * 2);
        stats.SetStats(w.weaponName, i, dmg, dur);

        // ── Tag as Weapon ─────────────────────────────────────────────────────
        spawned.tag = "Weapon";

        ShowStatus($"{w.weaponName} spawned! DMG:{dmg}  DUR:{dur} hits");
        Debug.Log($"[WeaponShop] Spawned {w.weaponName} | DMG:{dmg} DUR:{dur} at {spawnPos}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    public bool IsUnlocked(int i) => PlayerPrefs.GetInt(UNLOCK_KEY + i, 0) == 1;

    void ShowStatus(string msg)
    {
        if (statusMessageText != null) statusMessageText.text = msg;
    }

    public void SetupWeapons(List<WeaponEntry> entries) => weapons = entries;
}
