using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class AddNPCDialogueToShop
{
    public static void Execute()
    {
        GameObject shopZone = GameObject.Find("VR_Shop_Zone");
        if (shopZone != null)
        {
            // 1. Create NPC Dialogue Canvas
            GameObject npcDialogueCanvasObj = new GameObject("NPCDialogueCanvas");
            npcDialogueCanvasObj.transform.SetParent(shopZone.transform);
            
            // Position it slightly above and to the side of the main shop menu
            npcDialogueCanvasObj.transform.localPosition = new Vector3(0, 2.2f, 0);
            npcDialogueCanvasObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            Canvas dialogueCanvas = npcDialogueCanvasObj.AddComponent<Canvas>();
            dialogueCanvas.renderMode = RenderMode.WorldSpace;
            npcDialogueCanvasObj.AddComponent<CanvasScaler>();
            npcDialogueCanvasObj.AddComponent<GraphicRaycaster>();

            RectTransform dialogueCanvasRect = npcDialogueCanvasObj.GetComponent<RectTransform>();
            dialogueCanvasRect.sizeDelta = new Vector2(600, 150);

            // 2. Dialogue Background (Speech Bubble style)
            GameObject dialogueBgObj = new GameObject("Background");
            dialogueBgObj.transform.SetParent(npcDialogueCanvasObj.transform, false);
            Image dialogueBg = dialogueBgObj.AddComponent<Image>();
            dialogueBg.color = new Color(1f, 1f, 1f, 0.9f); // White background
            RectTransform dialogueBgRect = dialogueBgObj.GetComponent<RectTransform>();
            dialogueBgRect.anchorMin = Vector2.zero;
            dialogueBgRect.anchorMax = Vector2.one;
            dialogueBgRect.sizeDelta = Vector2.zero;

            // 3. Dialogue Text
            GameObject dialogueTextObj = new GameObject("Text");
            dialogueTextObj.transform.SetParent(npcDialogueCanvasObj.transform, false);
            Text dialogueText = dialogueTextObj.AddComponent<Text>();
            dialogueText.text = "Welcome to my shop! What can I get for you today?";
            dialogueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dialogueText.fontSize = 36;
            dialogueText.alignment = TextAnchor.MiddleCenter;
            dialogueText.color = Color.black; // Black text on white background
            RectTransform dialogueTextRect = dialogueTextObj.GetComponent<RectTransform>();
            dialogueTextRect.anchorMin = Vector2.zero;
            dialogueTextRect.anchorMax = Vector2.one;
            dialogueTextRect.sizeDelta = new Vector2(-20, -20); // Padding

            // 4. Make it face the player
            GameObject xrOrigin = GameObject.Find("Complete XR Origin Set Up Variant");
            if (xrOrigin != null)
            {
                npcDialogueCanvasObj.transform.LookAt(xrOrigin.transform);
            }

            // 5. Assign to VRShopZone script
            VRShopZone shopZoneScript = shopZone.GetComponent<VRShopZone>();
            if (shopZoneScript != null)
            {
                shopZoneScript.npcDialogueCanvas = npcDialogueCanvasObj;
                shopZoneScript.npcDialogueText = dialogueText;
            }

            // Hide it initially
            npcDialogueCanvasObj.SetActive(false);

            Debug.Log("Added NPC Dialogue to VR Shop.");
        }
    }
}
