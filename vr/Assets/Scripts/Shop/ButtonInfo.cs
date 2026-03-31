using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonInfo : MonoBehaviour
{
    [SerializeField] public int itemID;
    public TextMeshProUGUI NameTxt;
    public TextMeshProUGUI priceTxt;
    public TextMeshProUGUI quantityTxt;
    public GameObject ShopManager;

    void Update()
    {
        NameTxt.text = ShopManager.GetComponent<ShopManager>().shopItemsName[itemID].ToString();
        priceTxt.text = "Price: $" + ShopManager.GetComponent<ShopManager>().shopItems[2, itemID].ToString();
        quantityTxt.text = "have: " + ShopManager.GetComponent<ShopManager>().shopItems[3, itemID].ToString();
    }
}
