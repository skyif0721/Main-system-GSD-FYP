using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Economy system: When player reaches 1000 coins, they can buy a ticket
/// to the final boss fight. Shows a "Buy Ticket" button and a "Go to Final Boss" button.
/// Place in the shop/training scene.
/// </summary>
public class FinalBossTicketSystem : MonoBehaviour
{
    [Header("Settings")]
    public int ticketCost = 1000;
    public int finalBossSceneIndex = 3;

    [Header("UI (auto-created if null)")]
    public GameObject ticketPanel;
    public TextMeshPro statusText;
    public TextMeshPro coinCountText;

    private bool _hasTicket;
    private GameObject _buyButton;
    private GameObject _goButton;

    void Start()
    {
        _hasTicket = PlayerPrefs.GetInt("HasFinalBossTicket", 0) == 1;
        CreateTicketUI();
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    void CreateTicketUI()
    {
        if (ticketPanel != null) return;

        ticketPanel = new GameObject("FinalBossTicketPanel");
        ticketPanel.transform.position = new Vector3(3f, 1.5f, 3f);
        ticketPanel.transform.rotation = Quaternion.Euler(0f, 200f, 0f);

        // Background
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "BG";
        bg.transform.SetParent(ticketPanel.transform);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.01f);
        bg.transform.localRotation = Quaternion.identity;
        bg.transform.localScale = new Vector3(2f, 2.5f, 1f);
        Object.Destroy(bg.GetComponent<Collider>());
        Material bgMat = new Material(Shader.Find("Standard"));
        SetTransparent(bgMat, new Color(0.08f, 0.02f, 0.02f, 0.9f));
        bg.GetComponent<Renderer>().material = bgMat;

        // Title
        CreateTMPro(ticketPanel.transform, "Title", "FINAL BOSS",
            0.7f, new Vector3(0f, 0.9f, 0f), new Color(1f, 0.3f, 0.2f), FontStyles.Bold);

        // Coin count
        coinCountText = CreateTMPro(ticketPanel.transform, "CoinCount", "",
            0.5f, new Vector3(0f, 0.5f, 0f), new Color(1f, 0.85f, 0.2f), FontStyles.Bold);

        // Status
        statusText = CreateTMPro(ticketPanel.transform, "Status", "",
            0.4f, new Vector3(0f, 0.1f, 0f), Color.white, FontStyles.Normal);
        statusText.enableWordWrapping = true;
        statusText.rectTransform.sizeDelta = new Vector2(1.8f, 0.6f);

        // Button canvas
        GameObject btnCanvasGO = new GameObject("ButtonCanvas");
        btnCanvasGO.transform.SetParent(ticketPanel.transform);
        btnCanvasGO.transform.localPosition = new Vector3(0f, -0.5f, -0.02f);
        btnCanvasGO.transform.localRotation = Quaternion.identity;
        Canvas btnCanvas = btnCanvasGO.AddComponent<Canvas>();
        btnCanvas.renderMode = RenderMode.WorldSpace;
        btnCanvasGO.AddComponent<CanvasScaler>();
        btnCanvasGO.AddComponent<GraphicRaycaster>();
        RectTransform canvasRT = btnCanvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(300, 150);
        canvasRT.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        // Buy Ticket button
        _buyButton = CreateButton(btnCanvasGO.transform, "BuyTicketBtn",
            $"Buy Ticket ({ticketCost} coins)", new Vector2(0, 30),
            new Color(0.8f, 0.5f, 0.1f), BuyTicket);

        // Go to Final Boss button
        _goButton = CreateButton(btnCanvasGO.transform, "GoFinalBtn",
            "GO TO FINAL BOSS!", new Vector2(0, -30),
            new Color(0.7f, 0.1f, 0.1f), GoToFinalBoss);
    }

    void UpdateUI()
    {
        _hasTicket = PlayerPrefs.GetInt("HasFinalBossTicket", 0) == 1;

        if (coinCountText != null)
            coinCountText.text = $"Coins: {ShopManager.coins}";

        if (_hasTicket)
        {
            if (statusText != null)
                statusText.text = "<color=#00FF88>Ticket Purchased!</color>\nYou are ready for the final battle!";
            if (_buyButton != null) _buyButton.SetActive(false);
            if (_goButton != null) _goButton.SetActive(true);
        }
        else
        {
            bool canAfford = ShopManager.coins >= ticketCost;
            if (statusText != null)
            {
                if (canAfford)
                    statusText.text = $"<color=#FFD700>You have enough coins!</color>\nBuy a ticket to challenge the Final Boss!";
                else
                    statusText.text = $"Collect {ticketCost} coins to buy a ticket.\nNeed {ticketCost - ShopManager.coins} more coins.";
            }
            if (_buyButton != null)
            {
                _buyButton.SetActive(true);
                Button btn = _buyButton.GetComponent<Button>();
                if (btn != null) btn.interactable = canAfford;
                Image img = _buyButton.GetComponent<Image>();
                if (img != null) img.color = canAfford
                    ? new Color(0.8f, 0.5f, 0.1f)
                    : new Color(0.3f, 0.3f, 0.3f);
            }
            if (_goButton != null) _goButton.SetActive(false);
        }
    }

    public void BuyTicket()
    {
        if (ShopManager.coins < ticketCost) return;

        ShopManager.coins -= ticketCost;
        PlayerPrefs.SetInt("SavedCoins", ShopManager.coins);
        PlayerPrefs.SetInt("HasFinalBossTicket", 1);
        PlayerPrefs.Save();

        _hasTicket = true;

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();

        // Refresh shop displays
        ShopManager[] managers = FindObjectsOfType<ShopManager>();
        foreach (var sm in managers)
            sm.DisplayNumber(ShopManager.coins);

        Debug.Log("[FinalBossTicket] Ticket purchased!");
        UpdateUI();
    }

    public void GoToFinalBoss()
    {
        if (!_hasTicket) return;

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();

        if (SceneTransitionManager.singleton != null)
            SceneTransitionManager.singleton.GoToSceneAsync(finalBossSceneIndex);
        else
            SceneManager.LoadScene(finalBossSceneIndex);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    GameObject CreateButton(Transform parent, string name, string label,
        Vector2 pos, Color color, UnityEngine.Events.UnityAction action)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        RectTransform btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(280, 50);
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
        tmp.fontSize = 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btnGO;
    }

    TextMeshPro CreateTMPro(Transform parent, string name, string text,
        float fontSize, Vector3 localPos, Color color, FontStyles style)
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
        tmp.rectTransform.sizeDelta = new Vector2(1.8f, 0.4f);
        tmp.richText = true;
        return tmp;
    }

    void SetTransparent(Material mat, Color color)
    {
        mat.SetFloat("_Mode", 3f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        mat.color = color;
    }
}
