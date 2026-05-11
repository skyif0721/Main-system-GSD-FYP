using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the repeatable health potion purchase in the weapon shop.
/// Attach to the shop menu canvas or any persistent manager.
/// When the player buys a potion, it spawns a green cross at the spawn point
/// that can be grabbed and consumed for full HP.
/// </summary>
public class PotionShopHandler : MonoBehaviour
{
    [Header("Potion Settings")]
    public int potionPrice = 15;
    public GameObject potionPrefab;
    public Transform spawnPoint;

    [Header("UI References")]
    public Button buyButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI priceText;

    void Start()
    {
        // Auto-find spawn point
        if (spawnPoint == null)
        {
            GameObject sp = GameObject.Find("WeaponSpawnPoint");
            if (sp != null) spawnPoint = sp.transform;
        }

        // Auto-find potion prefab from scene (the green cross 3d model)
        if (potionPrefab == null)
        {
            // Try to load from prefab
            potionPrefab = UnityEngine.Resources.Load<GameObject>("green cross 3d model");
            if (potionPrefab == null)
            {
                // Try to find in scene as template
                GameObject scenePotion = GameObject.Find("green cross 3d model");
                if (scenePotion != null)
                {
                    potionPrefab = scenePotion;
                    // Hide the template
                    scenePotion.SetActive(false);
                }
            }
        }

        // Auto-find buy button
        if (buyButton == null)
        {
            Transform potionRow = transform.root.GetComponentInChildren<Transform>()?.Find("Row_Potion");
            if (potionRow == null)
            {
                // Search more broadly
                var allRows = FindObjectsOfType<RectTransform>(true);
                foreach (var rt in allRows)
                {
                    if (rt.name == "Row_Potion")
                    {
                        potionRow = rt;
                        break;
                    }
                }
            }
            if (potionRow != null)
            {
                var btn = potionRow.Find("BuyButton");
                if (btn != null) buyButton = btn.GetComponent<Button>();
                var st = potionRow.Find("StatusText/Text");
                if (st != null) statusText = st.GetComponent<TextMeshProUGUI>();
                var pt = potionRow.Find("PriceText/Text");
                if (pt != null) priceText = pt.GetComponent<TextMeshProUGUI>();
            }
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyPotion);
        }

        UpdateUI();
    }

    void OnEnable()
    {
        UpdateUI();
    }

    public void BuyPotion()
    {
        if (ShopManager.coins < potionPrice)
        {
            if (statusText != null)
                statusText.text = "Need more coins!";
            return;
        }

        // Deduct coins
        ShopManager.coins -= potionPrice;
        PlayerPrefs.SetInt("SavedCoins", ShopManager.coins);
        PlayerPrefs.Save();

        // Update coin display in shop
        var wsm = FindObjectOfType<WeaponShopManager>();
        if (wsm != null) wsm.RefreshUI();

        // Update the main coin display
        ShopManager shopMgr = FindObjectOfType<ShopManager>();
        if (shopMgr != null) shopMgr.DisplayNumber(ShopManager.coins);

        // Spawn the potion
        SpawnPotion();

        if (statusText != null)
            statusText.text = "Spawned!";

        UpdateUI();
    }

    void SpawnPotion()
    {
        if (potionPrefab == null)
        {
            Debug.LogWarning("[PotionShop] No potion prefab assigned!");
            return;
        }

        Vector3 pos;
        Quaternion rot;

        if (spawnPoint != null)
        {
            pos = spawnPoint.position;
            rot = spawnPoint.rotation;
        }
        else
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                pos = cam.transform.position + cam.transform.forward * 0.5f + Vector3.down * 0.2f;
                rot = Quaternion.identity;
            }
            else
            {
                pos = Vector3.zero + Vector3.up;
                rot = Quaternion.identity;
            }
        }

        GameObject potion = Instantiate(potionPrefab, pos, rot);
        potion.name = "HealthPotion_Spawned";
        potion.SetActive(true);

        Rigidbody rb = potion.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("[PotionShop] Health potion spawned at " + pos);
    }

    void UpdateUI()
    {
        if (priceText != null)
            priceText.text = potionPrice + " coins";

        if (buyButton != null)
        {
            bool canAfford = ShopManager.coins >= potionPrice;
            buyButton.interactable = canAfford;
            var img = buyButton.GetComponent<Image>();
            if (img != null)
                img.color = canAfford
                    ? new Color(0.10f, 0.65f, 0.20f, 1f)
                    : new Color(0.25f, 0.25f, 0.25f, 1f);
        }
    }
}
