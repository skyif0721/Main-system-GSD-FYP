using UnityEngine;
using TMPro;

/// <summary>
/// Shop girl NPC that faces the player and shows dialogue text.
/// Fixes text backflip issue by ensuring text always faces the camera correctly.
/// Place in the shop scene near the shop zone.
/// </summary>
public class ShopGirlNPC : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "Shop Girl";
    public string[] dialogueLines = new string[]
    {
        "Welcome to my shop!",
        "What can I get for you today?",
        "We have the finest weapons!",
        "Come back anytime!"
    };

    [Header("Visual")]
    public float textHeight = 2.3f;
    public Color nameColor = new Color(1f, 0.7f, 0.9f);
    public Color textColor = Color.white;

    private TextMeshPro _nameText;
    private TextMeshPro _dialogueText;
    private float _dialogueTimer;
    private int _currentLine;
    private float _lineChangeInterval = 5f;

    void Start()
    {
        // Only create procedural body if no real model is attached
        Renderer[] existingRenderers = GetComponentsInChildren<Renderer>();
        bool hasRealModel = false;
        foreach (var r in existingRenderers)
        {
            if (r is SkinnedMeshRenderer || r is MeshRenderer)
            {
                hasRealModel = true;
                break;
            }
        }

        if (!hasRealModel)
            CreateNPCVisual();

        CreateDialogueUI();
    }

    void Update()
    {
        // Face camera (Y-axis only for NPC body)
        if (Camera.main != null)
        {
            Vector3 lookDir = Camera.main.transform.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }

        // Fix text facing - ensure text faces camera correctly (not backflipped)
        FixTextFacing(_nameText);
        FixTextFacing(_dialogueText);

        // Cycle dialogue
        _dialogueTimer += Time.deltaTime;
        if (_dialogueTimer >= _lineChangeInterval)
        {
            _dialogueTimer = 0f;
            _currentLine = (_currentLine + 1) % dialogueLines.Length;
            if (_dialogueText != null)
                _dialogueText.text = dialogueLines[_currentLine];
        }
    }

    void FixTextFacing(TextMeshPro tmp)
    {
        if (tmp == null || Camera.main == null) return;

        // Make text face the camera directly (billboard style)
        Vector3 toCam = Camera.main.transform.position - tmp.transform.position;
        if (toCam.sqrMagnitude > 0.001f)
        {
            // Use LookRotation facing TOWARD camera (text reads correctly)
            tmp.transform.rotation = Quaternion.LookRotation(-toCam, Vector3.up);
        }
    }

    void CreateNPCVisual()
    {
        // Simple NPC body (capsule)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0f, 1f, 0f);
        body.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);

        Material bodyMat = new Material(Shader.Find("Standard"));
        bodyMat.color = new Color(0.9f, 0.5f, 0.6f); // Pink dress
        body.GetComponent<Renderer>().material = bodyMat;

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0f, 2f, 0f);
        head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        Material headMat = new Material(Shader.Find("Standard"));
        headMat.color = new Color(1f, 0.85f, 0.75f); // Skin color
        head.GetComponent<Renderer>().material = headMat;

        // Hair
        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hair.name = "Hair";
        hair.transform.SetParent(transform);
        hair.transform.localPosition = new Vector3(0f, 2.15f, -0.05f);
        hair.transform.localScale = new Vector3(0.45f, 0.35f, 0.45f);

        Material hairMat = new Material(Shader.Find("Standard"));
        hairMat.color = new Color(0.3f, 0.15f, 0.1f); // Brown hair
        hair.GetComponent<Renderer>().material = hairMat;
    }

    void CreateDialogueUI()
    {
        // Name text
        GameObject nameGO = new GameObject("NameText");
        nameGO.transform.SetParent(transform);
        nameGO.transform.localPosition = new Vector3(0f, textHeight + 0.3f, 0f);
        _nameText = nameGO.AddComponent<TextMeshPro>();
        _nameText.text = npcName;
        _nameText.fontSize = 3f;
        _nameText.fontStyle = FontStyles.Bold;
        _nameText.color = nameColor;
        _nameText.alignment = TextAlignmentOptions.Center;
        _nameText.rectTransform.sizeDelta = new Vector2(3f, 0.5f);

        // Dialogue text
        GameObject dialogueGO = new GameObject("DialogueText");
        dialogueGO.transform.SetParent(transform);
        dialogueGO.transform.localPosition = new Vector3(0f, textHeight, 0f);
        _dialogueText = dialogueGO.AddComponent<TextMeshPro>();
        _dialogueText.text = dialogueLines.Length > 0 ? dialogueLines[0] : "";
        _dialogueText.fontSize = 2.5f;
        _dialogueText.color = textColor;
        _dialogueText.alignment = TextAlignmentOptions.Center;
        _dialogueText.rectTransform.sizeDelta = new Vector2(3f, 0.5f);
        _dialogueText.enableWordWrapping = true;
    }
}
