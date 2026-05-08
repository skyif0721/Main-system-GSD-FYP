using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attach this to the coin prefab.
/// 
/// Flow:
///   1. Monster dies → coin is spawned via MonsterStat.Die()
///   2. Player grabs coin with VR controller (XRGrabInteractable handles this)
///   3. Player throws / drops coin inside the shop zone trigger
///   4. OnTriggerEnter fires → coins added to ShopManager → coin destroyed
/// 
/// Also auto-collects if the coin just falls into the shop zone without being grabbed.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CoinPickup : MonoBehaviour
{
    [Tooltip("How many coins this pickup is worth")]
    public int coinValue = 20;

    [Tooltip("Tag on the shop zone collider (set to 'ShopZone')")]
    public string shopZoneTag = "ShopZone";

    private bool collected = false;
    private XRGrabInteractable grabInteractable;
    
    [Tooltip("How long before the coin despawns (in seconds)")]
    public float despawnTime = 3f;
    private float timer = 0f;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        if (collected) return;

        // Don't despawn if the player is holding it
        bool isHeld = grabInteractable != null && grabInteractable.isSelected;
        if (isHeld)
        {
            timer = 0f; // Reset timer while held
            return;
        }

        timer += Time.deltaTime;
        if (timer >= despawnTime)
        {
            Debug.Log($"[CoinPickup] Coin despawned after {despawnTime} seconds.");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        // Check by tag OR by component (VRShopZone)
        bool isShopZone = other.CompareTag(shopZoneTag) || other.GetComponent<VRShopZone>() != null;
        if (!isShopZone) return;

        // Only collect if the player has released the coin (not currently held)
        bool isHeld = grabInteractable != null && grabInteractable.isSelected;
        if (isHeld) return;

        CollectCoin();
    }

    // Also check every frame while inside the zone in case the player releases inside
    void OnTriggerStay(Collider other)
    {
        if (collected) return;

        bool isShopZone = other.CompareTag(shopZoneTag) || other.GetComponent<VRShopZone>() != null;
        if (!isShopZone) return;

        bool isHeld = grabInteractable != null && grabInteractable.isSelected;
        if (isHeld) return; // still being held, wait

        CollectCoin();
    }

    void CollectCoin()
    {
        collected = true;

        // Add coins
        ShopManager.coins += coinValue;

        // Refresh all ShopManager UIs in scene
        ShopManager[] managers = FindObjectsOfType<ShopManager>();
        foreach (ShopManager sm in managers)
            sm.DisplayNumber(ShopManager.coins);

        // Persist
        PlayerPrefs.SetInt("SavedCoins", ShopManager.coins);
        PlayerPrefs.Save();

        Debug.Log($"[CoinPickup] +{coinValue} coins collected! Total: {ShopManager.coins}");

        Destroy(gameObject);
    }
}
