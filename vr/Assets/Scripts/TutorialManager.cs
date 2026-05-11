using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Step-by-step VR tutorial that teaches the player:
///   1. Welcome / look around
///   2. Rapier gesture (thrust forward)
///   3. Split gesture (chop downward)
///   4. Block gesture (both controllers together in front)
///   5. Walking to the shop zone
///   6. Tutorial complete
///
/// Tracks which steps are completed and shows a "Go to Main Game" button.
/// The player does NOT need to finish all steps — they can skip ahead at any time.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string title;
        [TextArea(3, 6)]
        public string description;
        [Tooltip("Gesture name the player must perform to advance (empty = auto-advance or manual).")]
        public string requiredGesture;
        [Tooltip("Seconds to wait before auto-advancing (0 = wait for gesture/manual).")]
        public float autoAdvanceDelay;
        [Tooltip("If true, step completes when player enters the shop zone.")]
        public bool requireShopZoneEntry;
        [Tooltip("If true, step completes when the shop menu is opened.")]
        public bool requireShopOpen;
        [Tooltip("If true, step completes when a weapon is bought.")]
        public bool requireWeaponBuy;
        [Tooltip("If true, step completes when a weapon is spawned/grabbed.")]
        public bool requireWeaponSpawn;

        [HideInInspector] public bool completed;
    }

    [Header("Tutorial Steps")]
    public List<TutorialStep> steps = new List<TutorialStep>();

    [Header("UI References")]
    public TextMeshPro titleText;
    public TextMeshPro descriptionText;
    public TextMeshPro stepCounterText;
    public TextMeshPro gestureResultText;

    [Header("Completion UI (auto-created if null)")]
    public GameObject completionCanvas;
    public TextMeshPro completionStatusText;

    [Header("References")]
    public PoseGestureDetector gestureDetector;
    public GestureActionHandler gestureHandler;
    public VRShopZone shopZone;
    public WeaponShopManager weaponShop;

    [Header("Scene Indices")]
    [Tooltip("Build index of the main game scene (shop-training).")]
    public int mainGameSceneIndex = 1;

    [Header("Audio (optional)")]
    public AudioSource stepSound;

    private int _currentStep = -1;
    private bool _stepComplete;
    private bool _shopEntered;
    private bool _shopOpened;
    private bool _weaponBought;
    private bool _weaponSpawned;
    private string _lastGesture;
    private bool _tutorialFinished;
    private GameObject _goToGameButton;

    void Start()
    {
        if (gestureDetector == null)
            gestureDetector = FindObjectOfType<PoseGestureDetector>();
        if (gestureHandler == null)
            gestureHandler = FindObjectOfType<GestureActionHandler>();
        if (shopZone == null)
            shopZone = FindObjectOfType<VRShopZone>();
        if (weaponShop == null)
            weaponShop = FindObjectOfType<WeaponShopManager>();

        if (gestureDetector != null)
            gestureDetector.OnGestureRecognized += OnGestureRecognized;

        if (steps.Count == 0)
            BuildDefaultSteps();

        CreateCompletionUI();
        StartCoroutine(RunTutorial());
    }

    void OnDestroy()
    {
        if (gestureDetector != null)
            gestureDetector.OnGestureRecognized -= OnGestureRecognized;
    }

    void OnGestureRecognized(string gestureName)
    {
        _lastGesture = gestureName;
        if (gestureResultText != null)
            gestureResultText.text = "<color=#FFD700>Detected:</color> <b>" + gestureName + "</b>";
    }

    public void NotifyShopEntered() { _shopEntered = true; }
    public void NotifyShopOpened() { _shopOpened = true; }
    public void NotifyWeaponBought() { _weaponBought = true; }
    public void NotifyWeaponSpawned() { _weaponSpawned = true; }

    IEnumerator RunTutorial()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < steps.Count; i++)
        {
            _currentStep = i;
            _stepComplete = false;
            _lastGesture = null;

            ShowStep(steps[i]);
            UpdateCompletionStatus();

            if (stepSound != null) stepSound.Play();

            if (steps[i].autoAdvanceDelay > 0 && string.IsNullOrEmpty(steps[i].requiredGesture)
                && !steps[i].requireShopZoneEntry && !steps[i].requireShopOpen
                && !steps[i].requireWeaponBuy && !steps[i].requireWeaponSpawn)
            {
                yield return new WaitForSeconds(steps[i].autoAdvanceDelay);
                steps[i].completed = true;
            }
            else
            {
                yield return StartCoroutine(WaitForStepCompletion(steps[i]));
                steps[i].completed = true;
            }

            UpdateCompletionStatus();
            yield return new WaitForSeconds(0.5f);
        }

        _tutorialFinished = true;
        ShowComplete();
        UpdateCompletionStatus();
    }

    IEnumerator WaitForStepCompletion(TutorialStep step)
    {
        while (!_stepComplete)
        {
            if (!string.IsNullOrEmpty(step.requiredGesture) && _lastGesture != null)
            {
                string norm = step.requiredGesture.ToLowerInvariant().Trim();
                string lastNorm = _lastGesture.ToLowerInvariant().Trim();
                if (lastNorm.Contains(norm) || norm.Contains(lastNorm))
                {
                    _stepComplete = true;
                    ShowSuccess(step.requiredGesture);
                }
            }

            if (step.requireShopZoneEntry && _shopEntered) _stepComplete = true;
            if (step.requireShopOpen && _shopOpened) _stepComplete = true;
            if (step.requireWeaponBuy && _weaponBought) _stepComplete = true;
            if (step.requireWeaponSpawn && _weaponSpawned) _stepComplete = true;

            yield return null;
        }
    }

    public void SkipStep()
    {
        _stepComplete = true;
    }

    public void GoToMainGame()
    {
        if (SceneTransitionManager.singleton != null)
            SceneTransitionManager.singleton.GoToSceneAsync(mainGameSceneIndex);
        else
            SceneManager.LoadScene(mainGameSceneIndex);
    }

    void ShowStep(TutorialStep step)
    {
        if (titleText != null)
            titleText.text = step.title;
        if (descriptionText != null)
            descriptionText.text = step.description;
        if (stepCounterText != null)
            stepCounterText.text = "Step " + (_currentStep + 1) + " / " + steps.Count;
        if (gestureResultText != null)
            gestureResultText.text = "";
    }

    void ShowSuccess(string gesture)
    {
        if (gestureResultText != null)
            gestureResultText.text = "<color=#00FF88>Done! " + gesture + " successful!</color>";
    }

    void ShowComplete()
    {
        if (titleText != null)
            titleText.text = "Tutorial Complete!";
        if (descriptionText != null)
            descriptionText.text = "You've learned all the basics!\n\nYou know how to:\n- Rapier (thrust forward)\n- Split (chop down)\n- Block (guard with both hands)\n- Buy weapons from the shop\n\nGood luck, warrior!";
        if (stepCounterText != null)
            stepCounterText.text = "DONE";
        if (gestureResultText != null)
            gestureResultText.text = "";
    }

    // ────────────────────────────────────────────────────────────────────────
    //  COMPLETION TRACKING UI
    // ────────────────────────────────────────────────────────────────────────

    void CreateCompletionUI()
    {
        if (completionCanvas != null) return;

        // Create a 3D world-space panel to the left of the tutorial board
        completionCanvas = new GameObject("CompletionPanel");
        completionCanvas.transform.position = new Vector3(-3f, 1.8f, 3.8f);
        completionCanvas.transform.rotation = Quaternion.Euler(0f, 160f, 0f);

        // Background quad
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "BG";
        bg.transform.SetParent(completionCanvas.transform);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.01f);
        bg.transform.localRotation = Quaternion.identity;
        bg.transform.localScale = new Vector3(2.2f, 2.8f, 1f);
        Destroy(bg.GetComponent<Collider>());
        var bgR = bg.GetComponent<Renderer>();
        Material bgMat = new Material(Shader.Find("Standard"));
        bgMat.SetFloat("_Mode", 3f);
        bgMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        bgMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        bgMat.SetInt("_ZWrite", 0);
        bgMat.DisableKeyword("_ALPHATEST_ON");
        bgMat.EnableKeyword("_ALPHABLEND_ON");
        bgMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        bgMat.renderQueue = 3000;
        bgMat.color = new Color(0.04f, 0.04f, 0.10f, 0.9f);
        bgR.material = bgMat;

        // Title
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(completionCanvas.transform);
        titleGO.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        titleGO.transform.localRotation = Quaternion.identity;
        TextMeshPro titleTmp = titleGO.AddComponent<TextMeshPro>();
        titleTmp.text = "Progress";
        titleTmp.fontSize = 0.8f;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = new Color(1f, 0.85f, 0.3f);
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.rectTransform.sizeDelta = new Vector2(2f, 0.4f);

        // Status text (checklist)
        GameObject statusGO = new GameObject("StatusText");
        statusGO.transform.SetParent(completionCanvas.transform);
        statusGO.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        statusGO.transform.localRotation = Quaternion.identity;
        completionStatusText = statusGO.AddComponent<TextMeshPro>();
        completionStatusText.text = "Loading...";
        completionStatusText.fontSize = 0.42f;
        completionStatusText.color = new Color(0.85f, 0.88f, 0.95f);
        completionStatusText.alignment = TextAlignmentOptions.Left;
        completionStatusText.rectTransform.sizeDelta = new Vector2(1.9f, 1.8f);
        completionStatusText.enableWordWrapping = true;
        completionStatusText.richText = true;

        // "Go to Main Game" button — world-space canvas
        GameObject btnCanvasGO = new GameObject("GoToGameCanvas");
        btnCanvasGO.transform.SetParent(completionCanvas.transform);
        btnCanvasGO.transform.localPosition = new Vector3(0f, -1.05f, -0.02f);
        btnCanvasGO.transform.localRotation = Quaternion.identity;
        Canvas btnCanvas = btnCanvasGO.AddComponent<Canvas>();
        btnCanvas.renderMode = RenderMode.WorldSpace;
        btnCanvasGO.AddComponent<CanvasScaler>();
        btnCanvasGO.AddComponent<GraphicRaycaster>();
        RectTransform canvasRT = btnCanvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(300, 80);
        canvasRT.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        // Button
        GameObject btnGO = new GameObject("GoToGameButton");
        btnGO.transform.SetParent(btnCanvasGO.transform, false);
        RectTransform btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(280, 60);
        btnRT.anchoredPosition = Vector2.zero;
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.1f, 0.55f, 0.2f);
        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(GoToMainGame);

        // Button text
        GameObject btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        RectTransform btnTextRT = btnTextGO.AddComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;
        TextMeshProUGUI btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnTMP.text = "Go to Main Game";
        btnTMP.fontSize = 24;
        btnTMP.fontStyle = FontStyles.Bold;
        btnTMP.color = Color.white;
        btnTMP.alignment = TextAlignmentOptions.Center;

        _goToGameButton = btnCanvasGO;

        UpdateCompletionStatus();
    }

    void UpdateCompletionStatus()
    {
        if (completionStatusText == null) return;

        string status = "";
        string[] labels = { "Welcome", "Rapier", "Rapier Done", "Split", "Split Done", "Block", "Block Done", "Shop", "Complete" };

        for (int i = 0; i < steps.Count; i++)
        {
            string label = i < labels.Length ? labels[i] : ("Step " + (i + 1));

            // Only show key steps in the checklist (skip the "success" transition steps)
            bool isKeyStep = false;
            string keyLabel = "";

            switch (i)
            {
                case 0: isKeyStep = true; keyLabel = "Welcome"; break;
                case 1: isKeyStep = true; keyLabel = "Rapier Gesture"; break;
                case 3: isKeyStep = true; keyLabel = "Split Gesture"; break;
                case 5: isKeyStep = true; keyLabel = "Block Gesture"; break;
                case 7: isKeyStep = true; keyLabel = "Visit Shop"; break;
            }

            if (!isKeyStep) continue;

            if (steps[i].completed)
                status += "<color=#00FF88>[DONE]</color> " + keyLabel + "\n";
            else if (i == _currentStep)
                status += "<color=#FFD700>[NOW]</color>  " + keyLabel + "\n";
            else
                status += "<color=#666666>[    ]</color>  " + keyLabel + "\n";
        }

        int doneCount = 0;
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i].completed) doneCount++;
        }

        status += "\n<size=80%>" + doneCount + " / " + steps.Count + " steps done</size>";

        completionStatusText.text = status;

        // Show the Go to Game button always (player can skip)
        if (_goToGameButton != null)
            _goToGameButton.SetActive(true);
    }

    // ────────────────────────────────────────────────────────────────────────
    void BuildDefaultSteps()
    {
        steps = new List<TutorialStep>
        {
            new TutorialStep
            {
                title = "Welcome, Warrior!",
                description = "Welcome to the training grounds!\n\nI'll teach you everything you need to survive out there.\n\nLook around and get comfortable with your surroundings.",
                autoAdvanceDelay = 6f
            },
            new TutorialStep
            {
                title = "Lesson 1: Rapier (Thrust)",
                description = "The <color=#66E6FF>Rapier</color> is a forward thrust attack.\n\n<color=#FFD700>How to do it:</color>\nExtend your arm and thrust your controller forward quickly, like a fencing lunge!\n\nThis fires a piercing projectile that deals heavy damage.\n\n<color=#FF6666>Try it now!</color> Thrust forward with either hand.",
                requiredGesture = "rapier"
            },
            new TutorialStep
            {
                title = "Excellent Thrust!",
                description = "Great job! The Rapier deals <color=#FF6666>60 damage</color> and shoots a fast projectile.\n\nUse it to hit enemies from a distance.\n\nLet's learn the next move...",
                autoAdvanceDelay = 5f
            },
            new TutorialStep
            {
                title = "Lesson 2: Split (Chop)",
                description = "The <color=#FFF033>Split</color> is a powerful downward chop.\n\n<color=#FFD700>How to do it:</color>\nRaise your hand up, then swing it DOWN hard, like chopping wood!\n\nThis creates a cone of damage in front of you.\n\n<color=#FF6666>Try it now!</color> Chop downward with either hand.",
                requiredGesture = "split"
            },
            new TutorialStep
            {
                title = "Powerful Chop!",
                description = "Well done! The Split deals <color=#FF6666>45 damage</color> in a cone area.\n\nIt's great for hitting enemies right in front of you.\n\nOne more combat move to learn...",
                autoAdvanceDelay = 5f
            },
            new TutorialStep
            {
                title = "Lesson 3: Block (Guard)",
                description = "The <color=#4DB8FF>Block</color> makes you invulnerable!\n\n<color=#FFD700>How to do it:</color>\nBring BOTH controllers together in front of your face, like holding up a shield!\n\nKeep them close together and raised.\n\nA shield will appear when you're blocking.\n\n<color=#FF6666>Try it now!</color> Guard with both hands!",
                requiredGesture = "block"
            },
            new TutorialStep
            {
                title = "Perfect Defense!",
                description = "Excellent! While blocking, you take <color=#00FF88>ZERO damage</color>.\n\nThe block lasts for 2.5 seconds after you trigger it.\n\nUse it when you see enemies about to attack!\n\nNow let's learn about the shop...",
                autoAdvanceDelay = 6f
            },
            new TutorialStep
            {
                title = "Lesson 4: The Weapon Shop",
                description = "See the shop area nearby? Walk over to it!\n\nWhen you enter the shop zone, a button will appear to open the shop menu.\n\n<color=#FFD700>How to buy:</color>\n1. Walk into the shop zone\n2. Press the 'Open Shop' button\n3. Browse weapons and press 'Buy'\n4. Press 'Spawn' to summon your weapon\n5. Grab it with your hand!\n\nWeapons get stronger (and more expensive) as you go.\nYou earn coins by defeating monsters.\n\n<color=#FF6666>Walk to the shop zone now!</color>",
                requireShopZoneEntry = true
            },
            new TutorialStep
            {
                title = "Tutorial Complete!",
                description = "You've learned all the basics!\n\n<color=#FFD700>Quick Reference:</color>\n- <color=#66E6FF>Rapier</color>: Thrust forward = Ranged projectile\n- <color=#FFF033>Split</color>: Chop down = Cone damage\n- <color=#4DB8FF>Block</color>: Both hands up = Invulnerable\n- <color=#00FF88>Shop</color>: Walk in, Buy, Spawn, Grab\n\nGood luck out there, warrior!",
                autoAdvanceDelay = 0f
            }
        };
    }
}
