using UnityEngine;
using UnityEditor;

public class AdjustVRShop3
{
    public static void Execute()
    {
        GameObject shopZone = GameObject.Find("VR_Shop_Zone");
        if (shopZone != null)
        {
            // Make the canvases face the player's starting position correctly
            GameObject xrOrigin = GameObject.Find("Complete XR Origin Set Up Variant");
            if (xrOrigin != null)
            {
                GameObject openBtnCanvas = GameObject.Find("OpenShopButtonCanvas");
                if (openBtnCanvas != null)
                {
                    openBtnCanvas.transform.LookAt(xrOrigin.transform);
                    // The text was backwards, so we don't rotate 180 this time
                }

                GameObject shopMenuCanvas = GameObject.Find("ShopMenuCanvas");
                if (shopMenuCanvas != null)
                {
                    shopMenuCanvas.transform.LookAt(xrOrigin.transform);
                    // The text was backwards, so we don't rotate 180 this time
                }
            }
            
            Debug.Log("Adjusted VR Shop Zone rotation to fix backwards text.");
        }
    }
}
