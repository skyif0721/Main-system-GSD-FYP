using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Adds a boss health bar stuck to the top of the player's screen-space UI
/// in the final-boss scene.
/// </summary>
public class SetupBossHealthBar
{
    [MenuItem("Tools/Setup Boss Health Bar")]
    public static void Execute()
    {
        // Find the player UI canvas (screen-space overlay on the player camera)
        GameObject playerUICanvas = GameObject.Find("PlayerUICanvas");
        if (playerUICanvas == null)
        {
            // Try the main camera child
            var cam = Camera.main;
            if (cam != null)
            {
                var canvasInCam = cam.GetComponentInChildren<Canvas>(true);
                if (canvasInCam != null)
                    playerUICanvas = canvasInCam.gameObject;
            }
        }

        if (playerUICanvas == null)
        {
            Debug.LogError("[BossHP] Could not find PlayerUICanvas!");
            return;
        }

        // Check if already exists
        if (playerUICanvas.transform.Find("BossHealthBar") != null)
        {
            Debug.Log("[BossHP] Boss health bar already exists.");
            return;
        }

        // Create the boss health bar panel at the top of the canvas
        GameObject panel = new GameObject("BossHealthBar");
        panel.transform.SetParent(playerUICanvas.transform, false);
        RectTransform panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.15f, 0.88f);
        panelRT.anchorMax = new Vector2(0.85f, 0.98f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        panel.AddComponent<CanvasRenderer>();
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);

        // Boss name text
        GameObject nameGO = new GameObject("BossName");
        nameGO.transform.SetParent(panel.transform, false);
        RectTransform nameRT = nameGO.AddComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 0.55f);
        nameRT.anchorMax = new Vector2(1, 1f);
        nameRT.offsetMin = new Vector2(10, 0);
        nameRT.offsetMax = new Vector2(-10, -2);
        nameGO.AddComponent<CanvasRenderer>();
        TextMeshProUGUI nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "BOSS";
        nameTMP.fontSize = 18;
        nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.color = new Color(1f, 0.3f, 0.2f);
        nameTMP.alignment = TextAlignmentOptions.Center;

        // Health slider
        GameObject sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(panel.transform, false);
        RectTransform sliderRT = sliderGO.AddComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.05f, 0.1f);
        sliderRT.anchorMax = new Vector2(0.95f, 0.5f);
        sliderRT.offsetMin = Vector2.zero;
        sliderRT.offsetMax = Vector2.zero;

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        slider.interactable = false;

        // Slider background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        bgGO.AddComponent<CanvasRenderer>();
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Fill area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = Vector2.zero;
        fillAreaRT.offsetMax = Vector2.zero;

        // Fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        RectTransform fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        fillGO.AddComponent<CanvasRenderer>();
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.8f, 0.15f, 0.1f, 1f);

        slider.fillRect = fillRT;

        // Health text overlay
        GameObject hpTextGO = new GameObject("HealthText");
        hpTextGO.transform.SetParent(sliderGO.transform, false);
        RectTransform hpTextRT = hpTextGO.AddComponent<RectTransform>();
        hpTextRT.anchorMin = Vector2.zero;
        hpTextRT.anchorMax = Vector2.one;
        hpTextRT.offsetMin = Vector2.zero;
        hpTextRT.offsetMax = Vector2.zero;
        hpTextGO.AddComponent<CanvasRenderer>();
        TextMeshProUGUI hpTMP = hpTextGO.AddComponent<TextMeshProUGUI>();
        hpTMP.text = "100 / 100";
        hpTMP.fontSize = 14;
        hpTMP.fontStyle = FontStyles.Bold;
        hpTMP.color = Color.white;
        hpTMP.alignment = TextAlignmentOptions.Center;

        // Add BossHealthBarUI component
        BossHealthBarUI bossUI = panel.AddComponent<BossHealthBarUI>();
        bossUI.healthSlider = slider;
        bossUI.bossNameText = nameTMP;
        bossUI.healthText = hpTMP;
        bossUI.healthBarPanel = panel;
        bossUI.bossDisplayName = "BOSS";
        bossUI.autoFindMonster = true;

        EditorUtility.SetDirty(panel);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[BossHP] Boss health bar added to PlayerUICanvas top.");
    }
}
