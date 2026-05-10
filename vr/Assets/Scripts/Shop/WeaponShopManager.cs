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

        [Tooltip("Local position of the grab handle relative to the weapon root (where the hand holds it)")]
        public Vector3 handleOffset = Vector3.zero;
        [Tooltip("Local rotation offset so weapon faces forward when held")]
        public Vector3 handleRotation = Vector3.zero;
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
    [Tooltip("Optional spawn point. If set, weapons will spawn here instead of the controller.")]
    public Transform spawnPoint;

    // Per-weapon stats: (damage, maxDurability)
    static readonly (int damage, int durability)[] WEAPON_STATS =
    {
        (8,  4),   // 00 Dagger      - 20 coins
        (12, 6),   // 01 Sword       - 40 coins
        (16, 8),   // 02 Long Sword  - 60 coins
        (20, 10),  // 03 Axe         - 80 coins
        (26, 13),  // 04 Battleaxe   - 100 coins
        (32, 16),  // 05 Mace        - 120 coins
        (38, 19),  // 06 Heavy Mace  - 150 coins
        (45, 22),  // 07 Hammer      - 180 coins
        (52, 26),  // 08 Warhammer   - 220 coins
        (60, 30),  // 09 Spear       - 260 coins
        (70, 35),  // 10 Halberd     - 300 coins
        (80, 40),  // 11 长剑        - 350 coins
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

    void UpdateRow(int i)
    {
        if (i < 0 || i >= weapons.Count) return;
        WeaponEntry w = weapons[i];
        if (w.rowUI == null) return;

        bool owned    = IsUnlocked(i);
        bool prevOwned= i == 0 || IsUnlocked(i - 1);
        bool canAfford= ShopManager.coins >= w.price;

        var statusTxt = w.rowUI.transform.Find("StatusText/Text")?.GetComponent<TextMeshProUGUI>();
        var buyBtn    = w.rowUI.transform.Find("BuyButton")?.GetComponent<Button>();
        var spawnBtn  = w.rowUI.transform.Find("SpawnButton")?.GetComponent<Button>();
        var rowImg    = w.rowUI.GetComponent<Image>();
        var priceTxt  = w.rowUI.transform.Find("PriceText/Text")?.GetComponent<TextMeshProUGUI>();
        if (priceTxt != null) priceTxt.text = $"{w.price} coins";

        if (owned)
        {
            if (rowImg    != null) rowImg.color = C_OWNED;
            if (statusTxt != null) { statusTxt.text = "OWNED"; statusTxt.color = new Color(0.3f, 1f, 0.3f); }
            if (buyBtn    != null) buyBtn.gameObject.SetActive(false);
            if (spawnBtn  != null)
            {
                spawnBtn.gameObject.SetActive(true);
                spawnBtn.onClick.RemoveAllListeners();
                int idx = i;
                spawnBtn.onClick.AddListener(() => SpawnWeapon(idx));
                var spawnImg = spawnBtn.GetComponent<Image>();
                if (spawnImg != null) spawnImg.color = C_BTN_SPAWN;
            }
        }
        else if (!prevOwned)
        {
            if (rowImg    != null) rowImg.color = C_LOCKED;
            if (statusTxt != null) { statusTxt.text = "LOCKED"; statusTxt.color = new Color(0.5f, 0.5f, 0.5f); }
            if (buyBtn    != null) buyBtn.gameObject.SetActive(false);
            if (spawnBtn  != null) spawnBtn.gameObject.SetActive(false);
        }
        else if (!canAfford)
        {
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
            if (rowImg    != null) rowImg.color = C_AFFORD;
            if (statusTxt != null) statusTxt.text = "";
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

    public void BuyWeapon(int i)
    {
        if (i < 0 || i >= weapons.Count) return;
        WeaponEntry w = weapons[i];
        if (IsUnlocked(i)) return;

        bool prevOwned = i == 0 || IsUnlocked(i - 1);
        if (!prevOwned) { ShowStatus("Unlock previous weapon first!"); return; }
        if (ShopManager.coins < w.price) { ShowStatus($"Need {w.price - ShopManager.coins} more coins!"); return; }

        ShopManager.coins -= w.price;
        PlayerPrefs.SetInt("SavedCoins", ShopManager.coins);
        PlayerPrefs.SetInt(UNLOCK_KEY + i, 1);
        PlayerPrefs.Save();

        ShowStatus($"{w.weaponName} unlocked! Press SPAWN to equip.");
        RefreshUI();
    }

    public void SpawnWeapon(int i)
    {
        if (i < 0 || i >= weapons.Count) return;
        WeaponEntry w = weapons[i];
        if (!IsUnlocked(i)) return;
        if (w.weaponObject == null) { ShowStatus($"Weapon object not found for {w.weaponName}!"); return; }

        // ── Spawn position ────────────────────────────────────────────────────
        Vector3 spawnPos;
        Quaternion spawnRot;

        if (spawnPoint != null)
        {
            spawnPos = spawnPoint.position;
            spawnRot = spawnPoint.rotation;
        }
        else if (rightControllerTransform != null)
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

        // ── Re-summon existing ────────────────────────────────────────────────
        GameObject existing = GameObject.Find($"{w.weaponName}_Spawned");
        if (existing != null)
        {
            // If it's currently being held, don't yank it out of the hand —
            // just notify the player.
            XRGrabInteractable existingGrab = existing.GetComponent<XRGrabInteractable>();
            if (existingGrab != null && existingGrab.isSelected)
            {
                ShowStatus($"{w.weaponName} is already in your hand!");
                return;
            }
            existing.transform.SetParent(null);
            existing.transform.position = spawnPos;
            existing.transform.rotation = spawnRot;
            existing.SetActive(true);
            Rigidbody existingRb = existing.GetComponent<Rigidbody>();
            if (existingRb != null)
            {
                existingRb.velocity = Vector3.zero;
                existingRb.angularVelocity = Vector3.zero;
            }
            ShowStatus($"{w.weaponName} summoned!");
            return;
        }

        // ── Instantiate (clone the scene template AS-IS) ──────────────────────
        // The scene weapons are already configured with the correct Rigidbody,
        // Collider, XRGrabInteractable + attachTransform. We trust those values
        // and only adjust pose / per-instance data (stats, name, tag).
        GameObject spawned = Object.Instantiate(w.weaponObject, spawnPos, spawnRot);
        spawned.name = $"{w.weaponName}_Spawned";
        spawned.SetActive(true);
        spawned.transform.SetParent(null);

        // ── Rigidbody: ensure non-kinematic so player can pick it up ──────────
        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb == null) rb = spawned.AddComponent<Rigidbody>();
        rb.useGravity  = true;
        rb.isKinematic = false;
        if (rb.mass <= 0f) rb.mass = 0.5f;

        // ── Ensure there is at least one non-trigger collider ─────────────────
        // (the scene templates already have one; this is just a safety net so
        //  spawned weapons are always grabbable / movable).
        bool hasSolidCollider = false;
        foreach (var c in spawned.GetComponentsInChildren<Collider>())
        {
            if (!c.isTrigger) { hasSolidCollider = true; break; }
        }
        if (!hasSolidCollider)
        {
            // Build a box collider sized to the renderer bounds (in local space).
            Renderer rend = spawned.GetComponentInChildren<Renderer>();
            BoxCollider box = spawned.AddComponent<BoxCollider>();
            if (rend != null)
            {
                // Convert world-space renderer bounds into the spawned object's local space
                Vector3 lossy = spawned.transform.lossyScale;
                Vector3 worldSize = rend.bounds.size;
                box.size = new Vector3(
                    lossy.x != 0f ? worldSize.x / lossy.x : 1f,
                    lossy.y != 0f ? worldSize.y / lossy.y : 1f,
                    lossy.z != 0f ? worldSize.z / lossy.z : 1f);
                box.center = spawned.transform.InverseTransformPoint(rend.bounds.center);
            }
        }

        // ── XRGrabInteractable: ensure present + has an attachTransform ───────
        XRGrabInteractable grab = spawned.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = spawned.AddComponent<XRGrabInteractable>();

        // If the template didn't carry an attachTransform, try to find one
        // by name among the children (designer-placed grip).
        if (grab.attachTransform == null)
        {
            Transform spawnedAttach = spawned.transform.Find("GameObject");
            if (spawnedAttach == null) spawnedAttach = spawned.transform.Find("w");
            if (spawnedAttach == null) spawnedAttach = spawned.transform.Find("default");
            if (spawnedAttach != null) grab.attachTransform = spawnedAttach;
        }

        // Make weapons feel natural when held: rotation follows the wrist,
        // weapon snaps to attach point, no leftover frozen-rotation constraints.
        grab.trackPosition       = true;
        grab.trackRotation       = true;
        grab.movementType        = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
        grab.throwOnDetach       = true;
        grab.useDynamicAttach    = false;
        grab.matchAttachPosition = true;
        grab.matchAttachRotation = true;
        rb.constraints           = RigidbodyConstraints.None;

        // ── WeaponStats ───────────────────────────────────────────────────────
        WeaponStats stats = spawned.GetComponent<WeaponStats>();
        if (stats == null) stats = spawned.AddComponent<WeaponStats>();
        var (dmg, dur) = i < WEAPON_STATS.Length
            ? WEAPON_STATS[i]
            : (10 + i * 5, 5 + i * 2);
        stats.SetStats(w.weaponName, i, dmg, dur);

        spawned.tag = "Weapon";

        ShowStatus($"{w.weaponName} spawned! DMG:{dmg}  DUR:{dur} hits");
        Debug.Log($"[WeaponShop] Spawned {w.weaponName} | DMG:{dmg} DUR:{dur} at {spawnPos}");
    }

    public bool IsUnlocked(int i) => PlayerPrefs.GetInt(UNLOCK_KEY + i, 0) == 1;

    void ShowStatus(string msg)
    {
        if (statusMessageText != null) statusMessageText.text = msg;
    }

    public void SetupWeapons(List<WeaponEntry> entries) => weapons = entries;
}
