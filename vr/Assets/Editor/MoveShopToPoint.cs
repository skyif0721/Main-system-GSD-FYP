using UnityEngine;
using UnityEditor;

public class MoveShopToPoint
{
    public static void Execute()
    {
        GameObject shopZone = GameObject.Find("VR_Shop_Zone");
        GameObject point = GameObject.Find("POint");

        if (shopZone != null && point != null)
        {
            // Move the shop zone to the Point's position
            shopZone.transform.position = point.transform.position;
            
            // Make the canvases face the player's starting position (or just face forward)
            GameObject xrOrigin = GameObject.Find("Complete XR Origin Set Up Variant");
            if (xrOrigin != null)
            {
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

            Debug.Log("Moved VR Shop Zone to POint position: " + point.transform.position);
        }
        else
        {
            if (shopZone == null) Debug.LogError("Could not find VR_Shop_Zone");
            if (point == null) Debug.LogError("Could not find POint");
        }
    }
}
