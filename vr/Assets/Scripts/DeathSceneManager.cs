using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Death scene controller. Shows "Ouch..." text with fade animation,
/// then allows player to rejoin the main gameplay.
/// Attach to a manager object in the Death scene.
/// </summary>
public class DeathSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public Canvas deathCanvas;
    public TextMeshProUGUI deathText;
    public TextMeshProUGUI subtitleText;
    public Button rejoinButton;
    public Button mainMenuButton;

    [Header("Scene Indices")]
    public int mainGameSceneIndex = 1;
    public int mainMenuSceneIndex = 0;

    [Header("Animation")]
    public float fadeInDuration = 2f;
    public float textDelay = 1f;
    public float buttonDelay = 3f;

    private CanvasGroup _canvasGroup;

    void Start()
    {
        if (deathCanvas == null)
            CreateDeathUI();

        StartCoroutine(PlayDeathSequence());
    }

    void CreateDeathUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("DeathCanvas");
        deathCanvas = canvasGO.AddComponent<Canvas>();
        deathCanvas.renderMode = RenderMode.WorldSpace;
        deathCanvas.sortingOrder = 200;

        RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(800, 600);
        canvasRT.localScale = Vector3.one * 0.005f;

        // Position in front of camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            canvasGO.transform.position = cam.transform.position + cam.transform.forward * 2f;
            canvasGO.transform.rotation = Quaternion.LookRotation(canvasGO.transform.position - cam.transform.position);
        }
        else
        {
            canvasGO.transform.position = new Vector3(0, 1.5f, 2f);
        }

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        _canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;

        // Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.02f, 0.02f, 0.95f);
        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // "Ouch..." text
        GameObject textGO = new GameObject("DeathText");
        textGO.transform.SetParent(canvasGO.transform, false);
        deathText = textGO.AddComponent<TextMeshProUGUI>();
        deathText.text = "Ouch...";
        deathText.fontSize = 72;
        deathText.fontStyle = FontStyles.Bold;
        deathText.color = new Color(0.9f, 0.2f, 0.2f);
        deathText.alignment = TextAlignmentOptions.Center;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0.5f);
        textRT.anchorMax = new Vector2(1, 0.8f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // Subtitle
        GameObject subGO = new GameObject("SubtitleText");
        subGO.transform.SetParent(canvasGO.transform, false);
        subtitleText = subGO.AddComponent<TextMeshProUGUI>();
        subtitleText.text = "You have fallen in battle...\nBut your journey is not over!";
        subtitleText.fontSize = 28;
        subtitleText.color = new Color(0.8f, 0.8f, 0.8f);
        subtitleText.alignment = TextAlignmentOptions.Center;
        RectTransform subRT = subGO.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0, 0.35f);
        subRT.anchorMax = new Vector2(1, 0.55f);
        subRT.offsetMin = new Vector2(40, 0);
        subRT.offsetMax = new Vector2(-40, 0);

        // Rejoin Button
        GameObject rejoinGO = CreateButton(canvasGO.transform, "RejoinButton",
            "Rejoin Battle", new Vector2(0, 0.15f), new Vector2(1, 0.28f),
            new Color(0.1f, 0.55f, 0.2f));
        rejoinButton = rejoinGO.GetComponent<Button>();
        rejoinButton.onClick.AddListener(RejoinGame);
        rejoinGO.SetActive(false);

        // Main Menu Button
        GameObject menuGO = CreateButton(canvasGO.transform, "MainMenuButton",
            "Main Menu", new Vector2(0, 0.02f), new Vector2(1, 0.13f),
            new Color(0.4f, 0.15f, 0.15f));
        mainMenuButton = menuGO.GetComponent<Button>();
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        menuGO.SetActive(false);
    }

    GameObject CreateButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = color;
        Button btn = btnGO.AddComponent<Button>();
        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = anchorMin;
        btnRT.anchorMax = anchorMax;
        btnRT.offsetMin = new Vector2(150, 0);
        btnRT.offsetMax = new Vector2(-150, 0);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btnGO;
    }

    IEnumerator PlayDeathSequence()
    {
        // Fade in the canvas
        if (_canvasGroup == null)
            _canvasGroup = deathCanvas.GetComponent<CanvasGroup>();

        if (_canvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeInDuration)
            {
                timer += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(textDelay);

        // Show subtitle with typewriter effect
        if (subtitleText != null)
        {
            string fullText = subtitleText.text;
            subtitleText.text = "";
            foreach (char c in fullText)
            {
                subtitleText.text += c;
                yield return new WaitForSeconds(0.03f);
            }
        }

        yield return new WaitForSeconds(buttonDelay - textDelay);

        // Show buttons
        if (rejoinButton != null) rejoinButton.gameObject.SetActive(true);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(true);
    }

    public void RejoinGame()
    {
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();

        if (SceneTransitionManager.singleton != null)
            SceneTransitionManager.singleton.GoToSceneAsync(mainGameSceneIndex);
        else
            SceneManager.LoadScene(mainGameSceneIndex);
    }

    public void GoToMainMenu()
    {
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();

        if (SceneTransitionManager.singleton != null)
            SceneTransitionManager.singleton.GoToSceneAsync(mainMenuSceneIndex);
        else
            SceneManager.LoadScene(mainMenuSceneIndex);
    }
}
