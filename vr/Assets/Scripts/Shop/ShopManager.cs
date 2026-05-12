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
                player.GetComponent<PlayerStats>().currentHealth += 50;
                player.GetComponent<PlayerStats>().UpdateHealthUI();
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

}
