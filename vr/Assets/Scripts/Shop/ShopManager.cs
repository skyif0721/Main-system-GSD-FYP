using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public int[,] shopItems = new int[12, 12];
    public string[] shopItemsName = new string[12];
    public GameObject[] item = new GameObject[12];
    public static float coins;
    public TextMeshProUGUI CoinsTxt;

    int spawned = 0;

    void Start()
    {
        spawned = 0;
        CoinsTxt.text = "Coins: " + coins.ToString();

        shopItems[1, 1] = 1;
        shopItems[1, 2] = 2;
        shopItems[1, 3] = 3;
        shopItems[1, 4] = 4;

        shopItems[2, 1] = 10;
        shopItems[2, 2] = 20;
        shopItems[2, 3] = 30;
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

    public void Buy()
    {
        var selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Buy() called but no UI element is selected.");
            return;
        }

        var info = selected.GetComponent<ButtonInfo>();
        if (info == null)
        {
            Debug.LogWarning("Selected UI element has no ButtonInfo component.");
            return;
        }

        int id = info.itemID;
        int price = shopItems[2, id];
        

        if (coins >= price)
        {
            coins -= price;
            shopItems[3, id]++;

            CoinsTxt.text = "Coins: " + coins;

            if (info.quantityTxt != null)
                info.quantityTxt.text = shopItems[3, id].ToString();
            else
                Debug.LogWarning("ButtonInfo.quantityTxt is not assigned.");
        }
        else
        {
            Debug.Log("Not enough coins.");
        }

        spawnItem();
    }

    private void spawnItem()
    {
        if (spawned < shopItems[3, 4])
        {
            Instantiate(item[0], transform.position, Quaternion.identity);
        }
        spawned++;
    }

}
