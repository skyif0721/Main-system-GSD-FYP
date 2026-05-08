using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthSlider;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

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

    private void UpdateHealthUI()
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
}
