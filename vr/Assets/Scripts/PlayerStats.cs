using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthSlider;

    [Header("Mana")]
    public int maxMana = 100;
    public int currentMana;
    public Slider manaSlider;

    [Header("Mana Settings")]
    [Tooltip("Mana regenerated per second")]
    public int manaRegenPerSecond = 5;
    [Tooltip("Mana cost for fireball gesture")]
    public int fireballManaCost = 10;
    [Tooltip("Seconds after last fireball use before regen resumes")]
    public float manaRegenDelay = 3f;

    private float _lastFireballTime = -999f;
    private float _manaRegenTimer = 0f;
    private bool _regenPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        UpdateHealthUI();
        UpdateManaUI();

        // Load saved coins if any
        if (PlayerPrefs.HasKey("SavedCoins"))
        {
            ShopManager.coins = PlayerPrefs.GetInt("SavedCoins");
            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null)
            {
                shopManager.DisplayNumber(ShopManager.coins);
            }
        }
    }

    void Update()
    {
        HandleManaRegen();
    }

    // ─── Health ───────────────────────────────────────────────────────────────

    public void TakeDamage(int damage)
    {
        // If player is blocking, apply damage multiplier (0 = no damage)
        if (VRGestureResponse.PlayerIsBlocking)
        {
            damage = Mathf.RoundToInt(damage * VRGestureResponse.BlockDamageMultiplier);
            Debug.Log("[PlayerStats] Block active! Damage reduced to: " + damage);
        }

        if (damage > 0 && DamageFlash.Instance != null)
        {
            DamageFlash.Instance.Flash(500f);
        }

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();
        Debug.Log("Player took " + damage + " damage. Health remaining: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void HealFull()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        Debug.Log("[PlayerStats] Player fully healed!");
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // Handle player death (e.g., reload scene, show game over screen)
    }

    // ─── Mana ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by GestureActionHandler when the fireball/konan gesture fires.
    /// </summary>
    public bool UseFireballMana()
    {
        if (currentMana < fireballManaCost)
        {
            Debug.Log("[PlayerStats] Not enough mana for fireball!");
            return false;
        }
        currentMana -= fireballManaCost;
        if (currentMana < 0) currentMana = 0;
        _lastFireballTime = Time.time;
        _regenPaused = true;
        UpdateManaUI();
        Debug.Log($"[PlayerStats] Fireball used. Mana: {currentMana}/{maxMana}");
        return true;
    }

    private void HandleManaRegen()
    {
        if (currentMana >= maxMana) return;

        // Check if regen delay has passed since last fireball
        if (_regenPaused)
        {
            if (Time.time - _lastFireballTime >= manaRegenDelay)
            {
                _regenPaused = false;
                _manaRegenTimer = 0f;
            }
            else
            {
                return;
            }
        }

        _manaRegenTimer += Time.deltaTime;
        if (_manaRegenTimer >= 1f)
        {
            _manaRegenTimer -= 1f;
            currentMana = Mathf.Min(maxMana, currentMana + manaRegenPerSecond);
            UpdateManaUI();
        }
    }

    public void UpdateManaUI()
    {
        if (manaSlider != null)
        {
            manaSlider.maxValue = maxMana;
            manaSlider.value = currentMana;
        }
    }
}
