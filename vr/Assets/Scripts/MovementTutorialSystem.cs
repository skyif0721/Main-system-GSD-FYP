using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Tutorial system that shows how to perform each movement/gesture.
/// Loops a demonstration animation model until the player performs the gesture.
/// Has 3 zones with buttons for each movement type (Rapier, Split, Block).
/// Also includes a gameplay description zone and a training zone.
/// </summary>
public class MovementTutorialSystem : MonoBehaviour
{
    [System.Serializable]
    public class MovementLesson
    {
        public string movementName;
        [TextArea(2, 4)]
        public string description;
        public string requiredGesture;  // gesture name to detect
        public GameObject demoModel;     // model that shows the movement
        public string demoAnimationClip; // animation clip name to play
        public bool completed;
    }

    [Header("Lessons")]
    public List<MovementLesson> lessons = new List<MovementLesson>();

    [Header("UI - World Space")]
    public Transform uiAnchor;  // Where to place the tutorial UI board

    [Header("References")]
    public PoseGestureDetector gestureDetector;
    public GestureActionHandler gestureHandler;

    [Header("Scene Navigation")]
    public int mainMenuSceneIndex = 0;
    public int mainGameSceneIndex = 1;

    // Runtime
    private int _currentLesson = -1;
    private bool _lessonActive = false;
    private string _lastGesture;
    private GameObject _tutorialBoard;
    private TextMeshPro _titleText;
    private TextMeshPro _descText;
    private TextMeshPro _statusText;
    private TextMeshPro _congratsText;
    private List<GameObject> _lessonButtons = new List<GameObject>();
    private GameObject _demoModelInstance;

    void Start()
    {
        if (gestureDetector == null)
            gestureDetector = FindObjectOfType<PoseGestureDetector>();
        if (gestureHandler == null)
            gestureHandler = FindObjectOfType<GestureActionHandler>();

        if (gestureDetector != null)
            gestureDetector.OnGestureRecognized += OnGestureRecognized;

        if (lessons.Count == 0)
            BuildDefaultLessons();

        CreateTutorialUI();
    }

    void OnDestroy()
    {
        if (gestureDetector != null)
            gestureDetector.OnGestureRecognized -= OnGestureRecognized;
    }

    void OnGestureRecognized(string gestureName)
    {
        _lastGesture = gestureName;

        if (_lessonActive && _currentLesson >= 0 && _currentLesson < lessons.Count)
        {
            var lesson = lessons[_currentLesson];
            if (!string.IsNullOrEmpty(lesson.requiredGesture))
            {
                string norm = lesson.requiredGesture.ToLowerInvariant().Trim();
                string lastNorm = gestureName.ToLowerInvariant().Trim();
                if (lastNorm.Contains(norm) || norm.Contains(lastNorm))
                {
                    CompleteCurrentLesson();
                }
            }
        }
    }

    void BuildDefaultLessons()
    {
        lessons = new List<MovementLesson>
        {
            new MovementLesson
            {
                movementName = "Rapier (Thrust)",
                description = "Extend your arm and thrust your controller FORWARD quickly!\n\nLike a fencing lunge - fast and straight.\n\n<color=#FFD700>This fires a piercing projectile!</color>",
                requiredGesture = "rapier"
            },
            new MovementLesson
            {
                movementName = "Split (Chop)",
                description = "Raise your hand UP, then swing it DOWN hard!\n\nLike chopping wood with an axe.\n\n<color=#FFD700>This creates a cone of damage in front of you!</color>",
                requiredGesture = "split"
            },
            new MovementLesson
            {
                movementName = "Block (Guard)",
                description = "Bring BOTH controllers together in front of your face!\n\nHold them close like a shield.\n\n<color=#FFD700>You become invulnerable while blocking!</color>",
                requiredGesture = "block"
            }
        };
    }

    void CreateTutorialUI()
    {
        Vector3 basePos = uiAnchor != null ? uiAnchor.position : new Vector3(0f, 1.5f, 3f);
        Quaternion baseRot = uiAnchor != null ? uiAnchor.rotation : Quaternion.Euler(0f, 180f, 0f);

        _tutorialBoard = new GameObject("TutorialBoard");
        _tutorialBoard.transform.position = basePos;
        _tutorialBoard.transform.rotation = baseRot;

        // Background
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "BoardBG";
        bg.transform.SetParent(_tutorialBoard.transform);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.02f);
        bg.transform.localRotation = Quaternion.identity;
        bg.transform.localScale = new Vector3(3.5f, 3f, 1f);
        Object.Destroy(bg.GetComponent<Collider>());
        var bgR = bg.GetComponent<Renderer>();
        Material bgMat = new Material(Shader.Find("Standard"));
        SetTransparent(bgMat, new Color(0.03f, 0.03f, 0.08f, 0.92f));
        bgR.material = bgMat;

        // Title
        _titleText = CreateTMPro(_tutorialBoard.transform, "Title",
            "Movement Tutorial", 0.9f, new Vector3(0f, 1.15f, 0f),
            new Color(1f, 0.85f, 0.3f), FontStyles.Bold, 2.5f);

        // Description
        _descText = CreateTMPro(_tutorialBoard.transform, "Description",
            "Select a movement below to learn how to perform it.\nThe demo will loop until you do it correctly!",
            0.4f, new Vector3(0f, 0.5f, 0f),
            new Color(0.85f, 0.88f, 0.95f), FontStyles.Normal, 3f);
        _descText.enableWordWrapping = true;

        // Status
        _statusText = CreateTMPro(_tutorialBoard.transform, "Status",
            "", 0.45f, new Vector3(0f, -0.1f, 0f),
            new Color(0.3f, 1f, 0.3f), FontStyles.Bold, 2.5f);

        // Congrats text (hidden initially)
        _congratsText = CreateTMPro(_tutorialBoard.transform, "Congrats",
            "", 0.6f, new Vector3(0f, -0.4f, 0f),
            new Color(1f, 0.85f, 0.2f), FontStyles.Bold, 3f);

        // Create lesson buttons using world-space canvas
        CreateLessonButtons();

        // Navigation buttons
        CreateNavigationButtons();
    }

    void CreateLessonButtons()
    {
        GameObject btnCanvasGO = new GameObject("LessonButtonCanvas");
        btnCanvasGO.transform.SetParent(_tutorialBoard.transform);
        btnCanvasGO.transform.localPosition = new Vector3(0f, -0.7f, -0.02f);
        btnCanvasGO.transform.localRotation = Quaternion.identity;
        Canvas btnCanvas = btnCanvasGO.AddComponent<Canvas>();
        btnCanvas.renderMode = RenderMode.WorldSpace;
        btnCanvasGO.AddComponent<CanvasScaler>();
        // Use TrackedDeviceGraphicRaycaster for VR ray interaction
        btnCanvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();
        RectTransform canvasRT = btnCanvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(600, 120);
        canvasRT.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        float btnWidth = 180f;
        float spacing = 10f;
        float totalWidth = lessons.Count * btnWidth + (lessons.Count - 1) * spacing;
        float startX = -totalWidth / 2f + btnWidth / 2f;

        for (int i = 0; i < lessons.Count; i++)
        {
            GameObject btnGO = new GameObject($"Lesson_{i}_Button");
            btnGO.transform.SetParent(btnCanvasGO.transform, false);
            RectTransform btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(btnWidth, 80);
            btnRT.anchoredPosition = new Vector2(startX + i * (btnWidth + spacing), 0);

            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.15f, 0.35f, 0.6f);
            Button btn = btnGO.AddComponent<Button>();
            int lessonIdx = i;
            btn.onClick.AddListener(() => StartLesson(lessonIdx));

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = lessons[i].movementName;
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            _lessonButtons.Add(btnGO);
        }
    }

    void CreateNavigationButtons()
    {
        GameObject navCanvasGO = new GameObject("NavButtonCanvas");
        navCanvasGO.transform.SetParent(_tutorialBoard.transform);
        navCanvasGO.transform.localPosition = new Vector3(0f, -1.2f, -0.02f);
        navCanvasGO.transform.localRotation = Quaternion.identity;
        Canvas navCanvas = navCanvasGO.AddComponent<Canvas>();
        navCanvas.renderMode = RenderMode.WorldSpace;
        navCanvasGO.AddComponent<CanvasScaler>();
        // Use TrackedDeviceGraphicRaycaster for VR ray interaction
        navCanvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();
        RectTransform canvasRT = navCanvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(500, 60);
        canvasRT.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        // Main Menu button
        CreateNavButton(navCanvasGO.transform, "MainMenuBtn", "Main Menu",
            new Vector2(-130, 0), new Color(0.5f, 0.15f, 0.15f), GoToMainMenu);

        // Start Game button
        CreateNavButton(navCanvasGO.transform, "StartGameBtn", "Start Game",
            new Vector2(130, 0), new Color(0.1f, 0.55f, 0.2f), GoToMainGame);
    }

    void CreateNavButton(Transform parent, string name, string label,
        Vector2 pos, Color color, UnityEngine.Events.UnityAction action)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        RectTransform btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(200, 50);
        btnRT.anchoredPosition = pos;

        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = color;
        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(action);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    public void StartLesson(int index)
    {
        if (index < 0 || index >= lessons.Count) return;

        _currentLesson = index;
        _lessonActive = true;
        _lastGesture = null;

        var lesson = lessons[index];

        if (_titleText != null)
            _titleText.text = lesson.movementName;
        if (_descText != null)
            _descText.text = lesson.description;
        if (_statusText != null)
            _statusText.text = "<color=#FFD700>Perform the gesture now!</color>\nThe demo loops until you do it.";
        if (_congratsText != null)
            _congratsText.text = "";

        // Play SFX
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();

        // Start demo model loop if available
        if (lesson.demoModel != null)
        {
            if (_demoModelInstance != null) Destroy(_demoModelInstance);
            Vector3 demoPos = _tutorialBoard.transform.position +
                _tutorialBoard.transform.right * 2f;
            _demoModelInstance = Instantiate(lesson.demoModel, demoPos, Quaternion.identity);
            _demoModelInstance.name = "DemoModel_" + lesson.movementName;

            // Loop the animation
            Animator anim = _demoModelInstance.GetComponent<Animator>();
            if (anim != null)
            {
                anim.Play(lesson.demoAnimationClip, 0, 0f);
                // Animation will loop by default if clip is set to loop
            }
        }

        // Update button colors
        for (int i = 0; i < _lessonButtons.Count; i++)
        {
            Image img = _lessonButtons[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = i == index
                    ? new Color(0.8f, 0.5f, 0.1f)
                    : (lessons[i].completed
                        ? new Color(0.1f, 0.5f, 0.15f)
                        : new Color(0.15f, 0.35f, 0.6f));
            }
        }

        Debug.Log($"[MovementTutorial] Started lesson: {lesson.movementName}");
    }

    void CompleteCurrentLesson()
    {
        if (_currentLesson < 0 || _currentLesson >= lessons.Count) return;

        var lesson = lessons[_currentLesson];
        lesson.completed = true;
        _lessonActive = false;

        if (_statusText != null)
            _statusText.text = "<color=#00FF88>COMPLETED!</color>";
        if (_congratsText != null)
            _congratsText.text = "Great job, you did it!\n" + GetMovementTip(lesson.requiredGesture);

        // Play success SFX
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayPunch();

        // Stop demo model
        if (_demoModelInstance != null)
        {
            Destroy(_demoModelInstance);
            _demoModelInstance = null;
        }

        // Update button color
        if (_currentLesson < _lessonButtons.Count)
        {
            Image img = _lessonButtons[_currentLesson].GetComponent<Image>();
            if (img != null) img.color = new Color(0.1f, 0.5f, 0.15f);
        }

        // Check if all lessons complete
        bool allDone = true;
        foreach (var l in lessons)
        {
            if (!l.completed) { allDone = false; break; }
        }

        if (allDone)
        {
            if (_titleText != null)
                _titleText.text = "All Movements Learned!";
            if (_descText != null)
                _descText.text = "Great job, you did it! You've mastered all the combat movements.\n\nYou're ready to fight!\n\nUse the buttons below to start the game or return to the menu.";
        }

        Debug.Log($"[MovementTutorial] Completed lesson: {lesson.movementName}");
    }

    string GetMovementTip(string gesture)
    {
        if (string.IsNullOrEmpty(gesture)) return "";
        string g = gesture.ToLowerInvariant();
        if (g.Contains("rapier"))
            return "Rapier: Thrust forward to fire a projectile.\nUse it for ranged attacks!";
        if (g.Contains("split"))
            return "Split: Chop downward for cone damage.\nGreat for close-range enemies!";
        if (g.Contains("block"))
            return "Block: Both hands up to become invulnerable.\nUse when enemies attack!";
        return "";
    }

    public void GoToMainMenu()
    {
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();

        if (SceneTransitionManager.singleton != null)
            SceneTransitionManager.singleton.GoToSceneAsync(mainMenuSceneIndex);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneIndex);
    }

    public void GoToMainGame()
    {
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();

        if (SceneTransitionManager.singleton != null)
            SceneTransitionManager.singleton.GoToSceneAsync(mainGameSceneIndex);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainGameSceneIndex);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    TextMeshPro CreateTMPro(Transform parent, string name, string text,
        float fontSize, Vector3 localPos, Color color, FontStyles style, float width)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(width, 1f);
        tmp.enableWordWrapping = true;
        tmp.richText = true;

        // Add billboard so text always faces camera
 

        return tmp;
    }

    void SetTransparent(Material mat, Color color)
    {
        mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        mat.color = color;
    }
}
