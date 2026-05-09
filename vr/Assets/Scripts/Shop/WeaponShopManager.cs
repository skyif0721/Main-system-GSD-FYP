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
            existing.transform.position = spawnPos;
            existing.transform.rotation = spawnRot;
            Rigidbody existingRb = existing.GetComponent<Rigidbody>();
            if (existingRb != null) { existingRb.velocity = Vector3.zero; existingRb.angularVelocity = Vector3.zero; }
            ShowStatus($"{w.weaponName} summoned!");
            return;
        }

        // ── Instantiate ───────────────────────────────────────────────────────
        GameObject spawned = Object.Instantiate(w.weaponObject, spawnPos, spawnRot);
        spawned.name = $"{w.weaponName}_Spawned";
        spawned.SetActive(true);
        spawned.transform.SetParent(null);

        // ── Rigidbody ─────────────────────────────────────────────────────────
        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb == null) rb = spawned.AddComponent<Rigidbody>();
        rb.useGravity  = true;
        rb.isKinematic = false;
        rb.mass        = 0.5f;
        rb.drag        = 1f;
        rb.angularDrag = 2f;

        // ── Colliders: remove all old ones, add clean set ─────────────────────
        foreach (var c in spawned.GetComponentsInChildren<Collider>())
            Object.DestroyImmediate(c);

        // Get the actual mesh bounds to size colliders correctly
        MeshFilter mf = spawned.GetComponentInChildren<MeshFilter>();
        Bounds meshBounds = mf != null ? mf.sharedMesh.bounds : new Bounds(Vector3.zero, Vector3.one * 0.3f);

        // Determine blade axis from mesh extents (longest axis = blade direction)
        Vector3 size = meshBounds.size;
        int bladeAxis = 1; // Y by default
        if (size.x > size.y && size.x > size.z) bladeAxis = 0;
        else if (size.z > size.y) bladeAxis = 2;

        float bladeLength = bladeAxis == 0 ? size.x : (bladeAxis == 1 ? size.y : size.z);
        float bladeRadius = Mathf.Min(size.x, size.y, size.z) * 0.35f;
        bladeRadius = Mathf.Clamp(bladeRadius, 0.01f, 0.05f);

        // Scale correction: if weapon was at scale 100, mesh bounds are in local units
        Transform spawnedT = spawned.transform;
        float scaleFactor = spawnedT.lossyScale.x;

        // Solid capsule for physics/grab — tight around the blade
        CapsuleCollider physCol = spawned.AddComponent<CapsuleCollider>();
        physCol.isTrigger = false;
        physCol.direction = bladeAxis;
        physCol.radius    = bladeRadius;
        physCol.height    = bladeLength;
        physCol.center    = meshBounds.center;

        // Trigger capsule for hit detection — slightly larger, blade only (upper 70%)
        CapsuleCollider hitCol = spawned.AddComponent<CapsuleCollider>();
        hitCol.isTrigger  = true;
        hitCol.direction  = bladeAxis;
        hitCol.radius     = bladeRadius * 1.2f;
        hitCol.height     = bladeLength * 0.7f;
        // Offset toward blade tip (away from handle)
        Vector3 hitCenter = meshBounds.center;
        float tipOffset = bladeLength * 0.15f;
        if (bladeAxis == 0) hitCenter.x += tipOffset;
        else if (bladeAxis == 1) hitCenter.y += tipOffset;
        else hitCenter.z += tipOffset;
        hitCol.center = hitCenter;

        // ── Attach Transform (handle / grip point) ────────────────────────────
        // Create a child GameObject at the handle position so the weapon
        // snaps correctly to the player's hand when grabbed
        GameObject attachGO = new GameObject("AttachPoint");
        attachGO.transform.SetParent(spawned.transform);
        // Handle is at the bottom of the blade (opposite end from tip)
        Vector3 handleLocal = meshBounds.center;
        float handleOffset = bladeLength * 0.35f;
        if (bladeAxis == 0) handleLocal.x -= handleOffset;
        else if (bladeAxis == 1) handleLocal.y -= handleOffset;
        else handleLocal.z -= handleOffset;
        attachGO.transform.localPosition = handleLocal + w.handleOffset;
        attachGO.transform.localEulerAngles = w.handleRotation;

        // ── Find attach point from spawned weapon ─────────────────────────────
        // Use the manually placed child attach point (named "GameObject", "w", or "default")
        // that the designer set up in the scene template weapon.
        Transform spawnedAttach = spawned.transform.Find("GameObject");
        if (spawnedAttach == null) spawnedAttach = spawned.transform.Find("w");
        if (spawnedAttach == null) spawnedAttach = spawned.transform.Find("default");
        // Fall back to the auto-generated one if no manual point exists
        if (spawnedAttach == null) spawnedAttach = attachGO.transform;

        // ── XRGrabInteractable ────────────────────────────────────────────────
        XRGrabInteractable grab = spawned.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = spawned.AddComponent<XRGrabInteractable>();
        grab.attachTransform          = spawnedAttach;
        grab.movementType             = XRBaseInteractable.MovementType.VelocityTracking; // Follows hand freely
        grab.trackPosition            = true;
        grab.trackRotation            = false;   // No spinning when held
        grab.throwOnDetach            = true;
        grab.throwSmoothingDuration   = 0.1f;
        grab.throwVelocityScale       = 1.5f;
        grab.useDynamicAttach         = false;

        // Freeze rotation so weapon doesn't tumble when dropped
        rb.drag        = 2f;
        rb.angularDrag = 5f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

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
