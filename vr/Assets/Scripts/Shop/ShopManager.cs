using Ink.Parsed;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public int[,] shopItems = new int[4, 12];
    public string[] shopItemsName = new string[12];
    public static int coins;
    public int coinreaching;
    public GameObject CoinsTxt;
    public GameObject player;

    public Sprite[] numberSprites; // Assign 0-9 in order
    public Image[] digitImages;    // Assign UI Image objects in order

    public GameObject potionPrefab;
    public Transform spawnPoint;

    void Start()
    {
        DisplayNumber(coins);

        shopItems[1, 1] = 1;
        shopItems[1, 2] = 2;
        shopItems[1, 3] = 3;
        shopItems[1, 4] = 4;

        shopItems[2, 1] = 50;
        shopItems[2, 2] = 50;
        shopItems[2, 3] = 10000;
        shopItems[2, 4] = 40;

        shopItems[3, 1] = 0;
        shopItems[3, 2] = 0;
        shopItems[3, 3] = 0;
        shopItems[3, 4] = 0;

        shopItemsName[1] = "Life";
        shopItemsName[2] = "Mana";
        shopItemsName[3] = "Attack";
        shopItemsName[4] = "Sword";

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
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsUIDisplayReady())
            DisplayNumber(coins);
    }

    private bool IsUIDisplayReady()
    {
        if (numberSprites == null || numberSprites.Length < 10) return false;
        if (digitImages == null || digitImages.Length == 0) return false;
        // Ensure all digits exist
        for (int i = 0; i < digitImages.Length; i++)
            if (digitImages[i] == null) return false;
        return true;
    }

    public void Buy()
    {
        var selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null) return;

        var info = selected.GetComponent<ButtonInfo>();
        if (info == null) return;

        int id = info.itemID;
        int price = shopItems[2, id];

        if (coins >= price)
        {
            coins -= price;
            shopItems[3, id]++;

            DisplayNumber(coins);

            if (info.quantityTxt != null)
                info.quantityTxt.text = shopItems[3, id].ToString();

            if (id == 1)
            {
                SpawnPotion();
            }

            if(id == 2)
            {
                player.GetComponent<PlayerStats>().currentMana += 50;
                player.GetComponent<PlayerStats>().UpdateManaUI();
            }

            if(id == 3)
            {
                // Attack damage + 1...
            }
        }
    }

    public void DisplayNumber(int number)
    {
        if (digitImages == null || digitImages.Length == 0)
        {
            Debug.LogWarning($"{name}: digitImages is null or empty; cannot display number.");
            return;
        }

        if (numberSprites == null || numberSprites.Length < 10)
        {
            Debug.LogWarning($"{name}: numberSprites is null or does not contain 10 digits.");
            return;
        }

        Debug.Log($"coins : {coins}");

        string numStr = number.ToString();

        for (int i = 0; i < digitImages.Length; i++)
        {
            var img = digitImages[i];
            if (img == null)
            {
                // Skip null entries but let you know
                Debug.LogWarning($"{name}: digitImages[{i}] is null.");
                continue;
            }

            if (i < numStr.Length)
            {
                int digit = numStr[i] - '0';

                var sprite = (digit >= 0 && digit < numberSprites.Length) ? numberSprites[digit] : null;
                if (sprite == null)
                {
                    Debug.LogWarning($"{name}: numberSprites[{digit}] is null.");
                    img.gameObject.SetActive(false);
                    continue;
                }

                digitImages[i].sprite = numberSprites[digit];
                digitImages[i].gameObject.SetActive(true);
            }
            else
            {
                digitImages[i].gameObject.SetActive(false);
            }
        }
    }

    public void Update() {
        if(99999 < coins)
        {
            coins = 99999;
        }
        coinreaching = coins;
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

}
