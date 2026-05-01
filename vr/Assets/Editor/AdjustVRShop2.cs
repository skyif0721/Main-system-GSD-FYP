using UnityEngine;
using UnityEditor;

public class AdjustVRShop2
{
    public static void Execute()
    {
        GameObject shopZone = GameObject.Find("VR_Shop_Zone");
        if (shopZone != null)
        {
            // Move it closer to the player's starting position
            GameObject xrOrigin = GameObject.Find("Complete XR Origin Set Up Variant");
            if (xrOrigin != null)
            {
                shopZone.transform.position = xrOrigin.transform.position + xrOrigin.transform.forward * 2f;
                
                // Make the canvases face the player
                GameObject openBtnCanvas = GameObject.Find("OpenShopButtonCanvas");
                if (openBtnCanvas != null)
                {
                    openBtnCanvas.transform.LookAt(xrOrigin.transform);
                    openBtnCanvas.transform.Rotate(0, 180, 0); // Flip it so it faces the right way
                }

                GameObject shopMenuCanvas = GameObject.Find("ShopMenuCanvas");
                if (shopMenuCanvas != null)
                {
                    shopMenuCanvas.transform.LookAt(xrOrigin.transform);
                    shopMenuCanvas.transform.Rotate(0, 180, 0); // Flip it so it faces the right way
                }
            }
            
            Debug.Log("Adjusted VR Shop Zone rotation.");
        }
    }
}
