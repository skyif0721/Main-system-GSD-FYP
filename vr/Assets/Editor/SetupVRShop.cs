using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class SetupVRShop
{
    public static void Execute()
    {
        // 1. Create the Shop Zone
        GameObject shopZoneObj = new GameObject("VR_Shop_Zone");
        shopZoneObj.transform.position = new Vector3(108.275f, 6.5f, 98.0f); // Place it near the player
        
        BoxCollider zoneCollider = shopZoneObj.AddComponent<BoxCollider>();
        zoneCollider.isTrigger = true;
        zoneCollider.size = new Vector3(3f, 3f, 3f); // 3x3x3 meter zone

        VRShopZone shopZoneScript = shopZoneObj.AddComponent<VRShopZone>();

        // 2. Create Open Shop Button Canvas
        GameObject openButtonCanvasObj = new GameObject("OpenShopButtonCanvas");
        openButtonCanvasObj.transform.SetParent(shopZoneObj.transform);
        openButtonCanvasObj.transform.localPosition = new Vector3(0, 1.2f, 0);
        openButtonCanvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
        
        Canvas openCanvas = openButtonCanvasObj.AddComponent<Canvas>();
        openCanvas.renderMode = RenderMode.WorldSpace;
        openButtonCanvasObj.AddComponent<CanvasScaler>();
        openButtonCanvasObj.AddComponent<GraphicRaycaster>();
        openButtonCanvasObj.AddComponent<TrackedDeviceGraphicRaycaster>(); // For VR interaction

        RectTransform openCanvasRect = openButtonCanvasObj.GetComponent<RectTransform>();
        openCanvasRect.sizeDelta = new Vector2(400, 200);

        // The Button itself
        GameObject openBtnObj = new GameObject("OpenButton");
        openBtnObj.transform.SetParent(openButtonCanvasObj.transform, false);
        Image btnImage = openBtnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        Button openBtn = openBtnObj.AddComponent<Button>();
        RectTransform openBtnRect = openBtnObj.GetComponent<RectTransform>();
        openBtnRect.sizeDelta = new Vector2(300, 100);

        GameObject openBtnTextObj = new GameObject("Text");
        openBtnTextObj.transform.SetParent(openBtnObj.transform, false);
        Text openBtnText = openBtnTextObj.AddComponent<Text>();
        openBtnText.text = "Open Shop";
        openBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        openBtnText.fontSize = 40;
        openBtnText.alignment = TextAnchor.MiddleCenter;
        openBtnText.color = Color.white;
        RectTransform openBtnTextRect = openBtnTextObj.GetComponent<RectTransform>();
        openBtnTextRect.sizeDelta = new Vector2(300, 100);

        // Hook up button event
        UnityEngine.Events.UnityAction openAction = new UnityEngine.Events.UnityAction(shopZoneScript.OpenShop);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(openBtn.onClick, openAction);

        // 3. Create Shop Menu Canvas
        GameObject shopMenuCanvasObj = new GameObject("ShopMenuCanvas");
        shopMenuCanvasObj.transform.SetParent(shopZoneObj.transform);
        shopMenuCanvasObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        shopMenuCanvasObj.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

        Canvas menuCanvas = shopMenuCanvasObj.AddComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.WorldSpace;
        shopMenuCanvasObj.AddComponent<CanvasScaler>();
        shopMenuCanvasObj.AddComponent<GraphicRaycaster>();
        shopMenuCanvasObj.AddComponent<TrackedDeviceGraphicRaycaster>(); // For VR interaction

        RectTransform menuCanvasRect = shopMenuCanvasObj.GetComponent<RectTransform>();
        menuCanvasRect.sizeDelta = new Vector2(800, 600);

        // Menu Background
        GameObject menuBgObj = new GameObject("Background");
        menuBgObj.transform.SetParent(shopMenuCanvasObj.transform, false);
        Image menuBg = menuBgObj.AddComponent<Image>();
        menuBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        RectTransform menuBgRect = menuBgObj.GetComponent<RectTransform>();
        menuBgRect.anchorMin = Vector2.zero;
        menuBgRect.anchorMax = Vector2.one;
        menuBgRect.sizeDelta = Vector2.zero;

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(shopMenuCanvasObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "VR SHOP";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 60;
        titleText.alignment = TextAnchor.UpperCenter;
        titleText.color = Color.yellow;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(800, 100);
        titleRect.anchoredPosition = new Vector2(0, 250);

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(shopMenuCanvasObj.transform, false);
        Image closeBtnImage = closeBtnObj.AddComponent<Image>();
        closeBtnImage.color = Color.red;
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        RectTransform closeBtnRect = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRect.sizeDelta = new Vector2(100, 50);
        closeBtnRect.anchoredPosition = new Vector2(300, 250);

        GameObject closeBtnTextObj = new GameObject("Text");
        closeBtnTextObj.transform.SetParent(closeBtnObj.transform, false);
        Text closeBtnText = closeBtnTextObj.AddComponent<Text>();
        closeBtnText.text = "X";
        closeBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeBtnText.fontSize = 30;
        closeBtnText.alignment = TextAnchor.MiddleCenter;
        closeBtnText.color = Color.white;
        RectTransform closeBtnTextRect = closeBtnTextObj.GetComponent<RectTransform>();
        closeBtnTextRect.sizeDelta = new Vector2(100, 50);

        UnityEngine.Events.UnityAction closeAction = new UnityEngine.Events.UnityAction(shopZoneScript.CloseShop);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, closeAction);

        // Buy Item Button (Health Potion)
        GameObject buyBtnObj = new GameObject("BuyHealthButton");
        buyBtnObj.transform.SetParent(shopMenuCanvasObj.transform, false);
        Image buyBtnImage = buyBtnObj.AddComponent<Image>();
        buyBtnImage.color = Color.green;
        Button buyBtn = buyBtnObj.AddComponent<Button>();
        RectTransform buyBtnRect = buyBtnObj.GetComponent<RectTransform>();
        buyBtnRect.sizeDelta = new Vector2(400, 100);
        buyBtnRect.anchoredPosition = new Vector2(0, 0);

        GameObject buyBtnTextObj = new GameObject("Text");
        buyBtnTextObj.transform.SetParent(buyBtnObj.transform, false);
        Text buyBtnText = buyBtnTextObj.AddComponent<Text>();
        buyBtnText.text = "Buy Health (+20 HP)\nCost: 10 Coins";
        buyBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buyBtnText.fontSize = 30;
        buyBtnText.alignment = TextAnchor.MiddleCenter;
        buyBtnText.color = Color.white;
        RectTransform buyBtnTextRect = buyBtnTextObj.GetComponent<RectTransform>();
        buyBtnTextRect.sizeDelta = new Vector2(400, 100);

        VRShopItem shopItem = buyBtnObj.AddComponent<VRShopItem>();
        shopItem.price = 10;
        shopItem.itemName = "Health Potion";
        shopItem.healthRestore = 20;

        UnityEngine.Events.UnityAction buyAction = new UnityEngine.Events.UnityAction(shopItem.BuyItem);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(buyBtn.onClick, buyAction);

        // Assign references
        shopZoneScript.openShopButtonCanvas = openButtonCanvasObj;
        shopZoneScript.shopMenuCanvas = shopMenuCanvasObj;

        // Put it in UI folder
        GameObject uiFolder = GameObject.Find("--- UI ---");
        if (uiFolder != null)
        {
            shopZoneObj.transform.SetParent(uiFolder.transform);
        }

        Debug.Log("VR Shop created successfully!");
    }
}
