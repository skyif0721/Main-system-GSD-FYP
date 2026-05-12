using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TMPro;

/// <summary>
/// VR Pause Menu - allows player to go back to main menu or start the game
/// using controller buttons. Press the Menu button (left controller) to toggle.
/// Works in any scene.
/// </summary>
public class VRPauseMenu : MonoBehaviour
{
    [Header("Scene Indices")]
    public int mainMenuSceneIndex = 0;
    public int mainGameSceneIndex = 1;
    public int tutorialSceneIndex = 2;

    [Header("Settings")]
    public float menuDistance = 2f;

    private GameObject _menuCanvas;
    private bool _menuVisible = false;
    private bool _buttonWasPressed = false;
    private InputDevice _leftController;
    private bool _initialized = false;

    void Start()
    {
        CreateMenu();
        _menuCanvas.SetActive(false);
    }

    void Update()
    {
        // Try to get left controller
        if (!_initialized || !_leftController.isValid)
        {
            _leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (_leftController.isValid) _initialized = true;
        }

        if (!_initialized) return;

        // Check menu button press
        bool menuPressed = false;
        _leftController.TryGetFeatureValue(CommonUsages.menuButton, out menuPressed);

        if (menuPressed && !_buttonWasPressed)
        {
            ToggleMenu();
        }
        _buttonWasPressed = menuPressed;

        // Keep menu facing camera
        if (_menuVisible && _menuCanvas != null && Camera.main != null)
        {
            _menuCanvas.transform.position = Camera.main.transform.position +
                Camera.main.transform.forward * menuDistance;
            _menuCanvas.transform.rotation = Quaternion.LookRotation(
                _menuCanvas.transform.position - Camera.main.transform.position);
        }
    }

    void ToggleMenu()
    {
        _menuVisible = !_menuVisible;
        if (_menuCanvas != null)
            _menuCanvas.SetActive(_menuVisible);

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();
    }

    void CreateMenu()
    {
        _menuCanvas = new GameObject("VRPauseMenuCanvas");
        Canvas canvas = _menuCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 150;

        RectTransform canvasRT = _menuCanvas.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(500, 400);
        canvasRT.localScale = Vector3.one * 0.003f;

        _menuCanvas.AddComponent<CanvasScaler>();
        // Use TrackedDeviceGraphicRaycaster for VR ray interaction
        _menuCanvas.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(_menuCanvas.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Title
        CreateText(_menuCanvas.transform, "PAUSE", 40, FontStyles.Bold,
            new Color(1f, 0.85f, 0.3f), new Vector2(0, 0.75f), new Vector2(1, 0.95f));

        // Resume button
        CreateMenuButton(_menuCanvas.transform, "Resume", new Vector2(0, 0.55f), new Vector2(1, 0.7f),
            new Color(0.15f, 0.5f, 0.15f), () => ToggleMenu());

        // Main Menu button
        CreateMenuButton(_menuCanvas.transform, "Main Menu", new Vector2(0, 0.38f), new Vector2(1, 0.53f),
            new Color(0.5f, 0.3f, 0.1f), () => GoToScene(mainMenuSceneIndex));

        // Tutorial button
        CreateMenuButton(_menuCanvas.transform, "Tutorial", new Vector2(0, 0.21f), new Vector2(1, 0.36f),
            new Color(0.2f, 0.3f, 0.5f), () => GoToScene(tutorialSceneIndex));

        // Start Game button
        CreateMenuButton(_menuCanvas.transform, "Start Game", new Vector2(0, 0.04f), new Vector2(1, 0.19f),
            new Color(0.1f, 0.55f, 0.2f), () => GoToScene(mainGameSceneIndex));
    }

    void CreateMenuButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax,
        Color color, UnityEngine.Events.UnityAction action)
    {
        GameObject btnGO = new GameObject(label + "Button");
        btnGO.transform.SetParent(parent, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = color;
        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(action);
        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = anchorMin;
        btnRT.anchorMax = anchorMax;
        btnRT.offsetMin = new Vector2(60, 0);
        btnRT.offsetMax = new Vector2(-60, 0);

        CreateText(btnGO.transform, label, 24, FontStyles.Bold, Color.white,
            Vector2.zero, Vector2.one);
    }

    void CreateText(Transform parent, string text, int fontSize, FontStyles style,
        Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = anchorMin;
        textRT.anchorMax = anchorMax;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
    }

    void GoToScene(int sceneIndex)
    {
        _menuVisible = false;
        if (_menuCanvas != null) _menuCanvas.SetActive(false);

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();

        if (SceneTransitionManager.singleton != null)
            SceneTransitionManager.singleton.GoToSceneAsync(sceneIndex);
        else
            SceneManager.LoadScene(sceneIndex);
    }
}
