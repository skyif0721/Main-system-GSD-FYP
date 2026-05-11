using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Editor script that builds a tutorial scene teaching:
///   - Rapier, Split, Block gestures
///   - How to use the weapon shop (buy, spawn, grab)
/// Uses 3D TextMeshPro boards for NPC-style dialogue/instructions.
/// All materials use the built-in Standard shader.
/// </summary>
public class BuildTutorialScene
{
    static Shader _standardShader;

    static Material MakeStandardMat(Color color, bool transparent = false)
    {
        if (_standardShader == null)
            _standardShader = Shader.Find("Standard");
        Material m = new Material(_standardShader);
        if (transparent)
        {
            m.SetFloat("_Mode", 3f); // Transparent
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = 3000;
        }
        m.color = color;
        return m;
    }

    static Material MakeEmissiveMat(Color color, Color emissionColor)
    {
        Material m = MakeStandardMat(color);
        m.SetColor("_EmissionColor", emissionColor);
        m.EnableKeyword("_EMISSION");
        return m;
    }

    [MenuItem("Tools/Build Tutorial Scene")]
    public static void Build()
    {
        // ── Create or open the tutorial scene ─────────────────────────────
        string scenePath = "Assets/SCENES/tutorial.unity";

        // Save current scene first
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        // Create a new empty scene
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Lighting ──────────────────────────────────────────────────────
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.22f, 0.24f, 0.30f);
        RenderSettings.ambientEquatorColor = new Color(0.15f, 0.16f, 0.18f);
        RenderSettings.ambientGroundColor = new Color(0.08f, 0.07f, 0.06f);
        RenderSettings.ambientIntensity = 0.8f;

        // Skybox
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Gradient Skybox.mat");
        if (skyMat != null) RenderSettings.skybox = skyMat;

        // Directional light
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.95f, 0.85f);
        light.intensity = 1.2f;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // ── Ground plane ──────────────────────────────────────────────────
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 1f, 10f);
        ground.isStatic = true;
        ground.GetComponent<Renderer>().material = MakeStandardMat(new Color(0.25f, 0.30f, 0.22f));

        // ── XR Origin (player) ───────────────────────────────────────────
        GameObject xrOriginPrefab = null;
        string[] guids = AssetDatabase.FindAssets("Complete XR Origin Set Up Variant t:Prefab");
        if (guids.Length > 0)
            xrOriginPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));

        GameObject player = null;
        if (xrOriginPrefab != null)
        {
            player = (GameObject)PrefabUtility.InstantiatePrefab(xrOriginPrefab);
            player.transform.position = new Vector3(0f, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("[BuildTutorial] Could not find XR Origin prefab. Creating placeholder camera.");
            player = new GameObject("Complete XR Origin Set Up Variant");
            player.transform.position = new Vector3(0f, 0f, 0f);
            GameObject camOffset = new GameObject("Camera Offset");
            camOffset.transform.SetParent(player.transform);
            camOffset.transform.localPosition = new Vector3(0f, 1.36f, 0f);
            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            cam.AddComponent<Camera>();
            cam.AddComponent<AudioListener>();
            cam.transform.SetParent(camOffset.transform);
            cam.transform.localPosition = Vector3.zero;
        }

        // Add PlayerStats if not present
        if (player.GetComponent<PlayerStats>() == null)
            player.AddComponent<PlayerStats>();
        if (player.GetComponent<VRGestureDetector>() == null)
            player.AddComponent<VRGestureDetector>();
        if (player.GetComponent<VRGestureResponse>() == null)
            player.AddComponent<VRGestureResponse>();

        // ── EventSystem ──────────────────────────────────────────────────
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // ── Tutorial Board (3D text wall in front of player) ─────────────
        Vector3 boardPos = new Vector3(0f, 1.8f, 4f);
        Quaternion boardRot = Quaternion.Euler(0f, 180f, 0f);

        GameObject tutorialBoard = new GameObject("TutorialBoard");
        tutorialBoard.transform.position = boardPos;
        tutorialBoard.transform.rotation = boardRot;

        // Background quad
        GameObject boardBG = GameObject.CreatePrimitive(PrimitiveType.Quad);
        boardBG.name = "Background";
        boardBG.transform.SetParent(tutorialBoard.transform);
        boardBG.transform.localPosition = new Vector3(0f, 0f, 0.02f);
        boardBG.transform.localRotation = Quaternion.identity;
        boardBG.transform.localScale = new Vector3(4f, 2.5f, 1f);
        Object.DestroyImmediate(boardBG.GetComponent<Collider>());
        boardBG.GetComponent<Renderer>().material = MakeStandardMat(new Color(0.05f, 0.05f, 0.12f, 0.92f), true);

        // Border frame
        GameObject boardFrame = GameObject.CreatePrimitive(PrimitiveType.Quad);
        boardFrame.name = "Frame";
        boardFrame.transform.SetParent(tutorialBoard.transform);
        boardFrame.transform.localPosition = new Vector3(0f, 0f, 0.025f);
        boardFrame.transform.localRotation = Quaternion.identity;
        boardFrame.transform.localScale = new Vector3(4.1f, 2.6f, 1f);
        Object.DestroyImmediate(boardFrame.GetComponent<Collider>());
        boardFrame.GetComponent<Renderer>().material = MakeStandardMat(new Color(0.15f, 0.35f, 0.65f, 0.95f), true);

        // Title text
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(tutorialBoard.transform);
        titleGO.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        titleGO.transform.localRotation = Quaternion.identity;
        TextMeshPro titleTMP = titleGO.AddComponent<TextMeshPro>();
        titleTMP.text = "Welcome, Warrior!";
        titleTMP.fontSize = 1.2f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = new Color(1f, 0.85f, 0.3f);
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.rectTransform.sizeDelta = new Vector2(3.6f, 0.5f);
        titleTMP.enableWordWrapping = true;

        // Description text
        GameObject descGO = new GameObject("DescriptionText");
        descGO.transform.SetParent(tutorialBoard.transform);
        descGO.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        descGO.transform.localRotation = Quaternion.identity;
        TextMeshPro descTMP = descGO.AddComponent<TextMeshPro>();
        descTMP.text = "Loading tutorial...";
        descTMP.fontSize = 0.55f;
        descTMP.color = new Color(0.9f, 0.92f, 1f);
        descTMP.alignment = TextAlignmentOptions.Center;
        descTMP.rectTransform.sizeDelta = new Vector2(3.4f, 1.6f);
        descTMP.enableWordWrapping = true;
        descTMP.richText = true;

        // Step counter text
        GameObject stepGO = new GameObject("StepCounterText");
        stepGO.transform.SetParent(tutorialBoard.transform);
        stepGO.transform.localPosition = new Vector3(0f, -1.0f, 0f);
        stepGO.transform.localRotation = Quaternion.identity;
        TextMeshPro stepTMP = stepGO.AddComponent<TextMeshPro>();
        stepTMP.text = "";
        stepTMP.fontSize = 0.4f;
        stepTMP.color = new Color(0.5f, 0.6f, 0.8f);
        stepTMP.alignment = TextAlignmentOptions.Center;
        stepTMP.rectTransform.sizeDelta = new Vector2(3.4f, 0.3f);

        // Gesture result text
        GameObject gestureResultGO = new GameObject("GestureResultText");
        gestureResultGO.transform.SetParent(tutorialBoard.transform);
        gestureResultGO.transform.localPosition = new Vector3(0f, -0.75f, 0f);
        gestureResultGO.transform.localRotation = Quaternion.identity;
        TextMeshPro gestureResultTMP = gestureResultGO.AddComponent<TextMeshPro>();
        gestureResultTMP.text = "";
        gestureResultTMP.fontSize = 0.5f;
        gestureResultTMP.color = new Color(1f, 0.85f, 0f);
        gestureResultTMP.alignment = TextAlignmentOptions.Center;
        gestureResultTMP.rectTransform.sizeDelta = new Vector2(3.4f, 0.4f);
        gestureResultTMP.richText = true;

        // ── Gesture Action Handler ───────────────────────────────────────
        GameObject gestureHandlerGO = new GameObject("GestureActionHandler");
        GestureActionHandler gah = gestureHandlerGO.AddComponent<GestureActionHandler>();
        PoseGestureDetector pgd = gestureHandlerGO.AddComponent<PoseGestureDetector>();
        pgd.actionHandler = gah;

        GameObject fireballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Fireball.prefab");
        if (fireballPrefab != null)
            gah.fireballPrefab = fireballPrefab;

        // ── Shop Zone ────────────────────────────────────────────────────
        Vector3 shopPos = new Vector3(8f, 0f, 4f);
        GameObject shopZoneGO = new GameObject("VR_Shop_Zone");
        shopZoneGO.transform.position = shopPos;
        shopZoneGO.tag = "ShopZone";
        BoxCollider shopCollider = shopZoneGO.AddComponent<BoxCollider>();
        shopCollider.isTrigger = true;
        shopCollider.size = new Vector3(5f, 3f, 5f);
        VRShopZone vrShopZone = shopZoneGO.AddComponent<VRShopZone>();

        // Shop zone visual (floor marker)
        GameObject shopFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        shopFloor.name = "ZoneVisual";
        shopFloor.transform.SetParent(shopZoneGO.transform);
        shopFloor.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        shopFloor.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        Object.DestroyImmediate(shopFloor.GetComponent<Collider>());
        shopFloor.GetComponent<Renderer>().material = MakeStandardMat(new Color(0.2f, 0.5f, 0.3f, 0.5f), true);

        // Shop sign
        GameObject shopSign = new GameObject("ShopSign");
        shopSign.transform.position = shopPos + new Vector3(0f, 2.5f, -2.5f);
        shopSign.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        TextMeshPro shopSignTMP = shopSign.AddComponent<TextMeshPro>();
        shopSignTMP.text = "WEAPON SHOP\n<size=60%>Walk here to browse</size>";
        shopSignTMP.fontSize = 1.0f;
        shopSignTMP.fontStyle = FontStyles.Bold;
        shopSignTMP.color = new Color(1f, 0.85f, 0.3f);
        shopSignTMP.alignment = TextAlignmentOptions.Center;
        shopSignTMP.rectTransform.sizeDelta = new Vector2(3f, 1f);

        // Arrow on ground pointing to shop
        GameObject arrowSign = new GameObject("ArrowToShop");
        arrowSign.transform.position = new Vector3(4f, 0.05f, 4f);
        arrowSign.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
        TextMeshPro arrowTMP = arrowSign.AddComponent<TextMeshPro>();
        arrowTMP.text = ">>> SHOP >>>";
        arrowTMP.fontSize = 1.5f;
        arrowTMP.fontStyle = FontStyles.Bold;
        arrowTMP.color = new Color(0.2f, 0.8f, 0.3f);
        arrowTMP.alignment = TextAlignmentOptions.Center;
        arrowTMP.rectTransform.sizeDelta = new Vector2(4f, 0.5f);

        // Open Shop Button Canvas (World Space)
        GameObject openBtnCanvas = CreateWorldCanvas("OpenShopButtonCanvas",
            shopPos + new Vector3(0f, 1.5f, -1.5f), new Vector2(200, 80));
        openBtnCanvas.transform.SetParent(shopZoneGO.transform);
        openBtnCanvas.SetActive(false);
        CreateButton(openBtnCanvas, "OpenButton", "Open Shop",
            new Color(0.1f, 0.6f, 0.2f), new Vector2(180, 60));
        vrShopZone.openShopButtonCanvas = openBtnCanvas;

        // Shop Menu Canvas (World Space)
        GameObject shopMenuCanvas = CreateWorldCanvas("ShopMenuCanvas",
            shopPos + new Vector3(0f, 1.8f, 0f), new Vector2(500, 600));
        shopMenuCanvas.transform.SetParent(shopZoneGO.transform);
        shopMenuCanvas.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        shopMenuCanvas.SetActive(false);
        vrShopZone.shopMenuCanvas = shopMenuCanvas;

        // Add WeaponShopManager to shop menu
        WeaponShopManager wsm = shopMenuCanvas.AddComponent<WeaponShopManager>();

        // Create a simple shop panel inside the menu canvas
        GameObject shopPanel = new GameObject("WeaponShopPanel");
        shopPanel.transform.SetParent(shopMenuCanvas.transform);
        RectTransform panelRT = shopPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Header text in shop
        GameObject headerGO = new GameObject("Header");
        headerGO.transform.SetParent(shopPanel.transform);
        RectTransform headerRT = headerGO.AddComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0, 0.85f);
        headerRT.anchorMax = new Vector2(1, 1f);
        headerRT.offsetMin = Vector2.zero;
        headerRT.offsetMax = Vector2.zero;
        var headerTMP = headerGO.AddComponent<TextMeshProUGUI>();
        headerTMP.text = "Weapon Shop";
        headerTMP.fontSize = 28;
        headerTMP.fontStyle = FontStyles.Bold;
        headerTMP.color = new Color(1f, 0.85f, 0.3f);
        headerTMP.alignment = TextAlignmentOptions.Center;

        // ── Gesture Info Boards (3 pillars showing each gesture) ─────────
        CreateGestureInfoPillar("RapierInfo",
            new Vector3(-3f, 1.5f, 4f),
            "RAPIER",
            "Thrust Forward\n> Ranged Projectile\nDamage: 60",
            new Color(0.4f, 0.9f, 1f));

        CreateGestureInfoPillar("SplitInfo",
            new Vector3(0f, 1.5f, 6f),
            "SPLIT",
            "Chop Downward\nv Cone Damage\nDamage: 45",
            new Color(1f, 0.95f, 0.2f));

        CreateGestureInfoPillar("BlockInfo",
            new Vector3(3f, 1.5f, 4f),
            "BLOCK",
            "Both Hands Up\nInvulnerable\nDuration: 2.5s",
            new Color(0.3f, 0.7f, 1f));

        // ── Training Dummy ───────────────────────────────────────────────
        Material dummyMat = MakeStandardMat(new Color(0.6f, 0.2f, 0.2f));

        GameObject dummyParent = new GameObject("TrainingDummy");
        dummyParent.transform.position = new Vector3(0f, 0f, 7f);

        GameObject dummyBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        dummyBody.name = "Body";
        dummyBody.transform.SetParent(dummyParent.transform);
        dummyBody.transform.localPosition = new Vector3(0f, 1f, 0f);
        dummyBody.GetComponent<Renderer>().material = dummyMat;

        GameObject dummyHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dummyHead.name = "Head";
        dummyHead.transform.SetParent(dummyParent.transform);
        dummyHead.transform.localPosition = new Vector3(0f, 2.1f, 0f);
        dummyHead.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        dummyHead.GetComponent<Renderer>().material = dummyMat;

        // Dummy label
        GameObject dummyLabel = new GameObject("DummyLabel");
        dummyLabel.transform.SetParent(dummyParent.transform);
        dummyLabel.transform.localPosition = new Vector3(0f, 2.8f, 0f);
        dummyLabel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        TextMeshPro dummyTMP = dummyLabel.AddComponent<TextMeshPro>();
        dummyTMP.text = "Training Dummy\n<size=60%>Practice your moves!</size>";
        dummyTMP.fontSize = 0.6f;
        dummyTMP.color = new Color(1f, 0.4f, 0.4f);
        dummyTMP.alignment = TextAlignmentOptions.Center;
        dummyTMP.rectTransform.sizeDelta = new Vector2(2f, 0.6f);

        // ── Tutorial Manager ─────────────────────────────────────────────
        GameObject tutorialMgrGO = new GameObject("TutorialManager");
        TutorialManager tm = tutorialMgrGO.AddComponent<TutorialManager>();
        tm.titleText = titleTMP;
        tm.descriptionText = descTMP;
        tm.stepCounterText = stepTMP;
        tm.gestureResultText = gestureResultTMP;
        tm.gestureDetector = pgd;
        tm.gestureHandler = gah;
        tm.shopZone = vrShopZone;
        tm.weaponShop = wsm;

        // Tutorial-aware shop zone trigger
        TutorialShopTrigger tst = shopZoneGO.AddComponent<TutorialShopTrigger>();
        tst.tutorialManager = tm;

        // ── Organize hierarchy ───────────────────────────────────────────
        GameObject envRoot = new GameObject("--- ENVIRONMENT ---");
        ground.transform.SetParent(envRoot.transform);
        dummyParent.transform.SetParent(envRoot.transform);

        GameObject uiRoot = new GameObject("--- UI ---");
        tutorialBoard.transform.SetParent(uiRoot.transform);
        shopZoneGO.transform.SetParent(uiRoot.transform);
        shopSign.transform.SetParent(uiRoot.transform);
        arrowSign.transform.SetParent(uiRoot.transform);

        GameObject managersRoot = new GameObject("--- MANAGERS ---");
        gestureHandlerGO.transform.SetParent(managersRoot.transform);
        tutorialMgrGO.transform.SetParent(managersRoot.transform);
        lightGO.transform.SetParent(managersRoot.transform);

        GameObject playerRoot = new GameObject("--- PLAYER ---");
        player.transform.SetParent(playerRoot.transform);

        GameObject infoRoot = new GameObject("--- GESTURE INFO ---");
        GameObject.Find("RapierInfo")?.transform.SetParent(infoRoot.transform);
        GameObject.Find("SplitInfo")?.transform.SetParent(infoRoot.transform);
        GameObject.Find("BlockInfo")?.transform.SetParent(infoRoot.transform);

        // ── Wire up the Open Shop button ─────────────────────────────────
        var openBtn = openBtnCanvas.GetComponentInChildren<UnityEngine.UI.Button>();
        if (openBtn != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                openBtn.onClick, vrShopZone.OpenShop);
        }

        // ── Save scene ───────────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[BuildTutorial] Tutorial scene saved to {scenePath}");

        AddSceneToBuildSettings(scenePath);
    }

    // ────────────────────────────────────────────────────────────────────────
    static void CreateGestureInfoPillar(string name, Vector3 position, string title, string desc, Color accentColor)
    {
        GameObject pillar = new GameObject(name);
        pillar.transform.position = position;
        pillar.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        // Background
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "BG";
        bg.transform.SetParent(pillar.transform);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.01f);
        bg.transform.localRotation = Quaternion.identity;
        bg.transform.localScale = new Vector3(1.8f, 1.4f, 1f);
        Object.DestroyImmediate(bg.GetComponent<Collider>());
        bg.GetComponent<Renderer>().material = MakeStandardMat(new Color(0.08f, 0.08f, 0.15f, 0.9f), true);

        // Title
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(pillar.transform);
        titleGO.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        titleGO.transform.localRotation = Quaternion.identity;
        TextMeshPro titleTMP = titleGO.AddComponent<TextMeshPro>();
        titleTMP.text = title;
        titleTMP.fontSize = 0.8f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = accentColor;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.rectTransform.sizeDelta = new Vector2(1.6f, 0.4f);
        titleTMP.richText = true;

        // Description
        GameObject descGO = new GameObject("Desc");
        descGO.transform.SetParent(pillar.transform);
        descGO.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        descGO.transform.localRotation = Quaternion.identity;
        TextMeshPro descTMP = descGO.AddComponent<TextMeshPro>();
        descTMP.text = desc;
        descTMP.fontSize = 0.45f;
        descTMP.color = new Color(0.85f, 0.88f, 0.95f);
        descTMP.alignment = TextAlignmentOptions.Center;
        descTMP.rectTransform.sizeDelta = new Vector2(1.5f, 0.8f);
        descTMP.enableWordWrapping = true;

        // Accent bar at bottom
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "AccentBar";
        bar.transform.SetParent(pillar.transform);
        bar.transform.localPosition = new Vector3(0f, -0.6f, 0f);
        bar.transform.localScale = new Vector3(1.6f, 0.04f, 0.04f);
        Object.DestroyImmediate(bar.GetComponent<Collider>());
        bar.GetComponent<Renderer>().material = MakeEmissiveMat(accentColor, accentColor * 2f);
    }

    static GameObject CreateWorldCanvas(string name, Vector3 position, Vector2 size)
    {
        GameObject canvasGO = new GameObject(name);
        canvasGO.transform.position = position;
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        RectTransform rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        return canvasGO;
    }

    static void CreateButton(GameObject canvasGO, string name, string label, Color color, Vector2 size)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        UnityEngine.UI.Image img = btnGO.AddComponent<UnityEngine.UI.Image>();
        img.color = color;
        btnGO.AddComponent<UnityEngine.UI.Button>();

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool found = false;
        foreach (var s in scenes)
        {
            if (s.path == scenePath) { found = true; s.enabled = true; break; }
        }
        if (!found)
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[BuildTutorial] Added {scenePath} to build settings.");
        }
    }
}
