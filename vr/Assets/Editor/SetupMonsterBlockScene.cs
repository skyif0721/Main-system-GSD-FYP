using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SetupMonsterBlockScene
{
    public static void Execute()
    {
        // 1. Find XR Origin
        GameObject xrOrigin = GameObject.Find("Complete XR Origin Set Up Variant");
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin not found!");
            return;
        }

        // 2. Add PlayerStats to XR Origin
        PlayerStats playerStats = xrOrigin.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = xrOrigin.AddComponent<PlayerStats>();
        }

        // 3. Create VR UI for HP
        Transform mainCamera = xrOrigin.transform.Find("Camera Offset/Main Camera");
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found in XR Origin!");
            return;
        }

        GameObject vrCanvasObj = new GameObject("VR_HP_Canvas");
        vrCanvasObj.transform.SetParent(mainCamera, false);
        vrCanvasObj.transform.localPosition = new Vector3(0, -0.2f, 0.5f); // Slightly below and in front of the camera
        vrCanvasObj.transform.localRotation = Quaternion.identity;
        vrCanvasObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        Canvas canvas = vrCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        vrCanvasObj.AddComponent<CanvasScaler>();
        vrCanvasObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = vrCanvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400, 100);

        // Create HP Slider
        GameObject sliderObj = new GameObject("HP_Slider");
        sliderObj.transform.SetParent(vrCanvasObj.transform, false);
        Slider slider = sliderObj.AddComponent<Slider>();
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(300, 40);
        sliderRect.anchoredPosition = Vector2.zero;

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, 0); // Padding

        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.red;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        slider.fillRect = fillRect;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;

        // Assign slider to PlayerStats
        playerStats.healthSlider = slider;

        // 4. Create Monster Block
        GameObject monsterBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        monsterBlock.name = "MonsterBlock";
        monsterBlock.transform.position = xrOrigin.transform.position + xrOrigin.transform.forward * 2f + Vector3.up * 1f;
        monsterBlock.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // Add Rigidbody so it can detect collisions properly
        Rigidbody rb = monsterBlock.AddComponent<Rigidbody>();
        rb.isKinematic = true; // Keep it static for now, or false if you want it to fall

        // Add MonsterBlock script
        MonsterBlock mbScript = monsterBlock.AddComponent<MonsterBlock>();
        mbScript.health = 50;
        mbScript.damageToPlayer = 10;
        mbScript.coinsToDrop = 20;

        // Make it red
        Renderer renderer = monsterBlock.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material redMat = new Material(Shader.Find("Standard"));
            redMat.color = Color.red;
            renderer.material = redMat;
        }

        Debug.Log("Setup complete: Added PlayerStats, VR HP Canvas, and MonsterBlock.");
    }
}
