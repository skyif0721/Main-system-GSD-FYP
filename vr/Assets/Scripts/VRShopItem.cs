using UnityEngine;
using UnityEngine.UI;

public class VRShopItem : MonoBehaviour
{
    public int price = 10;
    public string itemName = "Health Potion";
    public int healthRestore = 20;

    public void BuyItem()
    {
        if (ShopManager.coins >= price)
        {
            ShopManager.coins -= price;
            
            // Update UI
            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null) shopManager.DisplayNumber(ShopManager.coins);
            
            PlayerPrefs.SetInt("SavedCoins", ShopManager.coins);
            PlayerPrefs.Save();

            // Apply effect
            PlayerStats stats = FindObjectOfType<PlayerStats>();
            if (stats != null)
            {
                stats.currentHealth += healthRestore;
                if (stats.currentHealth > stats.maxHealth) stats.currentHealth = stats.maxHealth;
                stats.TakeDamage(0); // Hack to update UI
            }
            
            Debug.Log($"Bought {itemName} for {price} coins!");
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }
}
