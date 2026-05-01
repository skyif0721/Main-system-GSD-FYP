using UnityEngine;
using UnityEditor;

public class AdjustVRShop
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
            }

            // Make the canvases larger and face the player
            GameObject openBtnCanvas = GameObject.Find("OpenShopButtonCanvas");
            if (openBtnCanvas != null)
            {
                openBtnCanvas.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                openBtnCanvas.transform.localPosition = new Vector3(0, 1.5f, 0);
                
                // Add a visual indicator for the zone
                GameObject zoneVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                zoneVisual.name = "ZoneVisual";
                zoneVisual.transform.SetParent(shopZone.transform);
                zoneVisual.transform.localPosition = Vector3.zero;
                zoneVisual.transform.localScale = new Vector3(3f, 0.05f, 3f);
                
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0, 1, 0, 0.3f); // Transparent green
                mat.SetFloat("_Mode", 3); // Transparent mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                
                zoneVisual.GetComponent<Renderer>().material = mat;
                GameObject.DestroyImmediate(zoneVisual.GetComponent<Collider>()); // Remove collider so it doesn't block
            }

            GameObject shopMenuCanvas = GameObject.Find("ShopMenuCanvas");
            if (shopMenuCanvas != null)
            {
                shopMenuCanvas.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                shopMenuCanvas.transform.localPosition = new Vector3(0, 1.5f, 0);
            }
            
            Debug.Log("Adjusted VR Shop Zone visibility and position.");
        }
    }
}
