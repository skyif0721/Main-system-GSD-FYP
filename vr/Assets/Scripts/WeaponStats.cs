using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attached to every spawned weapon.
/// - Deals damage to MonsterStat on trigger/collision.
/// - Has durability: cheaper weapons break faster.
/// - Shows a durability indicator that turns red as it degrades.
/// - When broken, the weapon is destroyed and the player is notified to respawn via shop.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WeaponStats : MonoBehaviour
{
    [Header("Weapon Identity")]
    public string weaponName = "Weapon";
    public int weaponShopIndex = 0;   // Which shop slot this came from

    [Header("Combat")]
    [Tooltip("Damage dealt per hit")]
    public int damage = 10;

    [Header("Durability")]
    [Tooltip("Max hits before weapon breaks")]
    public int maxDurability = 10;
    public int currentDurability;

    [Tooltip("Cooldown between hits (prevents multi-hit spam)")]
    public float hitCooldown = 0.4f;

    private float lastHitTime = -999f;
    private Renderer weaponRenderer;
    private Color originalColor;
    private bool isBroken = false;

    void Awake()
    {
        currentDurability = maxDurability;
        weaponRenderer = GetComponentInChildren<Renderer>();
        if (weaponRenderer != null)
            originalColor = weaponRenderer.material.color;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isBroken) return;
        TryHit(other.gameObject);
    }

    void OnCollisionEnter(Collision col)
    {
        if (isBroken) return;
        TryHit(col.gameObject);
    }

    void TryHit(GameObject target)
    {
        if (Time.time - lastHitTime < hitCooldown) return;

        // Hit MonsterStat (your enemy script)
        MonsterStat monster = target.GetComponentInParent<MonsterStat>();
        if (monster == null) monster = target.GetComponent<MonsterStat>();
        if (monster != null)
        {
            lastHitTime = Time.time;
            monster.TakeDamage(damage);

            // Play weapon hit SFX
            if (GameAudioManager.Instance != null)
                GameAudioManager.Instance.PlaySwordHit();

            Debug.Log($"[WeaponStats] {weaponName} hit {target.name} for {damage} dmg. Durability: {currentDurability}/{maxDurability}");
            UseDurability();
        }
    }

    void UseDurability()
    {
        currentDurability--;
        UpdateDurabilityVisual();

        if (currentDurability <= 0)
            BreakWeapon();
    }

    void UpdateDurabilityVisual()
    {
        if (weaponRenderer == null) return;
        float t = (float)currentDurability / maxDurability;
        // Lerp from original color → red as durability drops
        Color degraded = Color.Lerp(Color.red, originalColor, t);
        weaponRenderer.material.color = degraded;
    }

    void BreakWeapon()
    {
        isBroken = true;
        Debug.Log($"[WeaponStats] {weaponName} has BROKEN! Go to the shop to respawn it.");

        // Flash red briefly then destroy
        if (weaponRenderer != null)
            weaponRenderer.material.color = Color.red;

        // Drop from hand by disabling the grab interactable
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null) grab.enabled = false;

        Destroy(gameObject, 0.5f);
    }

    // Called by WeaponShopManager to set stats based on weapon tier
    public void SetStats(string name, int shopIndex, int dmg, int durability)
    {
        weaponName      = name;
        weaponShopIndex = shopIndex;
        damage          = dmg;
        maxDurability   = durability;
        currentDurability = durability;
    }
}
