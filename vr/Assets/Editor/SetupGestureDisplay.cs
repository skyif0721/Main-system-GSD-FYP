using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;

public static class SetupGestureDisplay
{
    /// <summary>
    /// Creates / refreshes a world-space TextMeshPro display that shows the
    /// last-recognized gesture name, plus a PoseGestureDetector wired into
    /// the GestureActionHandler. Both are placed at a sensible spot near the
    /// player's spawn so the demo is visible.
    /// </summary>
    public static void Run()
    {
        // ── 1. Wall display ─────────────────────────────────────────────────
        const string DISPLAY_NAME = "GestureDisplayWall";
        GameObject display = GameObject.Find(DISPLAY_NAME);
        if (display != null) Object.DestroyImmediate(display);

        // Find player so we put text in front of them
        Vector3 wallPos = new Vector3(102f, 2.5f, 100f);   // sensible default near WeaponSpawnPoint
        Quaternion wallRot = Quaternion.identity;
        GameObject player = GameObject.Find("Complete XR Origin Set Up Variant");
        if (player != null)
        {
            // 4 m in front of player at eye-ish height
            Vector3 fwd = player.transform.forward;
            wallPos = player.transform.position + fwd * 4f + Vector3.up * 1.6f;
            // Make the text face the player (its forward axis points TO the player)
            wallRot = Quaternion.LookRotation(player.transform.position - wallPos, Vector3.up);
        }

        display = new GameObject(DISPLAY_NAME);
        display.transform.position = wallPos;
        display.transform.rotation = wallRot;

        // Background quad — flipped so the visible face points toward the
        // player (the quad's normal is its -Z, so we rotate it 180° on Y)
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "Background";
        bg.transform.SetParent(display.transform, false);
        bg.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.005f);   // tiny offset so text isn't z-fighting
        bg.transform.localScale    = new Vector3(2.6f, 0.9f, 1f);
        Object.DestroyImmediate(bg.GetComponent<Collider>());
        var bgRend = bg.GetComponent<Renderer>();
        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (urpUnlit == null) urpUnlit = Shader.Find("Unlit/Color");
        if (urpUnlit == null) urpUnlit = Shader.Find("Standard");
        Material bgMat = new Material(urpUnlit);
        ConfigureFade(bgMat, new Color(0.05f, 0.05f, 0.1f, 0.85f));
        bgRend.sharedMaterial = bgMat;

        // Title "GESTURE" – note: avoid setting outlineWidth in edit mode,
        // it triggers TMP to instantiate a material (warning + leak).
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(display.transform, false);
        titleGO.transform.localPosition = new Vector3(0f, 0.28f, -0.01f);
        TextMeshPro title = titleGO.AddComponent<TextMeshPro>();
        title.text = "<size=60%>LAST GESTURE</size>";
        title.fontSize = 0.6f;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(0.7f, 0.85f, 1f);
        title.rectTransform.sizeDelta = new Vector2(2.4f, 0.4f);
        title.fontStyle = FontStyles.Bold;

        // Recognized name label
        GameObject nameGO = new GameObject("RecognizedText");
        nameGO.transform.SetParent(display.transform, false);
        nameGO.transform.localPosition = new Vector3(0f, -0.05f, -0.01f);
        TextMeshPro recognized = nameGO.AddComponent<TextMeshPro>();
        recognized.text = "<i>(no gesture yet)</i>";
        recognized.fontSize = 1.2f;
        recognized.alignment = TextAlignmentOptions.Center;
        recognized.color = new Color(1f, 0.85f, 0.3f);
        recognized.fontStyle = FontStyles.Bold;
        recognized.rectTransform.sizeDelta = new Vector2(2.4f, 0.6f);

        // ── 2. PoseGestureDetector on the existing handler ──────────────────
        GestureActionHandler handler = Object.FindObjectOfType<GestureActionHandler>();
        if (handler == null)
        {
            Debug.LogWarning("[SetupGestureDisplay] No GestureActionHandler in scene; create one first.");
        }
        else
        {
            PoseGestureDetector detector = handler.GetComponent<PoseGestureDetector>();
            if (detector == null)
                detector = handler.gameObject.AddComponent<PoseGestureDetector>();

            detector.actionHandler   = handler;
            detector.recognizedLabel = recognized;
            handler.recognizedLabel  = recognized;
            EditorUtility.SetDirty(detector);
            EditorUtility.SetDirty(handler);
            Debug.Log("[SetupGestureDisplay] Wired PoseGestureDetector + GestureActionHandler to wall text.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[SetupGestureDisplay] Display placed at {wallPos}");
    }

    static void ConfigureFade(Material m, Color c)
    {
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Surface"))   m.SetFloat("_Surface", 1f);
        if (m.HasProperty("_Blend"))     m.SetFloat("_Blend",   0f);
        if (m.HasProperty("_ZWrite"))    m.SetFloat("_ZWrite",  0f);
        if (m.HasProperty("_SrcBlend"))  m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (m.HasProperty("_DstBlend"))  m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (m.HasProperty("_Mode"))      m.SetFloat("_Mode", 3f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
    }
}
