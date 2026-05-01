using UnityEngine;
using UnityEditor;

public class AdjustVRShop4
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
                    openBtnCanvas.transform.Rotate(0, 180, 0); // Flip it so it faces the right way
                }

                GameObject shopMenuCanvas = GameObject.Find("ShopMenuCanvas");
                if (shopMenuCanvas != null)
                {
                    shopMenuCanvas.transform.LookAt(xrOrigin.transform);
                    shopMenuCanvas.transform.Rotate(0, 180, 0); // Flip it so it faces the right way
                }

                GameObject npcDialogueCanvas = GameObject.Find("NPCDialogueCanvas");
                if (npcDialogueCanvas != null)
                {
                    npcDialogueCanvas.transform.LookAt(xrOrigin.transform);
                    npcDialogueCanvas.transform.Rotate(0, 180, 0); // Flip it so it faces the right way
                }
            }
            
            Debug.Log("Adjusted VR Shop Zone rotation to fix backwards text.");
        }
    }
}
